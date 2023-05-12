using System.Drawing;

namespace ParticleGame
{
    public static class Power
    {
        // Predefining hash set prevents excessive heap allocations.
        private static readonly HashSet<Point> seenPoints = new(500 * 500);
        private static readonly Queue<Point> pointQueue = new(500 * 500);

        public static void UpdateFieldPower(ParticleField field)
        {
            seenPoints.Clear();
            pointQueue.Clear();
            // Unpower all particles first
            for (int x = 0; x < 500; x++)
            {
                for (int y = 0; y < 500; y++)
                {
                    ParticleData data = field[x, y];
                    if (data.ParticleType == ParticleTypes.Types.Air)
                    {
                        continue;
                    }
                    ParticleTypes.Types particleType = data.ParticleType;
                    data.ParticleType = ParticleTypes.UnpoweredStates.GetValueOrDefault(particleType, particleType);
                    if (particleType != data.ParticleType)
                    {
                        // Particle type changed (i.e. became unpowered), update color to draw
                        field.UpdateColor(x, y);
                    }
                }
            }

            // From any power emitters, power connected particles
            for (int x = 0; x < 500; x++)
            {
                for (int y = 0; y < 500; y++)
                {
                    ParticleData data = field[x, y];
                    if (data.ParticleType != ParticleTypes.Types.Air
                        && ParticleTypes.EmitsPower.Contains(data.ParticleType))
                    {
                        TransmitPower(field, new Point(x, y));
                    }
                }
            }
        }

        private static void TransmitPower(ParticleField field, Point point)
        {
            // Add all points around power emitter to queue
            foreach (Point adj in ParticleGame.Adjacent)
            {
                Point newTarget = new(point.X + adj.X, point.Y + adj.Y);
                if (newTarget.X < 0 || newTarget.Y < 0 || newTarget.X >= 500 || newTarget.Y >= 500)
                {
                    continue;
                }
                pointQueue.Enqueue(newTarget);
            }

            while (pointQueue.TryDequeue(out Point powerPoint))
            {
                ParticleData data = field[powerPoint.X, powerPoint.Y];
                if (data.ParticleType == ParticleTypes.Types.Air)
                {
                    continue;
                }
                // If this particle has already been processed, move on
                if (!seenPoints.Add(powerPoint))
                {
                    continue;
                }

                ParticleTypes.Types particleType = data.ParticleType;
                data.ParticleType = ParticleTypes.PoweredStates.GetValueOrDefault(data.ParticleType, data.ParticleType);
                if (data.ParticleType != particleType)
                {
                    // Particle type changed (i.e. became powered), update color to draw
                    field.UpdateColor(powerPoint.X, powerPoint.Y);
                }

                if (ParticleTypes.ConductsPower.Contains(data.ParticleType))
                {
                    // If this particle conducts power, add surrounding particles to queue
                    foreach (Point adj in ParticleGame.Adjacent)
                    {
                        Point newTarget = new(powerPoint.X + adj.X, powerPoint.Y + adj.Y);
                        if (newTarget.X < 0 || newTarget.Y < 0 || newTarget.X >= 500 || newTarget.Y >= 500)
                        {
                            continue;
                        }
                        pointQueue.Enqueue(newTarget);
                    }
                }
            }
        }
    }
}

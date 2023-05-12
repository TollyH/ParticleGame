using System.Drawing;

namespace ParticleGame
{
    public static class Power
    {
        // Predefining hash set prevents excessive heap allocations.
        private static readonly HashSet<Point> seenPoints = new(500 * 500);
        private static readonly Queue<(Point, ParticleTypes.Types)> pointQueue = new(500 * 500);

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

            // From any non-conditional power emitters, power connected particles
            for (int x = 0; x < 500; x++)
            {
                for (int y = 0; y < 500; y++)
                {
                    ParticleData data = field[x, y];
                    if (data.ParticleType != ParticleTypes.Types.Air
                        && ParticleTypes.EmitsPower.Contains(data.ParticleType)
                        && !ParticleTypes.EmitsWhenUnpowered.Contains(data.ParticleType))
                    {
                        Point point = new(x, y);
                        // Add all points around power emitter to queue
                        foreach (Point adj in ParticleGame.Adjacent)
                        {
                            Point newTarget = new(point.X + adj.X, point.Y + adj.Y);
                            if (newTarget.X < 0 || newTarget.Y < 0 || newTarget.X >= 500 || newTarget.Y >= 500)
                            {
                                continue;
                            }
                            ParticleTypes.Types particleType = field[x, y].ParticleType;
                            // Emitters cannot directly power themselves
                            if (particleType != field[newTarget.X, newTarget.Y].ParticleType)
                            {
                                pointQueue.Enqueue((newTarget, particleType));
                            }
                            else
                            {
                                _ = seenPoints.Add(newTarget);
                            }
                        }
                    }
                }
            }
            TransmitPower(field);

            // Second pass for conditional power emitters
            for (int x = 0; x < 500; x++)
            {
                for (int y = 0; y < 500; y++)
                {
                    ParticleData data = field[x, y];
                    if (data.ParticleType != ParticleTypes.Types.Air
                        && ParticleTypes.EmitsWhenUnpowered.Contains(data.ParticleType))
                    {
                        Point point = new(x, y);
                        // Add all points around power emitter to queue
                        foreach (Point adj in ParticleGame.Adjacent)
                        {
                            Point newTarget = new(point.X + adj.X, point.Y + adj.Y);
                            if (newTarget.X < 0 || newTarget.Y < 0 || newTarget.X >= 500 || newTarget.Y >= 500)
                            {
                                continue;
                            }
                            ParticleTypes.Types particleType = field[x, y].ParticleType;
                            // Emitters cannot directly power themselves
                            if (particleType != field[newTarget.X, newTarget.Y].ParticleType)
                            {
                                pointQueue.Enqueue((newTarget, particleType));
                            }
                            else
                            {
                                _ = seenPoints.Add(newTarget);
                            }
                        }
                    }
                }
            }
            TransmitPower(field);
        }

        private static void TransmitPower(ParticleField field)
        {
            while (pointQueue.TryDequeue(out (Point, ParticleTypes.Types) value))
            {
                Point powerPoint = value.Item1;
                ParticleTypes.Types previousType = value.Item2;
                ParticleData currentData = field[powerPoint.X, powerPoint.Y];
                if (currentData.ParticleType == ParticleTypes.Types.Air)
                {
                    continue;
                }

                ParticleTypes.Types particleType = currentData.ParticleType;

                // Power source is conditional, but this particle only conducts non-conditional power
                if (ParticleTypes.EmitsWhenUnpowered.Contains(previousType)
                    && ParticleTypes.ConductsUnconditionalPower.Contains(particleType))
                {
                    continue;
                }

                // If this particle has already been processed, move on
                if (!seenPoints.Add(powerPoint))
                {
                    continue;
                }

                currentData.ParticleType = ParticleTypes.PoweredStates.GetValueOrDefault(currentData.ParticleType, currentData.ParticleType);
                if (currentData.ParticleType != particleType)
                {
                    // Particle type changed (i.e. became powered), update color to draw
                    field.UpdateColor(powerPoint.X, powerPoint.Y);
                }

                bool isConductive = ParticleTypes.ConductsPower.Contains(currentData.ParticleType);
                foreach (Point adj in ParticleGame.Adjacent)
                {
                    Point newTarget = new(powerPoint.X + adj.X, powerPoint.Y + adj.Y);
                    if (newTarget.X < 0 || newTarget.Y < 0 || newTarget.X >= 500 || newTarget.Y >= 500)
                    {
                        continue;
                    }
                    // If this particle conducts power, add any surrounding particles to queue,
                    // otherwise only add particles of the same type
                    ParticleTypes.Types newType = field[newTarget.X, newTarget.Y].ParticleType;
                    if ((isConductive || newType == particleType)
                        && (!ParticleTypes.WillNotPowerEmitters.Contains(particleType) || !ParticleTypes.EmitsPower.Contains(newType)))
                    {
                        pointQueue.Enqueue((newTarget, particleType));
                    }
                }
            }
        }
    }
}

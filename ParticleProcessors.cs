using System.Drawing;

namespace ParticleGame
{
    public static class ParticleProcessors
    {
        public delegate Point Processor(Point position, ParticleField field, ParticleData data);

        // These are definitions of possible direction offsets that need to be randomised by processors.
        // Having them pre-defined prevents unnecessary object allocations when the processors are executed.
        private static readonly int[] leftRight = new int[] { 1, -1 };
        private static readonly int[] leftRightInverse = new int[] { -1, 1 };
        private static readonly int[][] leftCenterRightCombos = new int[][]
        {
            new int[] { -1, 0, 1 },
            new int[] { -1, 1, 0 },
            new int[] { 0, -1, 1 },
            new int[] { 0, 1, -1 },
            new int[] { 1, -1, 0 },
            new int[] { 1, 0, -1 }
        };

        public static Point ProcessorWater(Point position, ParticleField field, ParticleData data)
        {
            Point newPos = position;
            for (int i = 0; i < ParticleGame.RNG.Next(1, 3); i++)
            {
                if (newPos.Y == 500 - 1)
                {
                    // Destroy particle if bottom reached
                    newPos = new Point(-1, -1);
                    break;
                }
                else
                {
                    ParticleTypes.Types newType = field[newPos.X, newPos.Y + 1].ParticleType;
                    if (newType != data.ParticleType && ParticleTypes.Fluids.Contains(newType))
                    {
                        // Fall down if possible
                        newPos = new Point(newPos.X, newPos.Y + 1);
                    }
                    else
                    {
                        for (int j = 0; j < ParticleGame.RNG.Next(3, 6); j++)
                        {
                            if (newType != data.ParticleType && ParticleTypes.Fluids.Contains(newType))
                            {
                                break;
                            }
                            // Try to move to either side, with a higher chance of moving in the same direction
                            int lastX = position.X - data.PreviousPosition.X;
                            if (Math.Abs(lastX) != 1)
                            {
                                lastX = 1;
                            }
                            int[] sides = ParticleGame.RNG.NextDouble() <= 0.75 ? leftRight : leftRightInverse;
                            foreach (int dx in sides)
                            {
                                int newX = newPos.X + (dx * lastX);
                                if (newX is < 0 or >= 500)
                                {
                                    // Destroy particle if edge reached
                                    newPos = new Point(-1, -1);
                                    break;
                                }
                                ParticleTypes.Types sideType = field[newX, newPos.Y].ParticleType;
                                if (sideType != data.ParticleType && ParticleTypes.Fluids.Contains(sideType))
                                {
                                    newPos = new Point(newX, newPos.Y);
                                    break;
                                }
                            }
                            if (newPos == new Point(-1, -1))
                            {
                                break;
                            }
                        }
                        if (newPos == new Point(-1, -1))
                        {
                            break;
                        }
                    }
                }
            }
            return newPos;
        }

        public static Point ProcessorSand(Point position, ParticleField field, ParticleData data)
        {
            Point newPos = position;
            if (newPos.Y == 500 - 1)
            {
                // Destroy particle if bottom reached
                newPos = new Point(-1, -1);
            }
            else
            {
                ParticleTypes.Types newType = field[newPos.X, newPos.Y + 1].ParticleType;
                if (newType != data.ParticleType && ParticleTypes.Fluids.Contains(newType))
                {
                    // Fall down if possible
                    newPos = new Point(newPos.X, newPos.Y + 1);
                }
                else
                {
                    // Try to move to either diagonal downward, with an equal chance of either direction
                    int[] sides = ParticleGame.RNG.NextDouble() <= 0.5 ? leftRight : leftRightInverse;
                    foreach (int dx in sides)
                    {
                        int newX = newPos.X + dx;
                        int newY = newPos.Y + 1;
                        if (newX < 0 || newX >= 500 || newY >= 500)
                        {
                            // Destroy particle if edge reached
                            newPos = new Point(-1, -1);
                            break;
                        }
                        ParticleTypes.Types sideType = field[newX, newY].ParticleType;
                        if (sideType != data.ParticleType && ParticleTypes.Fluids.Contains(sideType))
                        {
                            newPos = new Point(newX, newY);
                            break;
                        }
                    }
                }
            }
            return newPos;
        }

        public static Point ProcessorSteam(Point position, ParticleField field, ParticleData data)
        {
            if (data.Age >= 5)
            {
                // Turn back to water if over 5 seconds old
                field[position.X, position.Y].ParticleType = ParticleTypes.Types.Water;
                return position;
            }
            Point newPos = position;
            if (newPos.Y == 0)
            {
                // Destroy particle if top reached
                newPos = new Point(-1, -1);
            }
            else
            {
                // Try and move in random upwards direction
                int[] xOffsets = leftCenterRightCombos[ParticleGame.RNG.Next(leftCenterRightCombos.Length)];
                foreach (int dx in xOffsets)
                {
                    int newX = position.X + dx;
                    if (newX is < 0 or >= 500)
                    {
                        // Destroy particle if edge reached
                        newPos = new Point(-1, -1);
                        break;
                    }
                    Point tryPos = new(newX, position.Y - 1);
                    ParticleTypes.Types tryType = field[tryPos.X, tryPos.Y].ParticleType;
                    if (tryType != data.ParticleType && ParticleTypes.Fluids.Contains(tryType))
                    {
                        newPos = tryPos;
                        break;
                    }
                }
            }
            // Not moved yet
            if (newPos == position)
            {
                // Try and move to a random side
                int[] sides = ParticleGame.RNG.NextDouble() <= 0.5 ? leftRight : leftRightInverse;
                foreach (int dx in sides)
                {
                    int newX = newPos.X + dx;
                    if (newX is < 0 or >= 500)
                    {
                        // Destroy particle if edge reached
                        newPos = new Point(-1, -1);
                        break;
                    }
                    Point tryPos = new(newX, position.Y);
                    ParticleTypes.Types tryType = field[tryPos.X, tryPos.Y].ParticleType;
                    if (tryType != data.ParticleType && ParticleTypes.Fluids.Contains(tryType))
                    {
                        newPos = tryPos;
                        break;
                    }
                }
            }
            return newPos;
        }

        public static readonly Dictionary<ParticleTypes.Types, Processor> Processors = new()
        {
            { ParticleTypes.Types.Water, new(ProcessorWater) },
            { ParticleTypes.Types.Sand, new(ProcessorSand) },
            { ParticleTypes.Types.RedSand, new(ProcessorSand) },
            { ParticleTypes.Types.Lava, new(ProcessorWater) },
            { ParticleTypes.Types.Steam, new(ProcessorSteam) }
        };
    }
}

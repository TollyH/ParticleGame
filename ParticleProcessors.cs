using System.Collections.Immutable;
using System.Drawing;

namespace ParticleGame
{
    public static class ParticleProcessors
    {
        public delegate Point Processor(Point position, ParticleData[,] field, ParticleData data);

        public static Point ProcessorWater(Point position, ParticleData[,] field, ParticleData data)
        {
            Point newPos = position;
            for (int i = 0; i < ParticleGame.RNG.Next(1, 3); i++)
            {
                if (newPos.Y == field.GetLength(1) - 1)
                {
                    // Destroy particle if bottom reached
                    newPos = new Point(-1, -1);
                    break;
                }
                else if (field[newPos.X, newPos.Y + 1].ParticleType == ParticleTypes.Types.Air)
                {
                    // Fall down if possible
                    newPos = new Point(newPos.X, newPos.Y + 1);
                }
                else
                {
                    for (int j = 0; j < ParticleGame.RNG.Next(3, 6); j++)
                    {
                        if (field[newPos.X, newPos.Y + 1].ParticleType == ParticleTypes.Types.Air)
                        {
                            break;
                        }
                        // Try to move to either side, with a higher chance of moving in the same direction
                        int lastX = position.X - data.PreviousPosition.X;
                        if (Math.Abs(lastX) != 1)
                        {
                            lastX = 1;
                        }
                        int[] sides = ParticleGame.RNG.NextDouble() <= 0.75 ? new int[] { lastX, -lastX } : new int[] { -lastX, lastX };
                        foreach (int dx in sides)
                        {
                            int newX = newPos.X + dx;
                            if (newX < 0 || newX >= field.GetLength(0))
                            {
                                // Destroy particle if edge reached
                                newPos = new Point(-1, -1);
                                break;
                            }
                            if (field[newX, newPos.Y].ParticleType == ParticleTypes.Types.Air)
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
            return newPos;
        }

        public static Point ProcessorSand(Point position, ParticleData[,] field, ParticleData data)
        {
            Point newPos = position;
            if (newPos.Y == field.GetLength(1) - 1)
            {
                // Destroy particle if bottom reached
                newPos = new Point(-1, -1);
            }
            else if (field[newPos.X, newPos.Y + 1].ParticleType == ParticleTypes.Types.Air)
            {
                // Fall down if possible
                newPos = new Point(newPos.X, newPos.Y + 1);
            }
            else
            {
                // Try to move to either diagonal downward, with an equal chance of either direction
                int[] sides = ParticleGame.RNG.NextDouble() <= 0.5 ? new int[] { 1, -1 } : new int[] {1, -1 };
                foreach (int dx in sides)
                {
                    int newX = newPos.X + dx;
                    int newY = newPos.Y + 1;
                    if (newX < 0 || newX >= field.GetLength(0) || newY >= field.GetLength(1))
                    {
                        // Destroy particle if edge reached
                        newPos = new Point(-1, -1);
                        break;
                    }
                    if (field[newX, newPos.Y].ParticleType == ParticleTypes.Types.Air)
                    {
                        newPos = new Point(newX, newY);
                        break;
                    }
                }
            }
            return newPos;
        }

        public static Point ProcessorSteam(Point position, ParticleData[,] field, ParticleData data)
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
                int[] xOffsets = new int[] { -1, 0, 1 };
                xOffsets = xOffsets.OrderBy(x => ParticleGame.RNG.Next()).ToArray();
                foreach (int dx in xOffsets)
                {
                    int newX = position.X + dx;
                    if (newX < 0 || newX >= field.GetLength(0))
                    {
                        // Destroy particle if edge reached
                        position = new Point(-1, -1);
                        break;
                    }
                    Point tryPos = new(newX, position.Y - 1);
                    if (field[tryPos.X, tryPos.Y].ParticleType is not ParticleTypes.Types.Steam and not ParticleTypes.Types.Block)
                    {
                        position = tryPos;
                        break;
                    }
                }
            }
            // Not moved yet
            if (newPos == position)
            {
                // Try and move to a random side
                int[] sides = ParticleGame.RNG.NextDouble() <= 0.5 ? new int[] { 1, -1 } : new int[] { 1, -1 };
                foreach (int dx in sides)
                {
                    int newX = newPos.X + dx;
                    if (newX < 0 || newX >= field.GetLength(0))
                    {
                        // Destroy particle if edge reached
                        newPos = new Point(-1, -1);
                        break;
                    }
                    Point tryPos = new(newX, position.Y);
                    if (field[tryPos.X, tryPos.Y].ParticleType is not ParticleTypes.Types.Steam and not ParticleTypes.Types.Block)
                    {
                        newPos = tryPos;
                        break;
                    }
                }
            }
            return newPos;
        }

        public static readonly ImmutableDictionary<ParticleTypes.Types, Processor> Processors = new Dictionary<ParticleTypes.Types, Processor>()
        {
            { ParticleTypes.Types.Water, new(ProcessorWater) },
            { ParticleTypes.Types.Sand, new(ProcessorSand) },
            { ParticleTypes.Types.RedSand, new(ProcessorSand) },
            { ParticleTypes.Types.Lava, new(ProcessorWater) },
            { ParticleTypes.Types.Steam, new(ProcessorSteam) }
        }.ToImmutableDictionary();
    }
}

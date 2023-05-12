using System.Drawing;

namespace ParticleGame
{
    public static class FieldOperations
    {
        public static void InitialiseField(ParticleField field)
        {
            for (int x = 0; x < 500; x++)
            {
                for (int y = 0; y < 500; y++)
                {
                    field[x, y] = new ParticleData(ParticleTypes.Types.Air, new Point(x, y));
                }
            }
        }

        public static void BrushDraw(ParticleField field, Point position, ParticleTypes.Types particleType, int brushSize = 1,
            ParticleTypes.Types? filter = null)
        {
            float offset = brushSize / 2f;
            // Constrain to the boundaries of the field
            int startX = Math.Max(0, position.X - (int)Math.Floor(offset));
            int endX = Math.Min(500 - 1, position.X + (int)Math.Ceiling(offset));
            int startY = Math.Max(0, position.Y - (int)Math.Floor(offset));
            int endY = Math.Min(500 - 1, position.Y + (int)Math.Ceiling(offset));
            for (int x = startX; x < endX; x++)
            {
                for (int y = startY; y < endY; y++)
                {
                    if (filter is null || field[x, y].ParticleType == filter)
                    {
                        field[x, y] = new ParticleData(particleType, new Point(x, y));
                    }
                }
            }
            // Ensure that surrounding particles are awake
            for (int x = startX; x < endX; x++)
            {
                if (startY > 0)
                {
                    field[x, startY - 1].Awake = true;
                }
                if (startY < 500 - 1)
                {
                    field[x, startY + 1].Awake = true;
                }
            }
            for (int y = startY; y < startY; y++)
            {
                if (startX > 0)
                {
                    field[startX - 1, y].Awake = true;
                }
                if (startX < 500 - 1)
                {
                    field[startX + 1, y].Awake = true;
                }
            }
        }

        public static void BrushLine(ParticleField field, Point startPos, Point endPos, ParticleTypes.Types particleType,
            int brushSize = 1, ParticleTypes.Types? filter = null)
        {
            // Bresenham's line algorithm
            int x1 = startPos.X;
            int y1 = startPos.Y;
            int x2 = endPos.X;
            int y2 = endPos.Y;
            int dx = Math.Abs(x2 - x1);
            int dy = Math.Abs(y2 - y1);
            int sx = x1 < x2 ? 1 : -1;
            int sy = y1 < y2 ? 1 : -1;
            int err = dx - dy;
            while (true)
            {
                BrushDraw(field, new Point(x1, y1), particleType, brushSize, filter);
                if (x1 == x2 && y1 == y2)
                {
                    // End reached
                    break;
                }
                int e2 = err * 2;
                if (e2 > -dy)
                {
                    err -= dy;
                    x1 += sx;
                }
                if (e2 < dx)
                {
                    err += dx;
                    y1 += sy;
                }
            }
        }
    }
}

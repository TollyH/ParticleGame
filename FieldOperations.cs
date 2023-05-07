﻿using System.Drawing;

namespace ParticleGame
{
    public static class FieldOperations
    {
        public static void BrushDraw(ParticleData[,] field, Point position, ParticleTypes.Types particleType, int brushSize = 1)
        {
            int offset = brushSize / 2;
            // Constrain to the boundaries of the field
            int startX = Math.Max(0, position.X - offset);
            int endX = Math.Min(field.GetLength(0), position.X + offset);
            for (int x = startX; x < endX; x++)
            {
                // Constrain to the boundaries of the field
                int startY = Math.Max(0, position.Y - offset);
                int endY = Math.Min(field.GetLength(1), position.Y + offset);
                for (int y = startY; y < endY; y++)
                {
                    field[x, y] = new ParticleData(particleType, new Point(x, y));
                }
            }
        }

        public static void BrushLine(ParticleData[,] field, Point startPos, Point endPos, ParticleTypes.Types particleType,
            int brushSize = 1)
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
                BrushDraw(field, new Point(x1, y1), particleType, brushSize);
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

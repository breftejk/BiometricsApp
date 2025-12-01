using BiometricsApp.Core.Models;

namespace BiometricsApp.Algorithms.Drawing;

/// <summary>
/// Pencil drawing tool for freehand drawing on images
/// </summary>
public static class PencilTool
{
    /// <summary>
    /// Draw a single pixel on the image
    /// </summary>
    /// <param name="target">Target image to draw on</param>
    /// <param name="x">X coordinate</param>
    /// <param name="y">Y coordinate</param>
    /// <param name="color">Color to draw</param>
    public static void DrawPixel(Image target, int x, int y, Color color)
    {
        if (x >= 0 && x < target.Width && y >= 0 && y < target.Height)
        {
            target[x, y] = color;
        }
    }

    /// <summary>
    /// Draw a circle at the specified position (for brush-like strokes)
    /// </summary>
    /// <param name="target">Target image to draw on</param>
    /// <param name="centerX">Center X coordinate</param>
    /// <param name="centerY">Center Y coordinate</param>
    /// <param name="radius">Radius of the brush</param>
    /// <param name="color">Color to draw</param>
    public static void DrawCircle(Image target, int centerX, int centerY, int radius, Color color)
    {
        int radiusSquared = radius * radius;
        
        for (int dy = -radius; dy <= radius; dy++)
        {
            for (int dx = -radius; dx <= radius; dx++)
            {
                if (dx * dx + dy * dy <= radiusSquared)
                {
                    int x = centerX + dx;
                    int y = centerY + dy;
                    if (x >= 0 && x < target.Width && y >= 0 && y < target.Height)
                    {
                        target[x, y] = color;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Draw a line between two points using Bresenham's algorithm
    /// </summary>
    /// <param name="target">Target image to draw on</param>
    /// <param name="x0">Start X</param>
    /// <param name="y0">Start Y</param>
    /// <param name="x1">End X</param>
    /// <param name="y1">End Y</param>
    /// <param name="size">Brush size (1 = single pixel)</param>
    /// <param name="color">Color to draw</param>
    public static void DrawLine(Image target, int x0, int y0, int x1, int y1, int size, Color color)
    {
        int dx = Math.Abs(x1 - x0);
        int dy = Math.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;
        int err = dx - dy;

        while (true)
        {
            if (size <= 1)
            {
                DrawPixel(target, x0, y0, color);
            }
            else
            {
                DrawCircle(target, x0, y0, size / 2, color);
            }

            if (x0 == x1 && y0 == y1) break;

            int e2 = 2 * err;
            if (e2 > -dy)
            {
                err -= dy;
                x0 += sx;
            }
            if (e2 < dx)
            {
                err += dx;
                y0 += sy;
            }
        }
    }

    /// <summary>
    /// Draw a polyline (series of connected points)
    /// </summary>
    /// <param name="target">Target image to draw on</param>
    /// <param name="points">List of points to connect</param>
    /// <param name="size">Brush size</param>
    /// <param name="color">Color to draw</param>
    public static void DrawPolyline(Image target, List<(int x, int y)> points, int size, Color color)
    {
        if (points.Count == 0) return;
        
        if (points.Count == 1)
        {
            if (size <= 1)
                DrawPixel(target, points[0].x, points[0].y, color);
            else
                DrawCircle(target, points[0].x, points[0].y, size / 2, color);
            return;
        }

        for (int i = 0; i < points.Count - 1; i++)
        {
            DrawLine(target, points[i].x, points[i].y, points[i + 1].x, points[i + 1].y, size, color);
        }
    }

    /// <summary>
    /// Draw a rectangle outline
    /// </summary>
    /// <param name="target">Target image to draw on</param>
    /// <param name="x1">Top-left X</param>
    /// <param name="y1">Top-left Y</param>
    /// <param name="x2">Bottom-right X</param>
    /// <param name="y2">Bottom-right Y</param>
    /// <param name="size">Line thickness</param>
    /// <param name="color">Color to draw</param>
    public static void DrawRectangle(Image target, int x1, int y1, int x2, int y2, int size, Color color)
    {
        DrawLine(target, x1, y1, x2, y1, size, color); // Top
        DrawLine(target, x2, y1, x2, y2, size, color); // Right
        DrawLine(target, x2, y2, x1, y2, size, color); // Bottom
        DrawLine(target, x1, y2, x1, y1, size, color); // Left
    }

    /// <summary>
    /// Draw a filled rectangle
    /// </summary>
    /// <param name="target">Target image to draw on</param>
    /// <param name="x1">Top-left X</param>
    /// <param name="y1">Top-left Y</param>
    /// <param name="x2">Bottom-right X</param>
    /// <param name="y2">Bottom-right Y</param>
    /// <param name="color">Color to fill</param>
    public static void FillRectangle(Image target, int x1, int y1, int x2, int y2, Color color)
    {
        int minX = Math.Max(0, Math.Min(x1, x2));
        int maxX = Math.Min(target.Width - 1, Math.Max(x1, x2));
        int minY = Math.Max(0, Math.Min(y1, y2));
        int maxY = Math.Min(target.Height - 1, Math.Max(y1, y2));

        for (int y = minY; y <= maxY; y++)
        {
            for (int x = minX; x <= maxX; x++)
            {
                target[x, y] = color;
            }
        }
    }

    /// <summary>
    /// Create a copy of the source image for drawing
    /// </summary>
    /// <param name="source">Source image</param>
    /// <returns>Copy of the image</returns>
    public static Image CreateDrawingCanvas(Image source)
    {
        var canvas = new Image(source.Width, source.Height);
        for (int y = 0; y < source.Height; y++)
        {
            for (int x = 0; x < source.Width; x++)
            {
                canvas[x, y] = source[x, y];
            }
        }
        return canvas;
    }
}

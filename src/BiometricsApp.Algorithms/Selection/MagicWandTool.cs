using BiometricsApp.Core.Models;

namespace BiometricsApp.Algorithms.Selection;

/// <summary>
/// Magic Wand (Color Grouping) tool for selecting similar pixels.
/// Implements flood fill algorithm with customizable tolerance range.
/// </summary>
public static class MagicWandTool
{
    /// <summary>
    /// Selection mode for determining which pixels to include
    /// </summary>
    public enum SelectionMode
    {
        /// <summary>
        /// Standard flood fill - only connected pixels
        /// </summary>
        Contiguous,
        /// <summary>
        /// Global flood mode - all matching pixels regardless of connectivity
        /// </summary>
        Global
    }

    /// <summary>
    /// Connectivity mode for flood fill
    /// </summary>
    public enum Connectivity
    {
        /// <summary>
        /// 4-connectivity (up, down, left, right)
        /// </summary>
        Four,
        /// <summary>
        /// 8-connectivity (includes diagonals)
        /// </summary>
        Eight
    }

    /// <summary>
    /// Represents a selection mask for the image
    /// </summary>
    public class SelectionMask
    {
        public bool[,] Mask { get; }
        public int Width { get; }
        public int Height { get; }
        public int SelectedCount { get; private set; }

        public SelectionMask(int width, int height)
        {
            Width = width;
            Height = height;
            Mask = new bool[width, height];
            SelectedCount = 0;
        }

        public bool this[int x, int y]
        {
            get => Mask[x, y];
            set
            {
                if (Mask[x, y] != value)
                {
                    Mask[x, y] = value;
                    SelectedCount += value ? 1 : -1;
                }
            }
        }

        /// <summary>
        /// Creates a copy of the selection mask
        /// </summary>
        public SelectionMask Clone()
        {
            var clone = new SelectionMask(Width, Height);
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    clone.Mask[x, y] = Mask[x, y];
                }
            }
            clone.SelectedCount = SelectedCount;
            return clone;
        }
    }

    /// <summary>
    /// Select pixels using magic wand tool with contiguous (flood fill) mode
    /// </summary>
    /// <param name="source">Source image</param>
    /// <param name="startX">Starting X coordinate</param>
    /// <param name="startY">Starting Y coordinate</param>
    /// <param name="toleranceMin">Minimum tolerance value (0-255)</param>
    /// <param name="toleranceMax">Maximum tolerance value (0-255)</param>
    /// <param name="maxPixels">Maximum number of pixels to select (0 = unlimited)</param>
    /// <param name="connectivity">Connectivity mode for flood fill</param>
    /// <returns>Selection mask</returns>
    public static SelectionMask SelectContiguous(
        Image source,
        int startX,
        int startY,
        int toleranceMin,
        int toleranceMax,
        int maxPixels = 0,
        Connectivity connectivity = Connectivity.Four)
    {
        var mask = new SelectionMask(source.Width, source.Height);
        
        if (startX < 0 || startX >= source.Width || startY < 0 || startY >= source.Height)
            return mask;

        var targetPixel = source[startX, startY];
        byte targetR = targetPixel[0];
        byte targetG = targetPixel[1];
        byte targetB = targetPixel[2];

        var visited = new bool[source.Width, source.Height];
        var queue = new Queue<(int x, int y)>();
        queue.Enqueue((startX, startY));
        visited[startX, startY] = true;

        int[] dx4 = { 0, 1, 0, -1 };
        int[] dy4 = { -1, 0, 1, 0 };
        int[] dx8 = { 0, 1, 1, 1, 0, -1, -1, -1 };
        int[] dy8 = { -1, -1, 0, 1, 1, 1, 0, -1 };

        int[] dx = connectivity == Connectivity.Four ? dx4 : dx8;
        int[] dy = connectivity == Connectivity.Four ? dy4 : dy8;
        int directions = connectivity == Connectivity.Four ? 4 : 8;

        while (queue.Count > 0)
        {
            if (maxPixels > 0 && mask.SelectedCount >= maxPixels)
                break;

            var (x, y) = queue.Dequeue();

            // Check if this pixel matches the tolerance range
            var pixel = source[x, y];
            if (IsPixelInRange(targetR, targetG, targetB, pixel[0], pixel[1], pixel[2], toleranceMin, toleranceMax))
            {
                mask[x, y] = true;

                // Add neighbors to queue
                for (int i = 0; i < directions; i++)
                {
                    int nx = x + dx[i];
                    int ny = y + dy[i];

                    if (nx >= 0 && nx < source.Width && ny >= 0 && ny < source.Height && !visited[nx, ny])
                    {
                        visited[nx, ny] = true;
                        queue.Enqueue((nx, ny));
                    }
                }
            }
        }

        return mask;
    }

    /// <summary>
    /// Select pixels using magic wand tool with global mode (all matching pixels)
    /// </summary>
    /// <param name="source">Source image</param>
    /// <param name="startX">Starting X coordinate for reference color</param>
    /// <param name="startY">Starting Y coordinate for reference color</param>
    /// <param name="toleranceMin">Minimum tolerance value (0-255)</param>
    /// <param name="toleranceMax">Maximum tolerance value (0-255)</param>
    /// <param name="maxPixels">Maximum number of pixels to select (0 = unlimited)</param>
    /// <returns>Selection mask</returns>
    public static SelectionMask SelectGlobal(
        Image source,
        int startX,
        int startY,
        int toleranceMin,
        int toleranceMax,
        int maxPixels = 0)
    {
        var mask = new SelectionMask(source.Width, source.Height);
        
        if (startX < 0 || startX >= source.Width || startY < 0 || startY >= source.Height)
            return mask;

        var targetPixel = source[startX, startY];
        byte targetR = targetPixel[0];
        byte targetG = targetPixel[1];
        byte targetB = targetPixel[2];

        for (int y = 0; y < source.Height; y++)
        {
            for (int x = 0; x < source.Width; x++)
            {
                if (maxPixels > 0 && mask.SelectedCount >= maxPixels)
                    return mask;

                var pixel = source[x, y];
                if (IsPixelInRange(targetR, targetG, targetB, pixel[0], pixel[1], pixel[2], toleranceMin, toleranceMax))
                {
                    mask[x, y] = true;
                }
            }
        }

        return mask;
    }

    /// <summary>
    /// Apply flood fill to an image (similar to Paint bucket tool)
    /// </summary>
    /// <param name="source">Source image</param>
    /// <param name="startX">Starting X coordinate</param>
    /// <param name="startY">Starting Y coordinate</param>
    /// <param name="fillColor">Color to fill with</param>
    /// <param name="toleranceMin">Minimum tolerance value (0-255)</param>
    /// <param name="toleranceMax">Maximum tolerance value (0-255)</param>
    /// <param name="maxPixels">Maximum number of pixels to fill (0 = unlimited)</param>
    /// <param name="connectivity">Connectivity mode for flood fill</param>
    /// <param name="globalMode">Use global mode instead of contiguous</param>
    /// <returns>Modified image</returns>
    public static Image FloodFill(
        Image source,
        int startX,
        int startY,
        Color fillColor,
        int toleranceMin,
        int toleranceMax,
        int maxPixels = 0,
        Connectivity connectivity = Connectivity.Four,
        bool globalMode = false)
    {
        var result = new Image(source.Width, source.Height);
        
        // Copy source to result
        for (int y = 0; y < source.Height; y++)
        {
            for (int x = 0; x < source.Width; x++)
            {
                result[x, y] = source[x, y];
            }
        }

        SelectionMask mask;
        if (globalMode)
        {
            mask = SelectGlobal(source, startX, startY, toleranceMin, toleranceMax, maxPixels);
        }
        else
        {
            mask = SelectContiguous(source, startX, startY, toleranceMin, toleranceMax, maxPixels, connectivity);
        }

        // Apply fill color to selected pixels
        for (int y = 0; y < result.Height; y++)
        {
            for (int x = 0; x < result.Width; x++)
            {
                if (mask[x, y])
                {
                    result[x, y] = fillColor;
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Create a visualization of the selection mask
    /// </summary>
    /// <param name="source">Source image</param>
    /// <param name="mask">Selection mask</param>
    /// <param name="highlightColor">Color to highlight selected areas</param>
    /// <param name="opacity">Opacity of the highlight (0.0 - 1.0)</param>
    /// <returns>Image with selection highlighted</returns>
    public static Image VisualizeSelection(Image source, SelectionMask mask, Color highlightColor, double opacity = 0.5)
    {
        var result = new Image(source.Width, source.Height);
        
        for (int y = 0; y < source.Height; y++)
        {
            for (int x = 0; x < source.Width; x++)
            {
                var pixel = source[x, y];
                
                if (mask[x, y])
                {
                    // Blend with highlight color
                    byte r = (byte)(pixel[0] * (1 - opacity) + highlightColor.R * opacity);
                    byte g = (byte)(pixel[1] * (1 - opacity) + highlightColor.G * opacity);
                    byte b = (byte)(pixel[2] * (1 - opacity) + highlightColor.B * opacity);
                    result[x, y] = new byte[] { r, g, b };
                }
                else
                {
                    result[x, y] = pixel;
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Check if a pixel is within the tolerance range of the target color
    /// </summary>
    private static bool IsPixelInRange(
        byte targetR, byte targetG, byte targetB,
        byte pixelR, byte pixelG, byte pixelB,
        int toleranceMin, int toleranceMax)
    {
        // Calculate the color difference (using average of RGB differences)
        int diffR = Math.Abs(targetR - pixelR);
        int diffG = Math.Abs(targetG - pixelG);
        int diffB = Math.Abs(targetB - pixelB);
        int avgDiff = (diffR + diffG + diffB) / 3;

        return avgDiff >= toleranceMin && avgDiff <= toleranceMax;
    }
}

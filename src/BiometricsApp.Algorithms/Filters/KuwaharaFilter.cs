using BiometricsApp.Core.Models;

namespace BiometricsApp.Algorithms.Filters;

/// <summary>
/// Kuwahara filter - edge-preserving smoothing filter
/// Preserves edges while smoothing regions
/// </summary>
public static class KuwaharaFilter
{
    /// <summary>
    /// Apply Kuwahara filter
    /// </summary>
    /// <param name="source">Source image</param>
    /// <param name="windowSize">Size of the filter window (must be odd, typically 5)</param>
    public static Image Apply(Image source, int windowSize = 5)
    {
        if (windowSize % 2 == 0)
            throw new ArgumentException("Window size must be odd", nameof(windowSize));

        var result = new Image(source.Width, source.Height);
        int radius = windowSize / 2;

        for (int y = 0; y < source.Height; y++)
        {
            for (int x = 0; x < source.Width; x++)
            {
                // Define 4 overlapping regions (quadrants)
                var regions = new[]
                {
                    GetRegion(source, x, y, -radius, 0, -radius, 0),     // Top-left
                    GetRegion(source, x, y, 0, radius, -radius, 0),      // Top-right
                    GetRegion(source, x, y, -radius, 0, 0, radius),      // Bottom-left
                    GetRegion(source, x, y, 0, radius, 0, radius)        // Bottom-right
                };

                // Find region with minimum variance
                double minVariance = double.MaxValue;
                (byte r, byte g, byte b) meanColor = (0, 0, 0);

                foreach (var region in regions)
                {
                    double variance = CalculateVariance(region);
                    if (variance < minVariance)
                    {
                        minVariance = variance;
                        meanColor = CalculateMean(region);
                    }
                }

                result[x, y] = new byte[] { meanColor.r, meanColor.g, meanColor.b };
            }
        }

        return result;
    }

    /// <summary>
    /// Get pixel values from a region
    /// </summary>
    private static List<(byte r, byte g, byte b)> GetRegion(
        Image source, int centerX, int centerY,
        int xStart, int xEnd, int yStart, int yEnd)
    {
        var pixels = new List<(byte, byte, byte)>();

        for (int dy = yStart; dy <= yEnd; dy++)
        {
            for (int dx = xStart; dx <= xEnd; dx++)
            {
                int px = Math.Clamp(centerX + dx, 0, source.Width - 1);
                int py = Math.Clamp(centerY + dy, 0, source.Height - 1);

                var pixel = source[px, py];
                pixels.Add((pixel[0], pixel[1], pixel[2]));
            }
        }

        return pixels;
    }

    /// <summary>
    /// Calculate mean color of a region
    /// </summary>
    private static (byte r, byte g, byte b) CalculateMean(List<(byte r, byte g, byte b)> pixels)
    {
        if (pixels.Count == 0)
            return (0, 0, 0);

        long sumR = 0, sumG = 0, sumB = 0;
        foreach (var pixel in pixels)
        {
            sumR += pixel.r;
            sumG += pixel.g;
            sumB += pixel.b;
        }

        int count = pixels.Count;
        return (
            (byte)(sumR / count),
            (byte)(sumG / count),
            (byte)(sumB / count)
        );
    }

    /// <summary>
    /// Calculate variance of a region (combined RGB variance)
    /// </summary>
    private static double CalculateVariance(List<(byte r, byte g, byte b)> pixels)
    {
        if (pixels.Count == 0)
            return 0;

        var mean = CalculateMean(pixels);
        double variance = 0;

        foreach (var pixel in pixels)
        {
            double diffR = pixel.r - mean.r;
            double diffG = pixel.g - mean.g;
            double diffB = pixel.b - mean.b;

            // Combined variance for all channels
            variance += diffR * diffR + diffG * diffG + diffB * diffB;
        }

        return variance / pixels.Count;
    }

    /// <summary>
    /// Apply generalized Kuwahara filter with custom region size
    /// </summary>
    /// <param name="source">Source image</param>
    /// <param name="regionSize">Size of each quadrant region</param>
    public static Image ApplyGeneralized(Image source, int regionSize = 3)
    {
        var result = new Image(source.Width, source.Height);

        for (int y = 0; y < source.Height; y++)
        {
            for (int x = 0; x < source.Width; x++)
            {
                // Define 4 non-overlapping regions
                var regions = new[]
                {
                    GetRegion(source, x, y, -regionSize, -1, -regionSize, -1),  // Top-left
                    GetRegion(source, x, y, 0, regionSize-1, -regionSize, -1),  // Top-right
                    GetRegion(source, x, y, -regionSize, -1, 0, regionSize-1),  // Bottom-left
                    GetRegion(source, x, y, 0, regionSize-1, 0, regionSize-1)   // Bottom-right
                };

                // Find region with minimum variance
                double minVariance = double.MaxValue;
                (byte r, byte g, byte b) meanColor = (0, 0, 0);

                foreach (var region in regions)
                {
                    if (region.Count == 0) continue;

                    double variance = CalculateVariance(region);
                    if (variance < minVariance)
                    {
                        minVariance = variance;
                        meanColor = CalculateMean(region);
                    }
                }

                result[x, y] = new byte[] { meanColor.r, meanColor.g, meanColor.b };
            }
        }

        return result;
    }
}

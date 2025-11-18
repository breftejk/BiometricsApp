using BiometricsApp.Core.Models;

namespace BiometricsApp.Algorithms.Filters;

/// <summary>
/// Median filter for noise reduction
/// Supports arbitrary mask sizes
/// </summary>
public static class MedianFilter
{
    /// <summary>
    /// Apply median filter with specified window size
    /// </summary>
    /// <param name="source">Source image</param>
    /// <param name="windowSize">Size of the filter window (must be odd, e.g., 3, 5, 7)</param>
    public static Image Apply(Image source, int windowSize = 3)
    {
        if (windowSize % 2 == 0)
            throw new ArgumentException("Window size must be odd", nameof(windowSize));

        var result = new Image(source.Width, source.Height);
        int offset = windowSize / 2;

        for (int y = 0; y < source.Height; y++)
        {
            for (int x = 0; x < source.Width; x++)
            {
                List<byte> redValues = new();
                List<byte> greenValues = new();
                List<byte> blueValues = new();

                // Collect pixel values in the window
                for (int wy = -offset; wy <= offset; wy++)
                {
                    for (int wx = -offset; wx <= offset; wx++)
                    {
                        int px = Math.Clamp(x + wx, 0, source.Width - 1);
                        int py = Math.Clamp(y + wy, 0, source.Height - 1);

                        var pixel = source[px, py];
                        redValues.Add(pixel[0]);
                        greenValues.Add(pixel[1]);
                        blueValues.Add(pixel[2]);
                    }
                }

                // Find median for each channel
                result[x, y] = new byte[]
                {
                    GetMedian(redValues),
                    GetMedian(greenValues),
                    GetMedian(blueValues)
                };
            }
        }

        return result;
    }

    /// <summary>
    /// Calculate median value from a list
    /// </summary>
    private static byte GetMedian(List<byte> values)
    {
        values.Sort();
        int count = values.Count;
        if (count % 2 == 0)
            return (byte)((values[count / 2 - 1] + values[count / 2]) / 2);
        else
            return values[count / 2];
    }

    /// <summary>
    /// Apply median filter with rectangular window
    /// </summary>
    /// <param name="source">Source image</param>
    /// <param name="windowWidth">Width of the filter window (must be odd)</param>
    /// <param name="windowHeight">Height of the filter window (must be odd)</param>
    public static Image Apply(Image source, int windowWidth, int windowHeight)
    {
        if (windowWidth % 2 == 0 || windowHeight % 2 == 0)
            throw new ArgumentException("Window dimensions must be odd");

        var result = new Image(source.Width, source.Height);
        int offsetX = windowWidth / 2;
        int offsetY = windowHeight / 2;

        for (int y = 0; y < source.Height; y++)
        {
            for (int x = 0; x < source.Width; x++)
            {
                List<byte> redValues = new();
                List<byte> greenValues = new();
                List<byte> blueValues = new();

                // Collect pixel values in the window
                for (int wy = -offsetY; wy <= offsetY; wy++)
                {
                    for (int wx = -offsetX; wx <= offsetX; wx++)
                    {
                        int px = Math.Clamp(x + wx, 0, source.Width - 1);
                        int py = Math.Clamp(y + wy, 0, source.Height - 1);

                        var pixel = source[px, py];
                        redValues.Add(pixel[0]);
                        greenValues.Add(pixel[1]);
                        blueValues.Add(pixel[2]);
                    }
                }

                // Find median for each channel
                result[x, y] = new byte[]
                {
                    GetMedian(redValues),
                    GetMedian(greenValues),
                    GetMedian(blueValues)
                };
            }
        }

        return result;
    }
}

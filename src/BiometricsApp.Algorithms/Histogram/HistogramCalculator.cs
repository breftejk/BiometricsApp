using BiometricsApp.Core.Extensions;
using BiometricsApp.Core.Models;

namespace BiometricsApp.Algorithms.Histogram;

/// <summary>
/// Histogram calculation and display
/// </summary>
public static class HistogramCalculator
{
    /// <summary>
    /// Calculate histogram for all channels
    /// </summary>
    /// <param name="source">Source image</param>
    /// <returns>Array of histograms [R, G, B, Average]</returns>
    public static int[][] Calculate(Image source)
    {
        int[][] histogram = 
        [
            new int[256], // Red
            new int[256], // Green
            new int[256], // Blue
            new int[256]  // Average
        ];

        for (int x = 0; x < source.Width; x++)
        {
            for (int y = 0; y < source.Height; y++)
            {
                ++histogram[0][source[x, y, Channel.R]];
                ++histogram[1][source[x, y, Channel.G]];
                ++histogram[2][source[x, y, Channel.B]];
                ++histogram[3][(int)source[x, y].Average()];
            }
        }

        return histogram;
    }

    /// <summary>
    /// Create histogram visualization image
    /// </summary>
    public static Image CreateVisualization(int[] histogram, Color foreground, Color background, int height = 256)
    {
        return new Image(histogram, height, foreground, background);
    }

    /// <summary>
    /// Calculate histogram for a single channel
    /// </summary>
    public static int[] CalculateChannel(Image source, Channel channel)
    {
        int[] histogram = new int[256];

        for (int x = 0; x < source.Width; x++)
        {
            for (int y = 0; y < source.Height; y++)
            {
                ++histogram[source[x, y, channel]];
            }
        }

        return histogram;
    }
}


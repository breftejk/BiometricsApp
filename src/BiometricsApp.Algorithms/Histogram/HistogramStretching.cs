using BiometricsApp.Core.Models;

namespace BiometricsApp.Algorithms.Histogram;

/// <summary>
/// Histogram stretching (contrast stretching) algorithm
/// </summary>
public static class HistogramStretching
{
    /// <summary>
    /// Stretch histogram to specified range
    /// </summary>
    /// <param name="source">Source image</param>
    /// <param name="minOutput">Minimum output value (default: 0)</param>
    /// <param name="maxOutput">Maximum output value (default: 255)</param>
    /// <returns>Image with stretched histogram</returns>
    public static Image Apply(Image source, byte minOutput = 0, byte maxOutput = 255)
    {
        var result = new Image(source.Width, source.Height);

        // Find min and max values in the image
        byte minInput = 255;
        byte maxInput = 0;

        for (int x = 0; x < source.Width; x++)
        {
            for (int y = 0; y < source.Height; y++)
            {
                byte r = source[x, y, Channel.R];
                byte g = source[x, y, Channel.G];
                byte b = source[x, y, Channel.B];

                minInput = Math.Min(minInput, Math.Min(r, Math.Min(g, b)));
                maxInput = Math.Max(maxInput, Math.Max(r, Math.Max(g, b)));
            }
        }

        // If all pixels have the same value, return copy
        if (minInput == maxInput)
        {
            for (int x = 0; x < source.Width; x++)
                for (int y = 0; y < source.Height; y++)
                    result[x, y] = source[x, y];
            return result;
        }

        // Apply stretching formula: out = (in - min) * (maxOut - minOut) / (maxIn - minIn) + minOut
        double scale = (maxOutput - minOutput) / (double)(maxInput - minInput);

        for (int x = 0; x < source.Width; x++)
        {
            for (int y = 0; y < source.Height; y++)
            {
                byte r = source[x, y, Channel.R];
                byte g = source[x, y, Channel.G];
                byte b = source[x, y, Channel.B];

                byte newR = (byte)Math.Clamp((r - minInput) * scale + minOutput, 0, 255);
                byte newG = (byte)Math.Clamp((g - minInput) * scale + minOutput, 0, 255);
                byte newB = (byte)Math.Clamp((b - minInput) * scale + minOutput, 0, 255);

                result[x, y] = new byte[] { newR, newG, newB, 255 };
            }
        }

        return result;
    }
}

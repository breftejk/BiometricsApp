using BiometricsApp.Core.Models;

namespace BiometricsApp.Algorithms.Adjustments;

/// <summary>
/// Image adjustment algorithms for brightness and contrast
/// </summary>
public static class ImageAdjustments
{
    /// <summary>
    /// Adjust the brightness of an image
    /// </summary>
    /// <param name="source">Source image</param>
    /// <param name="brightnessValue">Brightness adjustment value (-255 to 255)</param>
    /// <returns>Image with adjusted brightness</returns>
    public static Image AdjustBrightness(Image source, int brightnessValue)
    {
        var result = new Image(source.Width, source.Height);

        for (int x = 0; x < source.Width; x++)
        {
            for (int y = 0; y < source.Height; y++)
            {
                byte r = source[x, y, Channel.R];
                byte g = source[x, y, Channel.G];
                byte b = source[x, y, Channel.B];
                byte a = source[x, y, Channel.A];

                // Apply brightness adjustment with clamping
                byte newR = (byte)Math.Clamp(r + brightnessValue, 0, 255);
                byte newG = (byte)Math.Clamp(g + brightnessValue, 0, 255);
                byte newB = (byte)Math.Clamp(b + brightnessValue, 0, 255);

                result[x, y] = new byte[] { newR, newG, newB, a };
            }
        }

        return result;
    }

    /// <summary>
    /// Adjust the contrast of an image
    /// </summary>
    /// <param name="source">Source image</param>
    /// <param name="contrastValue">Contrast adjustment value (0.1 to 10.0, where 1.0 is no change)</param>
    /// <returns>Image with adjusted contrast</returns>
    public static Image AdjustContrast(Image source, double contrastValue)
    {
        var result = new Image(source.Width, source.Height);

        for (int x = 0; x < source.Width; x++)
        {
            for (int y = 0; y < source.Height; y++)
            {
                byte r = source[x, y, Channel.R];
                byte g = source[x, y, Channel.G];
                byte b = source[x, y, Channel.B];
                byte a = source[x, y, Channel.A];

                // Apply contrast adjustment using the formula: new = (old - 128) * contrast + 128
                // This maintains 128 as the midpoint (gray level)
                byte newR = (byte)Math.Clamp((r - 128) * contrastValue + 128, 0, 255);
                byte newG = (byte)Math.Clamp((g - 128) * contrastValue + 128, 0, 255);
                byte newB = (byte)Math.Clamp((b - 128) * contrastValue + 128, 0, 255);

                result[x, y] = new byte[] { newR, newG, newB, a };
            }
        }

        return result;
    }
}

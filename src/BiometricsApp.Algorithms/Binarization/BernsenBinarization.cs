using BiometricsApp.Core.Extensions;
using BiometricsApp.Core.Models;

namespace BiometricsApp.Algorithms.Binarization;

/// <summary>
/// Bernsen's local binarization algorithm based on local contrast
/// Good for images with varying illumination
/// </summary>
public static class BernsenBinarization
{
    /// <summary>
    /// Apply Bernsen binarization
    /// </summary>
    /// <param name="source">Source image</param>
    /// <param name="windowSize">Window size for local calculation (default: 31)</param>
    /// <param name="contrastThreshold">Minimum contrast threshold (default: 15)</param>
    public static Image Apply(Image source, int windowSize = 31, int contrastThreshold = 15)
    {
        var result = new Image(source.Width, source.Height);
        int halfWindow = windowSize / 2;

        for (int x = 0; x < source.Width; x++)
        {
            for (int y = 0; y < source.Height; y++)
            {
                // Find min and max in local window
                byte min = 255;
                byte max = 0;

                int xStart = Math.Max(0, x - halfWindow);
                int xEnd = Math.Min(source.Width - 1, x + halfWindow);
                int yStart = Math.Max(0, y - halfWindow);
                int yEnd = Math.Min(source.Height - 1, y + halfWindow);

                for (int xx = xStart; xx <= xEnd; xx++)
                {
                    for (int yy = yStart; yy <= yEnd; yy++)
                    {
                        byte value = (byte)source[xx, yy].Average();
                        if (value < min) min = value;
                        if (value > max) max = value;
                    }
                }

                // Calculate local contrast
                int contrast = max - min;
                byte pixelValue = (byte)source[x, y].Average();

                // If contrast is too low, use global threshold (128)
                if (contrast < contrastThreshold)
                {
                    result[x, y] = [
                        pixelValue >= 128 ? byte.MaxValue : byte.MinValue
                    ];
                }
                else
                {
                    // Use local threshold (midpoint between min and max)
                    int threshold = (min + max) / 2;
                    result[x, y] = [
                        pixelValue >= threshold ? byte.MaxValue : byte.MinValue
                    ];
                }
            }
        }

        return result;
    }
}

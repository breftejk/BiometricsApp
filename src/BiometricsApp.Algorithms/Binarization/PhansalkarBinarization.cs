using BiometricsApp.Core.Extensions;
using BiometricsApp.Core.Models;

namespace BiometricsApp.Algorithms.Binarization;

/// <summary>
/// Phansalkar's local binarization algorithm (modification of Sauvola)
/// Better for low contrast images
/// </summary>
public static class PhansalkarBinarization
{
    /// <summary>
    /// Apply Phansalkar binarization
    /// </summary>
    /// <param name="source">Source image</param>
    /// <param name="k">Threshold parameter (default: 0.25)</param>
    /// <param name="r">Dynamic range of standard deviation (default: 0.5)</param>
    /// <param name="p">Additional parameter (default: 2.0)</param>
    /// <param name="q">Additional parameter (default: 10.0)</param>
    /// <param name="windowSize">Window size for local calculation (default: 15)</param>
    public static Image Apply(Image source, double k = 0.25, double r = 0.5, double p = 2.0, double q = 10.0, int windowSize = 15)
    {
        var result = new Image(source.Width, source.Height);
        int halfWindow = windowSize / 2;

        for (int x = 0; x < source.Width; x++)
        {
            for (int y = 0; y < source.Height; y++)
            {
                // Calculate mean and standard deviation in local window
                double mean = 0;
                double variance = 0;
                int count = 0;

                int xStart = Math.Max(0, x - halfWindow);
                int xEnd = Math.Min(source.Width - 1, x + halfWindow);
                int yStart = Math.Max(0, y - halfWindow);
                int yEnd = Math.Min(source.Height - 1, y + halfWindow);

                // First pass: calculate mean
                for (int xx = xStart; xx <= xEnd; xx++)
                {
                    for (int yy = yStart; yy <= yEnd; yy++)
                    {
                        mean += source[xx, yy].Average();
                        count++;
                    }
                }
                mean /= count;

                // Second pass: calculate variance
                for (int xx = xStart; xx <= xEnd; xx++)
                {
                    for (int yy = yStart; yy <= yEnd; yy++)
                    {
                        double diff = source[xx, yy].Average() - mean;
                        variance += diff * diff;
                    }
                }
                double stddev = Math.Sqrt(variance / count);

                // Phansalkar threshold formula
                double threshold = mean * (1.0 + p * Math.Exp(-q * mean) + k * ((stddev / r) - 1.0));

                // Apply threshold
                double pixelValue = source[x, y].Average();
                result[x, y] = [
                    pixelValue > threshold ? byte.MaxValue : byte.MinValue
                ];
            }
        }

        return result;
    }
}

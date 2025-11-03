using BiometricsApp.Core.Extensions;
using BiometricsApp.Core.Models;

namespace BiometricsApp.Algorithms.Binarization;

/// <summary>
/// Niblack's local binarization algorithm
/// </summary>
public static class NiblackBinarization
{
    /// <summary>
    /// Apply Niblack binarization
    /// </summary>
    /// <param name="source">Source image</param>
    /// <param name="k">Threshold parameter (default: 0.8)</param>
    /// <param name="windowSize">Window size for local calculation (default: 3)</param>
    public static Image Apply(Image source, double k = 0.8, int windowSize = 3)
    {
        var result = new Image(source.Width, source.Height);
        int halfWindow = windowSize / 2;

        for (int x = halfWindow; x < source.Width - halfWindow; x++)
        {
            for (int y = halfWindow; y < source.Height - halfWindow; y++)
            {
                // Calculate mean
                double mean = 0;
                int count = 0;
                for (int xx = -halfWindow; xx <= halfWindow; xx++)
                {
                    for (int yy = -halfWindow; yy <= halfWindow; yy++)
                    {
                        mean += source[x + xx, y + yy].Average();
                        count++;
                    }
                }
                mean /= count;

                // Calculate standard deviation
                double stddev = 0;
                for (int xx = -halfWindow; xx <= halfWindow; xx++)
                {
                    for (int yy = -halfWindow; yy <= halfWindow; yy++)
                    {
                        double diff = source[x + xx, y + yy].Average() - mean;
                        stddev += diff * diff;
                    }
                }
                stddev = Math.Sqrt(stddev / count);

                // Calculate threshold
                double threshold = mean + stddev * k;

                // Apply threshold
                result[x, y] = [
                    source[x, y].Average() < threshold
                        ? byte.MaxValue
                        : byte.MinValue
                ];
            }
        }

        return result;
    }
}


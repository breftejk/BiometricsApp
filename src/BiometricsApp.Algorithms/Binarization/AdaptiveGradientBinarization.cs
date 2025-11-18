using BiometricsApp.Core.Extensions;
using BiometricsApp.Core.Models;

namespace BiometricsApp.Algorithms.Binarization;

/// <summary>
/// Custom Adaptive Gradient Binarization Algorithm
/// Combines local gradient information with adaptive thresholding
/// Works well for images with varying illumination and edge preservation
/// </summary>
public static class AdaptiveGradientBinarization
{
    /// <summary>
    /// Apply custom adaptive gradient binarization
    /// </summary>
    /// <param name="source">Source image</param>
    /// <param name="windowSize">Window size for local calculation (default: 15)</param>
    /// <param name="gradientWeight">Weight for gradient influence (default: 0.3)</param>
    /// <param name="k">Threshold adjustment parameter (default: 0.2)</param>
    public static Image Apply(Image source, int windowSize = 15, double gradientWeight = 0.3, double k = 0.2)
    {
        var result = new Image(source.Width, source.Height);
        int halfWindow = windowSize / 2;

        // Calculate gradient magnitude for entire image
        double[,] gradients = CalculateGradients(source);

        for (int x = 0; x < source.Width; x++)
        {
            for (int y = 0; y < source.Height; y++)
            {
                // Calculate local statistics
                double mean = 0;
                double variance = 0;
                double gradientSum = 0;
                int count = 0;

                int xStart = Math.Max(0, x - halfWindow);
                int xEnd = Math.Min(source.Width - 1, x + halfWindow);
                int yStart = Math.Max(0, y - halfWindow);
                int yEnd = Math.Min(source.Height - 1, y + halfWindow);

                // First pass: calculate mean and gradient sum
                for (int xx = xStart; xx <= xEnd; xx++)
                {
                    for (int yy = yStart; yy <= yEnd; yy++)
                    {
                        double value = source[xx, yy].Average();
                        mean += value;
                        gradientSum += gradients[xx, yy];
                        count++;
                    }
                }
                mean /= count;
                double avgGradient = gradientSum / count;

                // Second pass: calculate standard deviation
                for (int xx = xStart; xx <= xEnd; xx++)
                {
                    for (int yy = yStart; yy <= yEnd; yy++)
                    {
                        double diff = source[xx, yy].Average() - mean;
                        variance += diff * diff;
                    }
                }
                double stddev = Math.Sqrt(variance / count);

                // Custom threshold formula combining mean, stddev, and gradient
                // Higher gradient values (edges) get lower threshold for better edge preservation
                double gradientFactor = 1.0 - (gradientWeight * (gradients[x, y] / 255.0));
                double threshold = mean * gradientFactor * (1.0 + k * ((stddev / 128.0) - 1.0));

                // Apply threshold
                double pixelValue = source[x, y].Average();
                result[x, y] = [
                    pixelValue > threshold ? byte.MaxValue : byte.MinValue
                ];
            }
        }

        return result;
    }

    /// <summary>
    /// Calculate gradient magnitude using Sobel operator
    /// </summary>
    private static double[,] CalculateGradients(Image source)
    {
        double[,] gradients = new double[source.Width, source.Height];

        // Sobel kernels
        int[,] sobelX = { { -1, 0, 1 }, { -2, 0, 2 }, { -1, 0, 1 } };
        int[,] sobelY = { { -1, -2, -1 }, { 0, 0, 0 }, { 1, 2, 1 } };

        for (int x = 1; x < source.Width - 1; x++)
        {
            for (int y = 1; y < source.Height - 1; y++)
            {
                double gx = 0;
                double gy = 0;

                // Apply Sobel kernels
                for (int i = -1; i <= 1; i++)
                {
                    for (int j = -1; j <= 1; j++)
                    {
                        double value = source[x + i, y + j].Average();
                        gx += value * sobelX[i + 1, j + 1];
                        gy += value * sobelY[i + 1, j + 1];
                    }
                }

                // Calculate gradient magnitude
                gradients[x, y] = Math.Sqrt(gx * gx + gy * gy);
            }
        }

        return gradients;
    }
}

using BiometricsApp.Core.Extensions;
using BiometricsApp.Core.Models;

namespace BiometricsApp.Algorithms.Binarization;

/// <summary>
/// Li and Wu's minimum cross entropy binarization algorithm
/// Uses minimum cross entropy criterion for threshold selection
/// </summary>
public static class LiWuBinarization
{
    /// <summary>
    /// Apply Li-Wu binarization
    /// </summary>
    /// <param name="source">Source image</param>
    /// <returns>Tuple with binarized image and optimal threshold</returns>
    public static (Image result, int threshold) Apply(Image source)
    {
        // Calculate histogram
        int[] histogram = new int[256];
        int totalPixels = source.Width * source.Height;

        for (int x = 0; x < source.Width; x++)
        {
            for (int y = 0; y < source.Height; y++)
            {
                byte value = (byte)source[x, y].Average();
                histogram[value]++;
            }
        }

        // Normalize histogram to get probability distribution
        double[] prob = new double[256];
        for (int i = 0; i < 256; i++)
        {
            prob[i] = (double)histogram[i] / totalPixels;
        }

        // Find optimal threshold using minimum cross entropy
        double minCrossEntropy = double.MaxValue;
        int optimalThreshold = 0;

        for (int t = 1; t < 255; t++)
        {
            // Calculate means for background and foreground
            double mb = 0; // background mean
            double mf = 0; // foreground mean
            double wb = 0; // background probability
            double wf = 0; // foreground probability

            for (int i = 0; i <= t; i++)
            {
                wb += prob[i];
                mb += i * prob[i];
            }

            for (int i = t + 1; i < 256; i++)
            {
                wf += prob[i];
                mf += i * prob[i];
            }

            if (wb == 0 || wf == 0)
                continue;

            mb /= wb;
            mf /= wf;

            // Calculate cross entropy
            double crossEntropy = 0;

            for (int i = 0; i <= t; i++)
            {
                if (prob[i] > 0)
                {
                    double temp = i - mb;
                    crossEntropy += prob[i] * temp * temp;
                }
            }

            for (int i = t + 1; i < 256; i++)
            {
                if (prob[i] > 0)
                {
                    double temp = i - mf;
                    crossEntropy += prob[i] * temp * temp;
                }
            }

            // Add entropy terms
            if (wb > 0)
                crossEntropy += wb * Math.Log(wb);
            if (wf > 0)
                crossEntropy += wf * Math.Log(wf);

            if (crossEntropy < minCrossEntropy)
            {
                minCrossEntropy = crossEntropy;
                optimalThreshold = t;
            }
        }

        // Apply binarization with optimal threshold
        var result = new Image(source.Width, source.Height);
        for (int x = 0; x < source.Width; x++)
        {
            for (int y = 0; y < source.Height; y++)
            {
                byte value = (byte)source[x, y].Average();
                result[x, y] = [
                    value > optimalThreshold ? byte.MaxValue : byte.MinValue
                ];
            }
        }

        return (result, optimalThreshold);
    }
}

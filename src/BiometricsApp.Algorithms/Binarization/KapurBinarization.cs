using BiometricsApp.Core.Extensions;
using BiometricsApp.Core.Models;

namespace BiometricsApp.Algorithms.Binarization;

/// <summary>
/// Kapur's entropy-based binarization algorithm
/// Uses maximum entropy criterion for threshold selection
/// </summary>
public static class KapurBinarization
{
    /// <summary>
    /// Apply Kapur binarization
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

        // Find optimal threshold using maximum entropy criterion
        double maxEntropy = double.MinValue;
        int optimalThreshold = 0;

        for (int t = 0; t < 256; t++)
        {
            // Calculate probabilities for background and foreground
            double wb = 0; // background probability
            double wf = 0; // foreground probability

            for (int i = 0; i <= t; i++)
                wb += prob[i];
            for (int i = t + 1; i < 256; i++)
                wf += prob[i];

            if (wb == 0 || wf == 0)
                continue;

            // Calculate entropies
            double hb = 0; // background entropy
            double hf = 0; // foreground entropy

            for (int i = 0; i <= t; i++)
            {
                if (prob[i] > 0)
                    hb -= (prob[i] / wb) * Math.Log(prob[i] / wb);
            }

            for (int i = t + 1; i < 256; i++)
            {
                if (prob[i] > 0)
                    hf -= (prob[i] / wf) * Math.Log(prob[i] / wf);
            }

            // Total entropy
            double totalEntropy = hb + hf;

            if (totalEntropy > maxEntropy)
            {
                maxEntropy = totalEntropy;
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

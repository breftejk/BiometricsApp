using BiometricsApp.Core.Models;

namespace BiometricsApp.Algorithms.Histogram;

/// <summary>
/// Histogram equalization algorithm for improving image contrast
/// </summary>
public static class HistogramEqualization
{
    /// <summary>
    /// Apply histogram equalization to improve image contrast
    /// </summary>
    /// <param name="source">Source image</param>
    /// <returns>Image with equalized histogram</returns>
    public static Image Apply(Image source)
    {
        var result = new Image(source.Width, source.Height);

        // Calculate histograms for each channel
        var histogramR = new int[256];
        var histogramG = new int[256];
        var histogramB = new int[256];

        for (int x = 0; x < source.Width; x++)
        {
            for (int y = 0; y < source.Height; y++)
            {
                histogramR[source[x, y, Channel.R]]++;
                histogramG[source[x, y, Channel.G]]++;
                histogramB[source[x, y, Channel.B]]++;
            }
        }

        // Calculate cumulative distribution functions (CDF)
        var cdfR = CalculateCDF(histogramR);
        var cdfG = CalculateCDF(histogramG);
        var cdfB = CalculateCDF(histogramB);

        // Find minimum non-zero CDF values
        int minCdfR = FindMinNonZeroCDF(cdfR);
        int minCdfG = FindMinNonZeroCDF(cdfG);
        int minCdfB = FindMinNonZeroCDF(cdfB);

        int totalPixels = source.Width * source.Height;

        // Apply equalization formula: h(v) = round((cdf(v) - cdfmin) / (M*N - cdfmin) * (L-1))
        for (int x = 0; x < source.Width; x++)
        {
            for (int y = 0; y < source.Height; y++)
            {
                byte r = source[x, y, Channel.R];
                byte g = source[x, y, Channel.G];
                byte b = source[x, y, Channel.B];
                byte a = source[x, y, Channel.A];

                byte newR = (byte)Math.Round((double)(cdfR[r] - minCdfR) / (totalPixels - minCdfR) * 255);
                byte newG = (byte)Math.Round((double)(cdfG[g] - minCdfG) / (totalPixels - minCdfG) * 255);
                byte newB = (byte)Math.Round((double)(cdfB[b] - minCdfB) / (totalPixels - minCdfB) * 255);

                result[x, y] = new byte[] { newR, newG, newB, a };
            }
        }

        return result;
    }

    /// <summary>
    /// Calculate the cumulative distribution function (CDF) from histogram
    /// </summary>
    /// <param name="histogram">Input histogram</param>
    /// <returns>Cumulative distribution function</returns>
    private static int[] CalculateCDF(int[] histogram)
    {
        var cdf = new int[256];
        cdf[0] = histogram[0];

        for (int i = 1; i < 256; i++)
        {
            cdf[i] = cdf[i - 1] + histogram[i];
        }

        return cdf;
    }

    /// <summary>
    /// Find the minimum non-zero value in the CDF
    /// </summary>
    /// <param name="cdf">Cumulative distribution function</param>
    /// <returns>Minimum non-zero CDF value</returns>
    private static int FindMinNonZeroCDF(int[] cdf)
    {
        for (int i = 0; i < cdf.Length; i++)
        {
            if (cdf[i] > 0)
                return cdf[i];
        }
        return 0;
    }
}

using BiometricsApp.Core.Models;

namespace BiometricsApp.Algorithms.Binarization;

/// <summary>
/// Otsu's method for automatic threshold selection in image binarization
/// </summary>
public static class OtsuBinarization
{
    /// <summary>
    /// Apply Otsu binarization to the image (using average of RGB channels)
    /// </summary>
    /// <param name="source">Source image</param>
    /// <returns>Tuple containing the binarized image and optimal threshold</returns>
    public static (Image result, int threshold) Apply(Image source)
    {
        // Calculate histogram for average values
        var histogram = new int[256];
        
        for (int x = 0; x < source.Width; x++)
        {
            for (int y = 0; y < source.Height; y++)
            {
                byte r = source[x, y, Channel.R];
                byte g = source[x, y, Channel.G];
                byte b = source[x, y, Channel.B];
                int avg = (r + g + b) / 3;
                histogram[avg]++;
            }
        }

        int optimalThreshold = CalculateOtsuThreshold(histogram, source.Width * source.Height);
        
        // Apply binarization with optimal threshold
        var result = new Image(source.Width, source.Height);
        
        for (int x = 0; x < source.Width; x++)
        {
            for (int y = 0; y < source.Height; y++)
            {
                byte r = source[x, y, Channel.R];
                byte g = source[x, y, Channel.G];
                byte b = source[x, y, Channel.B];
                int avg = (r + g + b) / 3;
                
                byte binaryValue = avg >= optimalThreshold ? byte.MaxValue : byte.MinValue;
                result[x, y] = new byte[] { binaryValue, binaryValue, binaryValue, 255 };
            }
        }

        return (result, optimalThreshold);
    }

    /// <summary>
    /// Apply Otsu binarization to a specific channel
    /// </summary>
    /// <param name="source">Source image</param>
    /// <param name="channel">Channel to process</param>
    /// <returns>Tuple containing the binarized image and optimal threshold</returns>
    public static (Image result, int threshold) ApplyToChannel(Image source, Channel channel)
    {
        // Calculate histogram for the specific channel
        var histogram = new int[256];
        
        for (int x = 0; x < source.Width; x++)
        {
            for (int y = 0; y < source.Height; y++)
            {
                byte value = source[x, y, channel];
                histogram[value]++;
            }
        }

        int optimalThreshold = CalculateOtsuThreshold(histogram, source.Width * source.Height);
        
        // Apply binarization with optimal threshold
        var result = new Image(source.Width, source.Height);
        
        for (int x = 0; x < source.Width; x++)
        {
            for (int y = 0; y < source.Height; y++)
            {
                byte value = source[x, y, channel];
                byte binaryValue = value >= optimalThreshold ? byte.MaxValue : byte.MinValue;
                result[x, y] = new byte[] { binaryValue, binaryValue, binaryValue, 255 };
            }
        }

        return (result, optimalThreshold);
    }

    /// <summary>
    /// Calculate the optimal threshold using Otsu's method
    /// </summary>
    /// <param name="histogram">Intensity histogram</param>
    /// <param name="totalPixels">Total number of pixels</param>
    /// <returns>Optimal threshold value</returns>
    private static int CalculateOtsuThreshold(int[] histogram, int totalPixels)
    {
        double sum = 0;
        for (int i = 0; i < 256; i++)
            sum += i * histogram[i];

        double sumB = 0;
        int wB = 0;
        int wF = 0;
        double maxVariance = 0;
        int threshold = 0;

        for (int i = 0; i < 256; i++)
        {
            wB += histogram[i]; // Weight background
            if (wB == 0) continue;

            wF = totalPixels - wB; // Weight foreground
            if (wF == 0) break;

            sumB += i * histogram[i];
            
            double mB = sumB / wB; // Mean background
            double mF = (sum - sumB) / wF; // Mean foreground

            // Calculate between-class variance
            double betweenClassVariance = wB * wF * (mB - mF) * (mB - mF);

            // Check if this is the maximum variance
            if (betweenClassVariance > maxVariance)
            {
                maxVariance = betweenClassVariance;
                threshold = i;
            }
        }

        return threshold;
    }
}

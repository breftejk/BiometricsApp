using BiometricsApp.Core.Models;

namespace BiometricsApp.Algorithms.Filters;

/// <summary>
/// Predator filter - combines pixelization, MinRGB, and Sobel edge detection
/// Creates a thermal/predator vision effect
/// </summary>
public static class PredatorFilter
{
    /// <summary>
    /// Apply complete Predator filter effect
    /// </summary>
    /// <param name="source">Source image</param>
    /// <param name="pixelSize">Size of pixelation blocks (default: 10)</param>
    public static Image Apply(Image source, int pixelSize = 10)
    {
        // Step 1: Pixelization
        var pixelized = PixelizationFilter.Apply(source, pixelSize);

        // Step 2: MinRGB
        var minRgb = ApplyMinRGB(pixelized);

        // Step 3: Sobel edge detection
        var edges = ConvolutionFilter.ApplySobelMagnitude(minRgb);

        return edges;
    }

    /// <summary>
    /// Apply MinRGB transformation
    /// Each pixel becomes the minimum of its RGB components
    /// </summary>
    private static Image ApplyMinRGB(Image source)
    {
        var result = new Image(source.Width, source.Height);

        for (int y = 0; y < source.Height; y++)
        {
            for (int x = 0; x < source.Width; x++)
            {
                var pixel = source[x, y];
                byte minValue = Math.Min(Math.Min(pixel[0], pixel[1]), pixel[2]);

                result[x, y] = new byte[] { minValue, minValue, minValue };
            }
        }

        return result;
    }

    /// <summary>
    /// Apply Predator filter with custom parameters
    /// </summary>
    /// <param name="source">Source image</param>
    /// <param name="pixelSize">Size of pixelation blocks</param>
    /// <param name="applyColorGrade">Apply thermal color grading</param>
    public static Image ApplyWithColorGrade(Image source, int pixelSize = 10, bool applyColorGrade = true)
    {
        // Apply base Predator filter
        var result = Apply(source, pixelSize);

        // Apply thermal color grading if requested
        if (applyColorGrade)
        {
            result = ApplyThermalColorGrade(result);
        }

        return result;
    }

    /// <summary>
    /// Apply thermal/heat vision color grading
    /// Maps grayscale to thermal colors (blue -> green -> yellow -> red)
    /// </summary>
    private static Image ApplyThermalColorGrade(Image source)
    {
        var result = new Image(source.Width, source.Height);

        for (int y = 0; y < source.Height; y++)
        {
            for (int x = 0; x < source.Width; x++)
            {
                var pixel = source[x, y];
                byte intensity = pixel[0]; // Assume grayscale

                // Map intensity to thermal colors
                byte r, g, b;

                if (intensity < 64)
                {
                    // Dark blue to cyan
                    r = 0;
                    g = (byte)(intensity * 2);
                    b = (byte)(64 + intensity * 2);
                }
                else if (intensity < 128)
                {
                    // Cyan to green
                    r = 0;
                    g = (byte)(128 + (intensity - 64) * 2);
                    b = (byte)(192 - (intensity - 64) * 3);
                }
                else if (intensity < 192)
                {
                    // Green to yellow
                    r = (byte)((intensity - 128) * 4);
                    g = 255;
                    b = 0;
                }
                else
                {
                    // Yellow to red
                    r = 255;
                    g = (byte)(255 - (intensity - 192) * 4);
                    b = 0;
                }

                result[x, y] = new byte[] { r, g, b };
            }
        }

        return result;
    }

    /// <summary>
    /// Apply individual components of Predator filter
    /// </summary>
    public static class Components
    {
        /// <summary>
        /// Apply only MinRGB transformation
        /// </summary>
        public static Image MinRGB(Image source)
        {
            return ApplyMinRGB(source);
        }

        /// <summary>
        /// Apply Pixelization + MinRGB (without Sobel)
        /// </summary>
        public static Image PixelizedMinRGB(Image source, int pixelSize = 10)
        {
            var pixelized = PixelizationFilter.Apply(source, pixelSize);
            return ApplyMinRGB(pixelized);
        }

        /// <summary>
        /// Apply MinRGB + Sobel (without pixelization)
        /// </summary>
        public static Image MinRGBWithEdges(Image source)
        {
            var minRgb = ApplyMinRGB(source);
            return ConvolutionFilter.ApplySobelMagnitude(minRgb);
        }
    }
}

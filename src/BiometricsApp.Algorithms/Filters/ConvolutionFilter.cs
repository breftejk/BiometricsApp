using BiometricsApp.Core.Models;
using BiometricsApp.Core.Extensions;

namespace BiometricsApp.Algorithms.Filters;

/// <summary>
/// Convolution-based linear filters
/// </summary>
public static class ConvolutionFilter
{
    /// <summary>
    /// Predefined convolution kernels
    /// </summary>
    public enum KernelType
    {
        GaussianBlur3x3,
        GaussianBlur5x5,
        Prewitt,
        Sobel,
        Laplacian4,
        Laplacian8,
        Sharpen,
        EdgeDetect,
        Emboss,
        Custom
    }

    /// <summary>
    /// Apply convolution filter with predefined kernel
    /// </summary>
    public static Image Apply(Image source, KernelType kernelType)
    {
        var kernel = GetKernel(kernelType);
        return Apply(source, kernel.matrix, kernel.divisor, kernel.offset);
    }

    /// <summary>
    /// Apply convolution filter with custom kernel
    /// </summary>
    public static Image Apply(Image source, double[,] kernel, double divisor = 1.0, double offset = 0.0)
    {
        var result = new Image(source.Width, source.Height);
        int kernelHeight = kernel.GetLength(0);
        int kernelWidth = kernel.GetLength(1);
        int offsetY = kernelHeight / 2;
        int offsetX = kernelWidth / 2;

        for (int y = 0; y < source.Height; y++)
        {
            for (int x = 0; x < source.Width; x++)
            {
                double sumR = 0, sumG = 0, sumB = 0;

                // Apply kernel
                for (int ky = 0; ky < kernelHeight; ky++)
                {
                    for (int kx = 0; kx < kernelWidth; kx++)
                    {
                        int py = y + ky - offsetY;
                        int px = x + kx - offsetX;

                        // Border handling: clamp to edges
                        py = Math.Clamp(py, 0, source.Height - 1);
                        px = Math.Clamp(px, 0, source.Width - 1);

                        var pixel = source[px, py];
                        double kernelValue = kernel[ky, kx];

                        sumR += pixel[0] * kernelValue;
                        sumG += pixel[1] * kernelValue;
                        sumB += pixel[2] * kernelValue;
                    }
                }

                // Apply divisor and offset
                sumR = sumR / divisor + offset;
                sumG = sumG / divisor + offset;
                sumB = sumB / divisor + offset;

                // Clamp values
                result[x, y] = new byte[]
                {
                    (byte)Math.Clamp(sumR, 0, 255),
                    (byte)Math.Clamp(sumG, 0, 255),
                    (byte)Math.Clamp(sumB, 0, 255)
                };
            }
        }

        return result;
    }

    /// <summary>
    /// Get predefined kernel
    /// </summary>
    private static (double[,] matrix, double divisor, double offset) GetKernel(KernelType type)
    {
        return type switch
        {
            KernelType.GaussianBlur3x3 => (new double[,]
            {
                { 1, 2, 1 },
                { 2, 4, 2 },
                { 1, 2, 1 }
            }, 16.0, 0.0),

            KernelType.GaussianBlur5x5 => (new double[,]
            {
                { 1,  4,  6,  4, 1 },
                { 4, 16, 24, 16, 4 },
                { 6, 24, 36, 24, 6 },
                { 4, 16, 24, 16, 4 },
                { 1,  4,  6,  4, 1 }
            }, 256.0, 0.0),

            KernelType.Prewitt => (new double[,]
            {
                { -1, 0, 1 },
                { -1, 0, 1 },
                { -1, 0, 1 }
            }, 1.0, 128.0),

            KernelType.Sobel => (new double[,]
            {
                { -1, 0, 1 },
                { -2, 0, 2 },
                { -1, 0, 1 }
            }, 1.0, 128.0),

            KernelType.Laplacian4 => (new double[,]
            {
                { 0, -1,  0 },
                {-1,  4, -1 },
                { 0, -1,  0 }
            }, 1.0, 128.0),

            KernelType.Laplacian8 => (new double[,]
            {
                {-1, -1, -1 },
                {-1,  8, -1 },
                {-1, -1, -1 }
            }, 1.0, 128.0),

            KernelType.Sharpen => (new double[,]
            {
                { 0, -1,  0 },
                {-1,  5, -1 },
                { 0, -1,  0 }
            }, 1.0, 0.0),

            KernelType.EdgeDetect => (new double[,]
            {
                {-1, -1, -1 },
                {-1,  8, -1 },
                {-1, -1, -1 }
            }, 1.0, 0.0),

            KernelType.Emboss => (new double[,]
            {
                {-2, -1, 0 },
                {-1,  1, 1 },
                { 0,  1, 2 }
            }, 1.0, 128.0),

            _ => (new double[,] { { 1 } }, 1.0, 0.0)
        };
    }

    /// <summary>
    /// Apply Sobel edge detection with magnitude calculation
    /// </summary>
    public static Image ApplySobelMagnitude(Image source)
    {
        var result = new Image(source.Width, source.Height);

        double[,] sobelX = { { -1, 0, 1 }, { -2, 0, 2 }, { -1, 0, 1 } };
        double[,] sobelY = { { -1, -2, -1 }, { 0, 0, 0 }, { 1, 2, 1 } };

        for (int y = 1; y < source.Height - 1; y++)
        {
            for (int x = 1; x < source.Width - 1; x++)
            {
                double gxR = 0, gyR = 0;
                double gxG = 0, gyG = 0;
                double gxB = 0, gyB = 0;

                for (int ky = -1; ky <= 1; ky++)
                {
                    for (int kx = -1; kx <= 1; kx++)
                    {
                        var pixel = source[x + kx, y + ky];
                        double sx = sobelX[ky + 1, kx + 1];
                        double sy = sobelY[ky + 1, kx + 1];

                        gxR += pixel[0] * sx;
                        gyR += pixel[0] * sy;
                        gxG += pixel[1] * sx;
                        gyG += pixel[1] * sy;
                        gxB += pixel[2] * sx;
                        gyB += pixel[2] * sy;
                    }
                }

                double magR = Math.Sqrt(gxR * gxR + gyR * gyR);
                double magG = Math.Sqrt(gxG * gxG + gyG * gyG);
                double magB = Math.Sqrt(gxB * gxB + gyB * gyB);

                result[x, y] = new byte[]
                {
                    (byte)Math.Clamp(magR, 0, 255),
                    (byte)Math.Clamp(magG, 0, 255),
                    (byte)Math.Clamp(magB, 0, 255)
                };
            }
        }

        return result;
    }
}

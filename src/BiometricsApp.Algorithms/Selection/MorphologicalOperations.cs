using BiometricsApp.Core.Models;
using static BiometricsApp.Algorithms.Selection.MagicWandTool;

namespace BiometricsApp.Algorithms.Selection;

/// <summary>
/// Morphological operations for selection mask manipulation
/// </summary>
public static class MorphologicalOperations
{
    /// <summary>
    /// Kernel shape for morphological operations
    /// </summary>
    public enum KernelShape
    {
        /// <summary>
        /// Square structuring element
        /// </summary>
        Square,
        /// <summary>
        /// Cross-shaped structuring element
        /// </summary>
        Cross,
        /// <summary>
        /// Circular structuring element
        /// </summary>
        Circle
    }

    /// <summary>
    /// Apply dilation to a selection mask (expand selection)
    /// </summary>
    /// <param name="mask">Input selection mask</param>
    /// <param name="kernelSize">Size of structuring element (must be odd)</param>
    /// <param name="shape">Shape of structuring element</param>
    /// <returns>Dilated selection mask</returns>
    public static SelectionMask Dilate(SelectionMask mask, int kernelSize = 3, KernelShape shape = KernelShape.Square)
    {
        if (kernelSize % 2 == 0) kernelSize++;
        var kernel = CreateKernel(kernelSize, shape);
        var result = new SelectionMask(mask.Width, mask.Height);
        int offset = kernelSize / 2;

        for (int y = 0; y < mask.Height; y++)
        {
            for (int x = 0; x < mask.Width; x++)
            {
                bool shouldSelect = false;

                for (int ky = 0; ky < kernelSize && !shouldSelect; ky++)
                {
                    for (int kx = 0; kx < kernelSize && !shouldSelect; kx++)
                    {
                        if (!kernel[ky, kx]) continue;

                        int ny = y + ky - offset;
                        int nx = x + kx - offset;

                        if (nx >= 0 && nx < mask.Width && ny >= 0 && ny < mask.Height)
                        {
                            if (mask[nx, ny])
                            {
                                shouldSelect = true;
                            }
                        }
                    }
                }

                result[x, y] = shouldSelect;
            }
        }

        return result;
    }

    /// <summary>
    /// Apply erosion to a selection mask (shrink selection)
    /// </summary>
    /// <param name="mask">Input selection mask</param>
    /// <param name="kernelSize">Size of structuring element (must be odd)</param>
    /// <param name="shape">Shape of structuring element</param>
    /// <returns>Eroded selection mask</returns>
    public static SelectionMask Erode(SelectionMask mask, int kernelSize = 3, KernelShape shape = KernelShape.Square)
    {
        if (kernelSize % 2 == 0) kernelSize++;
        var kernel = CreateKernel(kernelSize, shape);
        var result = new SelectionMask(mask.Width, mask.Height);
        int offset = kernelSize / 2;

        for (int y = 0; y < mask.Height; y++)
        {
            for (int x = 0; x < mask.Width; x++)
            {
                bool shouldSelect = true;

                for (int ky = 0; ky < kernelSize && shouldSelect; ky++)
                {
                    for (int kx = 0; kx < kernelSize && shouldSelect; kx++)
                    {
                        if (!kernel[ky, kx]) continue;

                        int ny = y + ky - offset;
                        int nx = x + kx - offset;

                        if (nx >= 0 && nx < mask.Width && ny >= 0 && ny < mask.Height)
                        {
                            if (!mask[nx, ny])
                            {
                                shouldSelect = false;
                            }
                        }
                        else
                        {
                            // Outside boundary treated as unselected
                            shouldSelect = false;
                        }
                    }
                }

                result[x, y] = shouldSelect;
            }
        }

        return result;
    }

    /// <summary>
    /// Apply morphological opening (erosion followed by dilation)
    /// </summary>
    public static SelectionMask Open(SelectionMask mask, int kernelSize = 3, KernelShape shape = KernelShape.Square)
    {
        var eroded = Erode(mask, kernelSize, shape);
        return Dilate(eroded, kernelSize, shape);
    }

    /// <summary>
    /// Apply morphological closing (dilation followed by erosion)
    /// </summary>
    public static SelectionMask Close(SelectionMask mask, int kernelSize = 3, KernelShape shape = KernelShape.Square)
    {
        var dilated = Dilate(mask, kernelSize, shape);
        return Erode(dilated, kernelSize, shape);
    }

    /// <summary>
    /// Apply dilation to a binary image (for image processing)
    /// </summary>
    public static Image DilateImage(Image source, int kernelSize = 3, KernelShape shape = KernelShape.Square)
    {
        if (kernelSize % 2 == 0) kernelSize++;
        var kernel = CreateKernel(kernelSize, shape);
        var result = new Image(source.Width, source.Height);
        int offset = kernelSize / 2;

        for (int y = 0; y < source.Height; y++)
        {
            for (int x = 0; x < source.Width; x++)
            {
                byte maxR = 0, maxG = 0, maxB = 0;

                for (int ky = 0; ky < kernelSize; ky++)
                {
                    for (int kx = 0; kx < kernelSize; kx++)
                    {
                        if (!kernel[ky, kx]) continue;

                        int ny = y + ky - offset;
                        int nx = x + kx - offset;

                        if (nx >= 0 && nx < source.Width && ny >= 0 && ny < source.Height)
                        {
                            var pixel = source[nx, ny];
                            maxR = Math.Max(maxR, pixel[0]);
                            maxG = Math.Max(maxG, pixel[1]);
                            maxB = Math.Max(maxB, pixel[2]);
                        }
                    }
                }

                result[x, y] = new byte[] { maxR, maxG, maxB };
            }
        }

        return result;
    }

    /// <summary>
    /// Apply erosion to a binary image (for image processing)
    /// </summary>
    public static Image ErodeImage(Image source, int kernelSize = 3, KernelShape shape = KernelShape.Square)
    {
        if (kernelSize % 2 == 0) kernelSize++;
        var kernel = CreateKernel(kernelSize, shape);
        var result = new Image(source.Width, source.Height);
        int offset = kernelSize / 2;

        for (int y = 0; y < source.Height; y++)
        {
            for (int x = 0; x < source.Width; x++)
            {
                byte minR = 255, minG = 255, minB = 255;

                for (int ky = 0; ky < kernelSize; ky++)
                {
                    for (int kx = 0; kx < kernelSize; kx++)
                    {
                        if (!kernel[ky, kx]) continue;

                        int ny = y + ky - offset;
                        int nx = x + kx - offset;

                        if (nx >= 0 && nx < source.Width && ny >= 0 && ny < source.Height)
                        {
                            var pixel = source[nx, ny];
                            minR = Math.Min(minR, pixel[0]);
                            minG = Math.Min(minG, pixel[1]);
                            minB = Math.Min(minB, pixel[2]);
                        }
                    }
                }

                result[x, y] = new byte[] { minR, minG, minB };
            }
        }

        return result;
    }

    /// <summary>
    /// Create a structuring element kernel
    /// </summary>
    private static bool[,] CreateKernel(int size, KernelShape shape)
    {
        var kernel = new bool[size, size];
        int center = size / 2;

        switch (shape)
        {
            case KernelShape.Square:
                for (int y = 0; y < size; y++)
                    for (int x = 0; x < size; x++)
                        kernel[y, x] = true;
                break;

            case KernelShape.Cross:
                for (int y = 0; y < size; y++)
                    for (int x = 0; x < size; x++)
                        kernel[y, x] = (x == center) || (y == center);
                break;

            case KernelShape.Circle:
                double radius = center + 0.5;
                for (int y = 0; y < size; y++)
                    for (int x = 0; x < size; x++)
                    {
                        double dx = x - center;
                        double dy = y - center;
                        kernel[y, x] = (dx * dx + dy * dy) <= (radius * radius);
                    }
                break;
        }

        return kernel;
    }
}

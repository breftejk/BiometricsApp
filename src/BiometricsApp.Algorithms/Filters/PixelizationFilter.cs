using BiometricsApp.Core.Models;

namespace BiometricsApp.Algorithms.Filters;

/// <summary>
/// Pixelization filter - creates mosaic/pixelated effect
/// </summary>
public static class PixelizationFilter
{
    /// <summary>
    /// Apply pixelization effect
    /// </summary>
    /// <param name="source">Source image</param>
    /// <param name="pixelSize">Size of each pixel block</param>
    public static Image Apply(Image source, int pixelSize = 10)
    {
        if (pixelSize < 1)
            throw new ArgumentException("Pixel size must be at least 1", nameof(pixelSize));

        var result = new Image(source.Width, source.Height);

        // Process image in blocks
        for (int y = 0; y < source.Height; y += pixelSize)
        {
            for (int x = 0; x < source.Width; x += pixelSize)
            {
                // Calculate average color for this block
                long sumR = 0, sumG = 0, sumB = 0;
                int count = 0;

                int blockWidth = Math.Min(pixelSize, source.Width - x);
                int blockHeight = Math.Min(pixelSize, source.Height - y);

                for (int by = 0; by < blockHeight; by++)
                {
                    for (int bx = 0; bx < blockWidth; bx++)
                    {
                        var pixel = source[x + bx, y + by];
                        sumR += pixel[0];
                        sumG += pixel[1];
                        sumB += pixel[2];
                        count++;
                    }
                }

                // Calculate average
                byte avgR = (byte)(sumR / count);
                byte avgG = (byte)(sumG / count);
                byte avgB = (byte)(sumB / count);

                // Fill the block with average color
                for (int by = 0; by < blockHeight; by++)
                {
                    for (int bx = 0; bx < blockWidth; bx++)
                    {
                        result[x + bx, y + by] = new byte[] { avgR, avgG, avgB };
                    }
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Apply pixelization with rectangular blocks
    /// </summary>
    /// <param name="source">Source image</param>
    /// <param name="blockWidth">Width of pixel blocks</param>
    /// <param name="blockHeight">Height of pixel blocks</param>
    public static Image Apply(Image source, int blockWidth, int blockHeight)
    {
        if (blockWidth < 1 || blockHeight < 1)
            throw new ArgumentException("Block dimensions must be at least 1");

        var result = new Image(source.Width, source.Height);

        // Process image in blocks
        for (int y = 0; y < source.Height; y += blockHeight)
        {
            for (int x = 0; x < source.Width; x += blockWidth)
            {
                // Calculate average color for this block
                long sumR = 0, sumG = 0, sumB = 0;
                int count = 0;

                int bw = Math.Min(blockWidth, source.Width - x);
                int bh = Math.Min(blockHeight, source.Height - y);

                for (int by = 0; by < bh; by++)
                {
                    for (int bx = 0; bx < bw; bx++)
                    {
                        var pixel = source[x + bx, y + by];
                        sumR += pixel[0];
                        sumG += pixel[1];
                        sumB += pixel[2];
                        count++;
                    }
                }

                // Calculate average
                byte avgR = (byte)(sumR / count);
                byte avgG = (byte)(sumG / count);
                byte avgB = (byte)(sumB / count);

                // Fill the block with average color
                for (int by = 0; by < bh; by++)
                {
                    for (int bx = 0; bx < bw; bx++)
                    {
                        result[x + bx, y + by] = new byte[] { avgR, avgG, avgB };
                    }
                }
            }
        }

        return result;
    }
}

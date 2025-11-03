using BiometricsApp.Core.Extensions;
using BiometricsApp.Core.Models;
using BiometricsApp.Core.Utilities;

namespace BiometricsApp.Algorithms.Binarization;

/// <summary>
/// Standard threshold binarization algorithms
/// </summary>
public static class ThresholdBinarization
{
    /// <summary>
    /// Apply standard binarization with threshold of 128
    /// </summary>
    public static Image ApplyStandard(Image source, byte threshold = 128)
    {
        var result = new Image(source.Width, source.Height);

        Output.WriteLine($"{source.Width}x{source.Height}");

        for (int x = 0; x < source.Width; x++)
            for (int y = 0; y < source.Height; y++)
            {
                double avg = source[x, y].Average();

                if (avg > threshold)
                    result[x, y] = Colors.White;
                else
                    result[x, y] = Colors.Black;
            }

        return result;
    }

    /// <summary>
    /// Apply binarization per bytes
    /// </summary>
    public static Image ApplyPerBytes(Image source, byte threshold = 128)
    {
        var result = new Image(source.Width, source.Height);

        for (int i = 0; i < source.LengthInBytes; i++)
            result[i] = source[i] > threshold ? byte.MaxValue : byte.MinValue;

        return result;
    }

    /// <summary>
    /// Apply binarization per pixels with channel selection
    /// </summary>
    public static Image ApplyPerPixels(Image source, Channel? channel = null)
    {
        var result = new Image(source.Width, source.Height);

        for (int i = 0; i < source.LengthInPixels; i++)
        {
            if (channel.HasValue)
            {
                result[i, channel.Value] = source[i, channel.Value];
            }
            else
            {
                result[i, Channel.B] = source[i, Channel.B];
                result[i, Channel.G] = source[i, Channel.G];
                result[i, Channel.R] = source[i, Channel.R];
            }
        }

        return result;
    }
}


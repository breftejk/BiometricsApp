namespace BiometricsApp.Core.Models;

/// <summary>
/// Represents a color with up to 4 channels
/// </summary>
/// <param name="R">Red channel</param>
/// <param name="G">Green channel</param>
/// <param name="B">Blue channel</param>
/// <param name="A">Alpha channel</param>
public record struct Color(byte R, byte G, byte B, byte A)
{
    public readonly int Sum => R + G + B;
    public readonly double Average => (R + G + B) / 3.0;

    public static implicit operator byte[](Color c) =>
        [c.R, c.G, c.B, c.A];
    
    public static Color FromRgb(byte r, byte g, byte b) => new(r, g, b, 255);
    public static Color FromArgb(byte a, byte r, byte g, byte b) => new(r, g, b, a);
}


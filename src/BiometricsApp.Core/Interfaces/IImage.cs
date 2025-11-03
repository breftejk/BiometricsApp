using System.Drawing;

namespace BiometricsApp.Core.Interfaces;

/// <summary>
/// Core image interface
/// </summary>
public interface IImage
{
    int Channels { get; }
    bool Dirty { get; }
    int Height { get; }
    int Stride { get; }
    int Width { get; }

    IImage Load(string filename);
    IImage Save(string filename);
    IImage Update();

    Bitmap RawBitmap { get; }
}


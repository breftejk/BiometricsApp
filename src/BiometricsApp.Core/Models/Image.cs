using System.Diagnostics;
using SkiaSharp;
using BiometricsApp.Core.Interfaces;
using BiometricsApp.Core.Extensions;

namespace BiometricsApp.Core.Models;

/// <summary>
/// Main image class for processing using SkiaSharp (cross-platform)
/// </summary>
[DebuggerStepThrough]
public class Image : IImage
{
    /// <summary>
    /// Sets or gets i-th byte of an image
    /// </summary>
    /// <param name="i">Index in bytes</param>
    public byte this[int i]
    {
        get => Raw[i];
        set => Set(i, value);
    }

    /// <summary>
    /// Sets or gets the given channel of i-th pixel of an image
    /// </summary>
    /// <param name="i">Index in pixels</param>
    /// <param name="channel">Channel to return</param>
    public byte this[int i, Channel channel]
    {
        get => Raw[i / Width * Stride + i % Width * Channels + (int)channel];
        set => Set(i / Width * Stride + i % Width * Channels + (int)channel, value);
    }

    /// <summary>
    /// Sets or gets the array of channels of pixel at x, y coordinates
    /// </summary>
    /// <param name="x">X in pixels, [0, Width - 1]</param>
    /// <param name="y">Y in pixels, [0, Height - 1]</param>
    public byte[] this[int x, int y]
    {
        get
        {
            int i = x * Channels + y * Stride;

            return
                Channels == 3 ? [Raw[i + 0], Raw[i + 1], Raw[i + 2]] :
                Channels == 4 ? [Raw[i + 0], Raw[i + 1], Raw[i + 2], Raw[i + 3]] :
                throw new NotImplementedException(nameof(Channels));
        }
        set
        {
            int i = x * Channels + y * Stride;
            if (Channels == 3)
            {
                Dirty = true;
                Raw[i + 0] = value[0];
                if (value.Length < 2)
                {
                    Raw[i + 1] = Raw[i + 2] = value[0];
                    return;
                }
                Raw[i + 1] = value[1];
                if (value.Length < 3) return;
                Raw[i + 2] = value[2];
            }
            else if (Channels == 4)
            {
                Dirty = true;
                Raw[i + 0] = value[0];
                if (value.Length < 2)
                {
                    Raw[i + 1] = Raw[i + 2] = value[0];
                    return;
                }
                Raw[i + 1] = value[1];
                if (value.Length < 3) return;
                Raw[i + 2] = value[2];
                if (value.Length < 4) return;
                Raw[i + 3] = value[3];
            }
            else throw new NotImplementedException(nameof(Channels));
        }
    }

    /// <summary>
    /// Sets or gets the value of given channel of pixel at x, y coordinates
    /// </summary>
    /// <param name="x">X in pixels, [0, Width - 1]</param>
    /// <param name="y">Y in pixels, [0, Height - 1]</param>
    /// <param name="channel">Channel index</param>
    public byte this[int x, int y, int channel]
    {
        get => Raw[x * Channels + y * Stride + channel];
        set => Set(x * Channels + y * Stride + channel, value);
    }

    /// <summary>
    /// Sets or gets the value of given channel of pixel at x, y coordinates
    /// </summary>
    /// <param name="x">X in pixels, [0, Width - 1]</param>
    /// <param name="y">Y in pixels, [0, Height - 1]</param>
    /// <param name="channel">Channel to return</param>
    public byte this[int x, int y, Channel channel]
    {
        get => Raw[x * Channels + y * Stride + (int)channel];
        set => Set(x * Channels + y * Stride + (int)channel, value);
    }

    public Image(string filename)
    {
        using var bitmap = SKBitmap.Decode(filename);
        if (bitmap == null)
            throw new InvalidOperationException($"Could not load image: {filename}");
        Load(bitmap);
    }

    public Image(int width, int height, bool hasAlpha = false)
    {
        var colorType = hasAlpha ? SKColorType.Rgba8888 : SKColorType.Rgb888x;
        var bitmap = new SKBitmap(width, height, colorType, SKAlphaType.Premul);
        Load(bitmap);
        _ownsBitmap = true;
    }

    public Image(int[] values, int? height = null, Color? foreground = null, Color? background = null)
    {
        var width = values.Length;
        var h = height ?? values.Max();
        
        var bitmap = new SKBitmap(width, h, SKColorType.Rgba8888, SKAlphaType.Premul);
        Load(bitmap);
        _ownsBitmap = true;

        if (height is not null)
        {
            double hd = (double)height;
            double max = values.Max();
            for (int i = 0; i < values.Length; i++)
                values[i] = (int)(values[i] / max * hd);
        }

        if (background is not null)
            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                    this[x, y] = background.Value;

        foreground ??= Colors.White;

        for (int i = 0; i < Width; i++)
            for (int j = 0; j < values[i]; j++)
                this[i, Height - j - 1] = foreground.Value;
    }

    internal Image(SKBitmap bitmap)
    {
        Load(bitmap);
        _ownsBitmap = false;
    }

    /// <summary>
    /// Loads a new filename.
    /// </summary>
    /// <param name="filename">Filename to load</param>
    /// <returns>This image</returns>
    public IImage Load(string filename)
    {
        if (_ownsBitmap && _bitmap != null)
            _bitmap.Dispose();

        using var bitmap = SKBitmap.Decode(filename);
        if (bitmap == null)
            throw new InvalidOperationException($"Could not load image: {filename}");
        
        Load(bitmap);
        _ownsBitmap = true;
        return this;
    }

    /// <summary>
    /// Saves to a new filename. Updates the bitmap if there was any change to pixels.
    /// </summary>
    /// <param name="filename">Filename to save</param>
    /// <returns>This image</returns>
    public IImage Save(string filename)
    {
        Update();
        
        var format = Path.GetExtension(filename).ToLowerInvariant() switch
        {
            ".png" => SKEncodedImageFormat.Png,
            ".jpg" or ".jpeg" => SKEncodedImageFormat.Jpeg,
            ".bmp" => SKEncodedImageFormat.Bmp,
            ".gif" => SKEncodedImageFormat.Gif,
            ".webp" => SKEncodedImageFormat.Webp,
            _ => SKEncodedImageFormat.Png
        };

        using var image = SKImage.FromBitmap(_bitmap);
        using var data = image.Encode(format, 100);
        using var stream = File.OpenWrite(filename);
        data.SaveTo(stream);
        
        return this;
    }

    /// <summary>
    /// Updates the bitmap if any pixel was changed.
    /// </summary>
    public IImage Update()
    {
        if (!Dirty)
            return this;

        var pixels = _bitmap!.GetPixelSpan();
        Raw.CopyTo(pixels);
        
        Dirty = false;
        return this;
    }

    /// <summary> Width in pixels </summary>
    public int Width { get; private set; }
    
    /// <summary> Height in pixels </summary>
    public int Height { get; private set; }
    
    /// <summary> Number of channels </summary>
    public int Channels { get; private set; }

    /// <summary> Width in bytes </summary>
    public int Stride => Width * Channels;

    /// <summary> Length in bytes </summary>
    public int LengthInBytes => Raw.Length;

    /// <summary> Length in pixels</summary>
    public int LengthInPixels => Width * Height;

    /// <summary> True if any pixel was changed and the image was not updated; false otherwise </summary>
    public bool Dirty { get; private set; } = false;

    public SKBitmap RawBitmap => _bitmap!;

    internal byte[] Raw { get; set; } = [];
    private SKBitmap? _bitmap;
    private bool _ownsBitmap;

    private void Load(SKBitmap bitmap)
    {
        Dirty = false;
        
        // Create a copy to ensure we have the right format
        _bitmap = new SKBitmap(bitmap.Width, bitmap.Height, SKColorType.Rgba8888, SKAlphaType.Premul);
        using var canvas = new SKCanvas(_bitmap);
        canvas.DrawBitmap(bitmap, 0, 0);
        
        Width = _bitmap.Width;
        Height = _bitmap.Height;
        Channels = 4; // RGBA

        Raw = new byte[Width * Height * Channels];
        var pixels = _bitmap.GetPixelSpan();
        pixels.CopyTo(Raw);
    }

    private void Set(int i, byte value)
    {
        if (Raw.Length <= i)
            throw new IndexOutOfRangeException(
                $"Index was outside the bounds of the array. " +
                $"Attempted to access {i} but the maximum length is {LengthInBytes} ({LengthInPixels} pixels * {Channels} channels)"
            );
        Raw[i] = value;
        Dirty = true;
    }

    public void Dispose()
    {
        if (_ownsBitmap && _bitmap != null)
        {
            _bitmap.Dispose();
            _bitmap = null;
        }
    }
}


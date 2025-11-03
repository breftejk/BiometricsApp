using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using BiometricsApp.Core.Interfaces;
using BiometricsApp.Core.Extensions;

namespace BiometricsApp.Core.Models;

/// <summary>
/// Main image class for processing
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
                Channels == 3 ? [Raw[i + 2], Raw[i + 1], Raw[i + 0]] :
                Channels == 4 ? [Raw[i + 2], Raw[i + 1], Raw[i + 0], Raw[i + 3]] :
                throw new NotImplementedException(nameof(Channels));
        }
        set
        {
            int i = x * Channels + y * Stride;
            if (Channels == 3)
            {
                Dirty = true;
                Raw[i + 2] = value[0];
                if (value.Length < 2)
                {
                    Raw[i + 1] = Raw[i + 0] = value[0];
                    return;
                }
                Raw[i + 1] = value[1];
                if (value.Length < 3) return;
                Raw[i + 0] = value[2];
            }
            else if (Channels == 4)
            {
                Dirty = true;
                Raw[i + 2] = value[0];
                if (value.Length < 2)
                {
                    Raw[i + 1] = Raw[i + 0] = value[0];
                    return;
                }
                Raw[i + 1] = value[1];
                if (value.Length < 3) return;
                Raw[i + 0] = value[2];
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

    public Image(string filename) : this(new Bitmap(filename)) { }

    public Image(int width, int height, bool hasAlpha = false) :
        this(new Bitmap(width, height,
            hasAlpha ? PixelFormat.Format32bppArgb : PixelFormat.Format24bppRgb))
    { }

    public Image(int[] values, int? height = null, Color? foreground = null, Color? background = null) :
        this(new Bitmap(values.Length, height ?? values.Max()))
    {
        if (height is not null)
        {
            double h = (double)height;
            double max = values.Max();
            for (int i = 0; i < values.Length; i++)
                values[i] = (int)(values[i] / max * h);
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

    internal Image(Bitmap bmp) => Load(bmp);

    /// <summary>
    /// Loads a new filename.
    /// </summary>
    /// <param name="filename">Filename to load</param>
    /// <returns>This image</returns>
    public IImage Load(string filename)
    {
        Load(new Bitmap(filename));
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
        _bmp.Save(filename);
        return this;
    }

    /// <summary>
    /// Updates the bitmap if any pixel was changed.
    /// </summary>
    public IImage Update()
    {
        if (!Dirty)
            return this;
        
        BitmapData data = LockBits(_bmp, ImageLockMode.WriteOnly);
        Marshal.Copy(Raw, 0, data.Scan0, Raw.Length);
        _bmp.UnlockBits(data);
        
        Dirty = false;
        return this;
    }

    /// <summary> Width in pixels </summary>
    public int Width { get; internal set; }
    
    /// <summary> Height in pixels </summary>
    public int Height { get; internal set; }
    
    /// <summary> Number of channels </summary>
    public int Channels { get; internal set; }

    /// <summary> Width in bytes </summary>
    public int Stride => Width * Channels;

    /// <summary> Length in bytes </summary>
    public int LengthInBytes => Raw.Length;

    /// <summary> Length in pixels</summary>
    public int LengthInPixels => Width * Height;

    /// <summary> True if any pixel was changed and the image was not updated; false otherwise </summary>
    public bool Dirty { get; private set; } = false;

    public Bitmap RawBitmap => _bmp;

    internal byte[] Raw { get; set; } = [];
    internal Bitmap _bmp = null!;

    private void Load(Bitmap bmp)
    {
        Dirty = false;
        _bmp = bmp;

        int channels = System.Drawing.Image.GetPixelFormatSize(_bmp.PixelFormat) >> 3;

        BitmapData data = LockBits(_bmp, ImageLockMode.ReadOnly);

        Raw = new byte[data.Width * data.Height * channels];
        Marshal.Copy(data.Scan0, Raw, 0, Raw.Length);

        bmp.UnlockBits(data);

        Width = bmp.Width;
        Height = bmp.Height;
        Channels = channels;
    }

    private static BitmapData LockBits(Bitmap bmp, ImageLockMode lockMode) =>
        bmp.LockBits(new Rectangle(Point.Empty, bmp.Size), lockMode, bmp.PixelFormat);

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
}


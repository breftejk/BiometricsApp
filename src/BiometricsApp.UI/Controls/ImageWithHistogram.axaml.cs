using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media.Imaging;

namespace BiometricsApp.UI.Controls;

public partial class ImageWithHistogram : UserControl
{
    public static readonly StyledProperty<string> TitleProperty =
        AvaloniaProperty.Register<ImageWithHistogram, string>(nameof(Title), "Image");

    public static readonly StyledProperty<Bitmap?> ImageSourceProperty =
        AvaloniaProperty.Register<ImageWithHistogram, Bitmap?>(nameof(ImageSource));

    public static readonly StyledProperty<Bitmap?> HistogramRedProperty =
        AvaloniaProperty.Register<ImageWithHistogram, Bitmap?>(nameof(HistogramRed));

    public static readonly StyledProperty<Bitmap?> HistogramGreenProperty =
        AvaloniaProperty.Register<ImageWithHistogram, Bitmap?>(nameof(HistogramGreen));

    public static readonly StyledProperty<Bitmap?> HistogramBlueProperty =
        AvaloniaProperty.Register<ImageWithHistogram, Bitmap?>(nameof(HistogramBlue));

    public static readonly StyledProperty<Bitmap?> HistogramAverageProperty =
        AvaloniaProperty.Register<ImageWithHistogram, Bitmap?>(nameof(HistogramAverage));

    public static readonly StyledProperty<bool> IsInteractiveProperty =
        AvaloniaProperty.Register<ImageWithHistogram, bool>(nameof(IsInteractive), false);

    public static readonly StyledProperty<Cursor?> DrawingCursorProperty =
        AvaloniaProperty.Register<ImageWithHistogram, Cursor?>(nameof(DrawingCursor), new Cursor(StandardCursorType.Cross));

    public static readonly StyledProperty<int> BrushSizeProperty =
        AvaloniaProperty.Register<ImageWithHistogram, int>(nameof(BrushSize), 3);

    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public Bitmap? ImageSource
    {
        get => GetValue(ImageSourceProperty);
        set => SetValue(ImageSourceProperty, value);
    }

    public Bitmap? HistogramRed
    {
        get => GetValue(HistogramRedProperty);
        set => SetValue(HistogramRedProperty, value);
    }

    public Bitmap? HistogramGreen
    {
        get => GetValue(HistogramGreenProperty);
        set => SetValue(HistogramGreenProperty, value);
    }

    public Bitmap? HistogramBlue
    {
        get => GetValue(HistogramBlueProperty);
        set => SetValue(HistogramBlueProperty, value);
    }

    public Bitmap? HistogramAverage
    {
        get => GetValue(HistogramAverageProperty);
        set => SetValue(HistogramAverageProperty, value);
    }

    public bool IsInteractive
    {
        get => GetValue(IsInteractiveProperty);
        set => SetValue(IsInteractiveProperty, value);
    }

    public Cursor? DrawingCursor
    {
        get => GetValue(DrawingCursorProperty);
        set => SetValue(DrawingCursorProperty, value);
    }

    public int BrushSize
    {
        get => GetValue(BrushSizeProperty);
        set => SetValue(BrushSizeProperty, value);
    }

    // Drawing events
    public event EventHandler<DrawingEventArgs>? Drawing;
    public event EventHandler<DrawingEventArgs>? DrawingStarted;
    public event EventHandler<DrawingEventArgs>? DrawingEnded;
    public event EventHandler<CanvasClickEventArgs>? CanvasClicked;

    private bool _isDrawing;
    private Point? _lastPoint;
    private Border? _drawingOverlay;
    private Image? _displayImage;
    private Viewbox? _imageViewbox;
    private readonly List<Point> _currentStroke = new();

    public ImageWithHistogram()
    {
        InitializeComponent();
    }

    protected override void OnLoaded(Avalonia.Interactivity.RoutedEventArgs e)
    {
        base.OnLoaded(e);
        
        _drawingOverlay = this.FindControl<Border>("DrawingOverlay");
        _displayImage = this.FindControl<Image>("DisplayImage");
        _imageViewbox = this.FindControl<Viewbox>("ImageViewbox");

        if (_drawingOverlay != null)
        {
            _drawingOverlay.PointerPressed += OnPointerPressed;
            _drawingOverlay.PointerMoved += OnPointerMoved;
            _drawingOverlay.PointerReleased += OnPointerReleased;
            _drawingOverlay.PointerCaptureLost += OnPointerCaptureLost;
        }
    }

    private void InitializeComponent()
    {
        Avalonia.Markup.Xaml.AvaloniaXamlLoader.Load(this);
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!IsInteractive || _drawingOverlay == null)
            return;

        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            var point = e.GetPosition(_imageViewbox);
            var imageCoords = TransformToImageCoordinates(point);

            if (imageCoords.HasValue)
            {
                // Always fire click event for click-based tools (flood fill)
                CanvasClicked?.Invoke(this, new CanvasClickEventArgs(imageCoords.Value));

                // Start drawing for freehand tools
                _isDrawing = true;
                _currentStroke.Clear();
                _lastPoint = imageCoords;
                _currentStroke.Add(imageCoords.Value);
                DrawingStarted?.Invoke(this, new DrawingEventArgs(imageCoords.Value, imageCoords.Value, BrushSize));
            }

            e.Pointer.Capture(_drawingOverlay);
        }
    }

    private void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (!_isDrawing || !IsInteractive || _lastPoint == null)
            return;

        var point = e.GetPosition(_imageViewbox);
        var imageCoords = TransformToImageCoordinates(point);

        if (imageCoords.HasValue)
        {
            _currentStroke.Add(imageCoords.Value);
            Drawing?.Invoke(this, new DrawingEventArgs(_lastPoint.Value, imageCoords.Value, BrushSize));
            _lastPoint = imageCoords;
        }
    }

    private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (_isDrawing && IsInteractive)
        {
            _isDrawing = false;

            if (_lastPoint.HasValue && _currentStroke.Count > 0)
            {
                DrawingEnded?.Invoke(this, new DrawingEventArgs(_lastPoint.Value, _lastPoint.Value, BrushSize, new List<Point>(_currentStroke)));
            }

            _lastPoint = null;
            _currentStroke.Clear();
        }

        e.Pointer.Capture(null);
    }

    private void OnPointerCaptureLost(object? sender, PointerCaptureLostEventArgs e)
    {
        if (_isDrawing)
        {
            _isDrawing = false;
            _lastPoint = null;
            _currentStroke.Clear();
        }
    }

    /// <summary>
    /// Transform screen coordinates to image coordinates
    /// </summary>
    private Point? TransformToImageCoordinates(Point screenPoint)
    {
        if (_imageViewbox == null || ImageSource == null)
            return null;

        var imageWidth = ImageSource.PixelSize.Width;
        var imageHeight = ImageSource.PixelSize.Height;
        var controlWidth = _imageViewbox.Bounds.Width;
        var controlHeight = _imageViewbox.Bounds.Height;

        if (controlWidth <= 0 || controlHeight <= 0 || imageWidth <= 0 || imageHeight <= 0)
            return null;

        // Calculate the actual rendered image size within the Viewbox (Uniform stretch)
        double scaleX = controlWidth / imageWidth;
        double scaleY = controlHeight / imageHeight;
        double scale = Math.Min(scaleX, scaleY);

        double renderedWidth = imageWidth * scale;
        double renderedHeight = imageHeight * scale;

        // Calculate offset (centered in the Viewbox)
        double offsetX = (controlWidth - renderedWidth) / 2;
        double offsetY = (controlHeight - renderedHeight) / 2;

        // Transform screen coordinates to image coordinates
        double imageX = (screenPoint.X - offsetX) / scale;
        double imageY = (screenPoint.Y - offsetY) / scale;

        // Check if within image bounds
        if (imageX < 0 || imageX >= imageWidth || imageY < 0 || imageY >= imageHeight)
            return null;

        return new Point(Math.Floor(imageX), Math.Floor(imageY));
    }
}

public class DrawingEventArgs : EventArgs
{
    public Point From { get; }
    public Point To { get; }
    public int BrushSize { get; }
    public IReadOnlyList<Point>? StrokePoints { get; }

    public DrawingEventArgs(Point from, Point to, int brushSize, IReadOnlyList<Point>? strokePoints = null)
    {
        From = from;
        To = to;
        BrushSize = brushSize;
        StrokePoints = strokePoints;
    }
}

public class CanvasClickEventArgs : EventArgs
{
    public Point Position { get; }

    public CanvasClickEventArgs(Point position)
    {
        Position = position;
    }
}


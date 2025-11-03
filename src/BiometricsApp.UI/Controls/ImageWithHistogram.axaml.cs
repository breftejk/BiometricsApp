using Avalonia;
using Avalonia.Controls;
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

    public ImageWithHistogram()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        Avalonia.Markup.Xaml.AvaloniaXamlLoader.Load(this);
    }
}


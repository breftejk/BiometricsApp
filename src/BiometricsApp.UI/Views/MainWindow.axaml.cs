using Avalonia.Controls;
using BiometricsApp.UI.Controls;
using BiometricsApp.UI.ViewModels;

namespace BiometricsApp.UI.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        
        // Wire up drawing events when the window is loaded
        Loaded += OnLoaded;
    }

    private void OnLoaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var processedImageControl = this.FindControl<ImageWithHistogram>("ProcessedImageControl");
        var originalImageInteractive = this.FindControl<ImageWithHistogram>("OriginalImageInteractive");

        if (processedImageControl != null)
        {
            processedImageControl.DrawingStarted += OnDrawingStarted;
            processedImageControl.Drawing += OnDrawing;
            processedImageControl.DrawingEnded += OnDrawingEnded;
            processedImageControl.CanvasClicked += OnCanvasClicked;
        }

        if (originalImageInteractive != null)
        {
            originalImageInteractive.DrawingStarted += OnDrawingStarted;
            originalImageInteractive.Drawing += OnDrawing;
            originalImageInteractive.DrawingEnded += OnDrawingEnded;
            originalImageInteractive.CanvasClicked += OnCanvasClicked;
        }
    }

    private void OnDrawingStarted(object? sender, DrawingEventArgs e)
    {
        if (DataContext is MainWindowViewModel vm)
        {
            vm.OnDrawingStarted((int)e.From.X, (int)e.From.Y);
        }
    }

    private void OnDrawing(object? sender, DrawingEventArgs e)
    {
        if (DataContext is MainWindowViewModel vm)
        {
            vm.OnDrawing((int)e.From.X, (int)e.From.Y, (int)e.To.X, (int)e.To.Y);
        }
    }

    private void OnDrawingEnded(object? sender, DrawingEventArgs e)
    {
        if (DataContext is MainWindowViewModel vm)
        {
            vm.OnDrawingEnded();
        }
    }

    private void OnCanvasClicked(object? sender, CanvasClickEventArgs e)
    {
        if (DataContext is MainWindowViewModel vm)
        {
            vm.OnCanvasClicked((int)e.Position.X, (int)e.Position.Y);
        }
    }
}
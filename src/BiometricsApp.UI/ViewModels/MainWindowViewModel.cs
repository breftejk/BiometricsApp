﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BiometricsApp.Core.Models;
using BiometricsApp.Algorithms.Binarization;
using BiometricsApp.Algorithms.Histogram;
using BiometricsApp.Algorithms.Adjustments;
using BiometricsApp.Algorithms.Filters;
using BiometricsApp.Algorithms.Selection;
using BiometricsApp.Algorithms.Drawing;

namespace BiometricsApp.UI.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    private Bitmap? _originalImage;

    [ObservableProperty]
    private Bitmap? _processedImage;

    [ObservableProperty]
    private Bitmap? _histogramRedOriginal;

    [ObservableProperty]
    private Bitmap? _histogramGreenOriginal;

    [ObservableProperty]
    private Bitmap? _histogramBlueOriginal;

    [ObservableProperty]
    private Bitmap? _histogramAverageOriginal;

    [ObservableProperty]
    private Bitmap? _histogramRedProcessed;

    [ObservableProperty]
    private Bitmap? _histogramGreenProcessed;

    [ObservableProperty]
    private Bitmap? _histogramBlueProcessed;

    [ObservableProperty]
    private Bitmap? _histogramAverageProcessed;

    [ObservableProperty]
    private int _binarizationThreshold = 128;

    [ObservableProperty]
    private string _selectedChannel = "Average";

    [ObservableProperty]
    private int _brightnessValue = 0;

    [ObservableProperty]
    private double _contrastValue = 1.0;

    [ObservableProperty]
    private int _stretchMin = 0;

    [ObservableProperty]
    private int _stretchMax = 255;

    [ObservableProperty]
    private int _otsuThreshold = 128;

    [ObservableProperty]
    private double _niblackK = -0.2;

    [ObservableProperty]
    private int _niblackWindowSize = 15;

    [ObservableProperty]
    private double _sauvolaK = 0.5;

    [ObservableProperty]
    private int _sauvolaWindowSize = 15;

    [ObservableProperty]
    private double _phansalkarK = 0.25;

    [ObservableProperty]
    private int _phansalkarWindowSize = 15;

    [ObservableProperty]
    private int _bernsenWindowSize = 31;

    [ObservableProperty]
    private int _bernsenContrastThreshold = 15;

    [ObservableProperty]
    private int _kapurThreshold = 128;

    [ObservableProperty]
    private int _liWuThreshold = 128;

    [ObservableProperty]
    private int _adaptiveGradientWindowSize = 15;

    [ObservableProperty]
    private double _adaptiveGradientWeight = 0.3;

    [ObservableProperty]
    private string _selectedConvolutionKernel = "Gaussian Blur 3x3";

    [ObservableProperty]
    private int _medianFilterSize = 3;

    [ObservableProperty]
    private int _pixelizationSize = 10;

    [ObservableProperty]
    private int _kuwaharaWindowSize = 5;

    [ObservableProperty]
    private int _predatorPixelSize = 10;

    [ObservableProperty]
    private bool _predatorColorGrade = true;

    // Magic Wand / Flood Fill properties
    [ObservableProperty]
    private int _magicWandToleranceMin = 0;

    [ObservableProperty]
    private int _magicWandToleranceMax = 32;

    [ObservableProperty]
    private int _magicWandMaxPixels = 0;

    [ObservableProperty]
    private bool _magicWandGlobalMode = false;

    [ObservableProperty]
    private bool _magicWandEightConnectivity = false;

    [ObservableProperty]
    private byte _fillColorR = 255;

    [ObservableProperty]
    private byte _fillColorG = 0;

    [ObservableProperty]
    private byte _fillColorB = 0;

    // Morphological operations properties
    [ObservableProperty]
    private int _morphKernelSize = 3;

    [ObservableProperty]
    private string _selectedMorphShape = "Square";

    // Pencil tool properties
    [ObservableProperty]
    private int _pencilSize = 3;

    [ObservableProperty]
    private byte _pencilColorR = 0;

    [ObservableProperty]
    private byte _pencilColorG = 0;

    [ObservableProperty]
    private byte _pencilColorB = 0;

    [ObservableProperty]
    private bool _isProcessing;

    [ObservableProperty]
    private string _statusText = "Ready";

    [ObservableProperty]
    private string _selectedOperation = "Threshold Binarization";

    private Image? _currentImage;
    private Image? _originalImageData;
    private string? _originalImagePath;
    private Stack<Image> _undoStack = new();
    private const int MaxUndoStackSize = 20;

    public List<string> Channels { get; } = new() { "Average", "Red", "Green", "Blue" };
    
    public List<string> ConvolutionKernels { get; } = new()
    {
        "Gaussian Blur 3x3",
        "Gaussian Blur 5x5",
        "Prewitt",
        "Sobel",
        "Sobel Magnitude",
        "Laplacian 4",
        "Laplacian 8",
        "Sharpen",
        "Edge Detect",
        "Emboss"
    };

    public List<string> MorphShapes { get; } = new() { "Square", "Cross", "Circle" };
    
    public List<string> Operations { get; } = new()
    {
        "Threshold Binarization",
        "Otsu Binarization",
        "Niblack Binarization",
        "Sauvola Binarization",
        "Phansalkar Binarization",
        "Kapur Binarization",
        "Li-Wu Binarization",
        "Bernsen Binarization",
        "Adaptive Gradient Binarization",
        "Convolution Filter",
        "Median Filter",
        "Pixelization",
        "Kuwahara Filter",
        "Predator Filter",
        "Histogram Stretching",
        "Histogram Equalization",
        "Brightness Adjustment",
        "Contrast Adjustment",
        "Pencil Drawing",
        "Magic Wand Selection",
        "Flood Fill",
        "Dilation",
        "Erosion",
        "Watershed Segmentation"
    };

    public MainWindowViewModel()
    {
    }

    [RelayCommand]
    private async Task LoadImage()
    {
        try
        {
            var topLevel = Avalonia.Application.Current?.ApplicationLifetime is 
                Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop
                ? desktop.MainWindow
                : null;

            if (topLevel == null) return;

            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Open Image",
                AllowMultiple = false,
                FileTypeFilter = new[]
                {
                    new FilePickerFileType("Images")
                    {
                        Patterns = new[] { "*.png", "*.jpg", "*.jpeg", "*.bmp", "*.gif" }
                    }
                }
            });

            if (files.Count > 0)
            {
                var file = files[0];
                var path = file.Path.LocalPath;

                IsProcessing = true;
                StatusText = "Loading image...";

                await Task.Run(() =>
                {
                    _originalImageData = new Image(path);
                    _currentImage = new Image(path);
                });

                _originalImagePath = path;
                OriginalImage = LoadBitmap(path);
                ProcessedImage = null;
                _undoStack.Clear();
                UndoCommand.NotifyCanExecuteChanged();

                await UpdateHistograms();

                StatusText = _currentImage != null 
                    ? $"Image loaded: {_currentImage.Width}x{_currentImage.Height}"
                    : "Image loaded";
                IsProcessing = false;
            }
        }
        catch (Exception ex)
        {
            StatusText = $"Error loading image: {ex.Message}";
            IsProcessing = false;
        }
    }

    [RelayCommand]
    private async Task SaveImage()
    {
        if (ProcessedImage == null)
        {
            StatusText = "No processed image to save";
            return;
        }

        try
        {
            var topLevel = Avalonia.Application.Current?.ApplicationLifetime is 
                Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop
                ? desktop.MainWindow
                : null;

            if (topLevel == null) return;

            var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Save Image",
                DefaultExtension = "png",
                FileTypeChoices = new[]
                {
                    new FilePickerFileType("PNG Image") { Patterns = new[] { "*.png" } },
                    new FilePickerFileType("JPEG Image") { Patterns = new[] { "*.jpg", "*.jpeg" } }
                }
            });

            if (file != null)
            {
                var path = file.Path.LocalPath;
                
                IsProcessing = true;
                StatusText = "Saving image...";

                // Save the current processed image
                await Task.Run(() =>
                {
                    if (_currentImage != null)
                    {
                        _currentImage.Save(path);
                    }
                });

                StatusText = $"Image saved: {path}";
                IsProcessing = false;
            }
        }
        catch (Exception ex)
        {
            StatusText = $"Error saving image: {ex.Message}";
            IsProcessing = false;
        }
    }

    [RelayCommand]
    private async Task ApplyBinarization()
    {
        if (_originalImageData == null)
        {
            StatusText = "Please load an image first";
            return;
        }

        try
        {
            IsProcessing = true;
            StatusText = "Applying binarization...";

            // Push current state to undo stack before making changes
            if (_currentImage != null)
            {
                PushToUndoStack(_currentImage);
            }

            Image result = null!;

            await Task.Run(() =>
            {
                // Always apply to original image, not the current processed one
                result = SelectedChannel switch
                {
                    "Red" => ApplyChannelBinarization(Channel.R, _originalImageData),
                    "Green" => ApplyChannelBinarization(Channel.G, _originalImageData),
                    "Blue" => ApplyChannelBinarization(Channel.B, _originalImageData),
                    _ => ThresholdBinarization.ApplyStandard(_originalImageData, (byte)BinarizationThreshold)
                };
            });

            _currentImage = result;
            ProcessedImage = ConvertToBitmap(result);

            // Update histograms to show processed image histograms
            await UpdateHistograms();

            StatusText = $"Binarization applied (Threshold: {BinarizationThreshold}, Channel: {SelectedChannel})";
            IsProcessing = false;
        }
        catch (Exception ex)
        {
            StatusText = $"Error applying binarization: {ex.Message}";
            IsProcessing = false;
        }
    }

    [RelayCommand]
    private async Task ApplyOperation()
    {
        if (_originalImageData == null)
        {
            StatusText = "Please load an image first";
            return;
        }

        try
        {
            IsProcessing = true;
            StatusText = $"Applying {SelectedOperation}...";

            // Push current state to undo stack before making changes
            if (_currentImage != null)
            {
                PushToUndoStack(_currentImage);
            }

            Image result = null!;
            string details = "";

            await Task.Run(() =>
            {
                result = SelectedOperation switch
                {
                    "Threshold Binarization" => ApplyThresholdBinarization(),
                    "Otsu Binarization" => ApplyOtsuBinarization(out details),
                    "Niblack Binarization" => ApplyNiblackBinarization(),
                    "Sauvola Binarization" => ApplySauvolaBinarization(),
                    "Phansalkar Binarization" => ApplyPhansalkarBinarization(),
                    "Kapur Binarization" => ApplyKapurBinarization(out details),
                    "Li-Wu Binarization" => ApplyLiWuBinarization(out details),
                    "Bernsen Binarization" => ApplyBernsenBinarization(),
                    "Adaptive Gradient Binarization" => ApplyAdaptiveGradientBinarization(),
                    "Convolution Filter" => ApplyConvolutionFilter(),
                    "Median Filter" => ApplyMedianFilter(),
                    "Pixelization" => ApplyPixelization(),
                    "Kuwahara Filter" => ApplyKuwaharaFilter(),
                    "Predator Filter" => ApplyPredatorFilter(),
                    "Histogram Stretching" => ApplyHistogramStretching(),
                    "Histogram Equalization" => ApplyHistogramEqualization(),
                    "Brightness Adjustment" => ApplyBrightnessAdjustment(),
                    "Contrast Adjustment" => ApplyContrastAdjustment(),
                    "Pencil Drawing" => ApplyPencilDrawing(out details),
                    "Magic Wand Selection" => ApplyMagicWandSelection(out details),
                    "Flood Fill" => ApplyFloodFill(out details),
                    "Dilation" => ApplyDilation(),
                    "Erosion" => ApplyErosion(),
                    "Watershed Segmentation" => ApplyWatershedSegmentation(),
                    _ => _originalImageData
                };
            });

            _currentImage = result;
            ProcessedImage = ConvertToBitmap(result);

            // Update histograms to show processed image histograms
            await UpdateHistograms();

            StatusText = string.IsNullOrEmpty(details) 
                ? $"{SelectedOperation} applied" 
                : $"{SelectedOperation} applied - {details}";
            IsProcessing = false;
        }
        catch (Exception ex)
        {
            StatusText = $"Error applying {SelectedOperation}: {ex.Message}";
            IsProcessing = false;
        }
    }

    private Image ApplyThresholdBinarization()
    {
        return SelectedChannel switch
        {
            "Red" => ApplyChannelBinarization(Channel.R, _originalImageData!),
            "Green" => ApplyChannelBinarization(Channel.G, _originalImageData!),
            "Blue" => ApplyChannelBinarization(Channel.B, _originalImageData!),
            _ => ThresholdBinarization.ApplyStandard(_originalImageData!, (byte)BinarizationThreshold)
        };
    }

    private Image ApplyOtsuBinarization(out string details)
    {
        var (result, threshold) = SelectedChannel switch
        {
            "Red" => OtsuBinarization.ApplyToChannel(_originalImageData!, Channel.R),
            "Green" => OtsuBinarization.ApplyToChannel(_originalImageData!, Channel.G),
            "Blue" => OtsuBinarization.ApplyToChannel(_originalImageData!, Channel.B),
            _ => OtsuBinarization.Apply(_originalImageData!)
        };
        
        OtsuThreshold = threshold;
        details = $"Optimal threshold: {threshold}";
        return result;
    }

    private Image ApplyHistogramStretching()
    {
        return HistogramStretching.Apply(_originalImageData!, (byte)StretchMin, (byte)StretchMax);
    }

    private Image ApplyHistogramEqualization()
    {
        return HistogramEqualization.Apply(_originalImageData!);
    }

    private Image ApplyBrightnessAdjustment()
    {
        return ImageAdjustments.AdjustBrightness(_originalImageData!, BrightnessValue);
    }

    private Image ApplyContrastAdjustment()
    {
        return ImageAdjustments.AdjustContrast(_originalImageData!, ContrastValue);
    }

    private Image ApplyNiblackBinarization()
    {
        return NiblackBinarization.Apply(_originalImageData!, NiblackK, NiblackWindowSize);
    }

    private Image ApplySauvolaBinarization()
    {
        return SauvolaBinarization.Apply(_originalImageData!, SauvolaK, 128.0, SauvolaWindowSize);
    }

    private Image ApplyPhansalkarBinarization()
    {
        return PhansalkarBinarization.Apply(_originalImageData!, PhansalkarK, 0.5, 2.0, 10.0, PhansalkarWindowSize);
    }

    private Image ApplyKapurBinarization(out string details)
    {
        var (result, threshold) = KapurBinarization.Apply(_originalImageData!);
        KapurThreshold = threshold;
        details = $"Optimal threshold: {threshold}";
        return result;
    }

    private Image ApplyLiWuBinarization(out string details)
    {
        var (result, threshold) = LiWuBinarization.Apply(_originalImageData!);
        LiWuThreshold = threshold;
        details = $"Optimal threshold: {threshold}";
        return result;
    }

    private Image ApplyBernsenBinarization()
    {
        return BernsenBinarization.Apply(_originalImageData!, BernsenWindowSize, BernsenContrastThreshold);
    }

    private Image ApplyAdaptiveGradientBinarization()
    {
        return AdaptiveGradientBinarization.Apply(_originalImageData!, AdaptiveGradientWindowSize, AdaptiveGradientWeight);
    }

    private Image ApplyConvolutionFilter()
    {
        var kernelType = SelectedConvolutionKernel switch
        {
            "Gaussian Blur 3x3" => ConvolutionFilter.KernelType.GaussianBlur3x3,
            "Gaussian Blur 5x5" => ConvolutionFilter.KernelType.GaussianBlur5x5,
            "Prewitt" => ConvolutionFilter.KernelType.Prewitt,
            "Sobel" => ConvolutionFilter.KernelType.Sobel,
            "Laplacian 4" => ConvolutionFilter.KernelType.Laplacian4,
            "Laplacian 8" => ConvolutionFilter.KernelType.Laplacian8,
            "Sharpen" => ConvolutionFilter.KernelType.Sharpen,
            "Edge Detect" => ConvolutionFilter.KernelType.EdgeDetect,
            "Emboss" => ConvolutionFilter.KernelType.Emboss,
            _ => ConvolutionFilter.KernelType.GaussianBlur3x3
        };

        if (SelectedConvolutionKernel == "Sobel Magnitude")
        {
            return ConvolutionFilter.ApplySobelMagnitude(_originalImageData!);
        }

        return ConvolutionFilter.Apply(_originalImageData!, kernelType);
    }

    private Image ApplyMedianFilter()
    {
        return MedianFilter.Apply(_originalImageData!, MedianFilterSize);
    }

    private Image ApplyPixelization()
    {
        return PixelizationFilter.Apply(_originalImageData!, PixelizationSize);
    }

    private Image ApplyKuwaharaFilter()
    {
        return KuwaharaFilter.Apply(_originalImageData!, KuwaharaWindowSize);
    }

    private Image ApplyPredatorFilter()
    {
        if (PredatorColorGrade)
        {
            return PredatorFilter.ApplyWithColorGrade(_originalImageData!, PredatorPixelSize, true);
        }
        return PredatorFilter.Apply(_originalImageData!, PredatorPixelSize);
    }

    private Image ApplyPencilDrawing(out string details)
    {
        // Create a canvas copy of the original image
        var canvas = PencilTool.CreateDrawingCanvas(_originalImageData!);
        
        // Draw a sample stroke for demonstration
        // In a full implementation, this would be interactive with mouse events
        var pencilColor = new Color(PencilColorR, PencilColorG, PencilColorB, 255);
        
        // Draw diagonal lines across the image as a demo
        int centerX = canvas.Width / 2;
        int centerY = canvas.Height / 2;
        int length = Math.Min(canvas.Width, canvas.Height) / 4;
        
        // Draw a crosshair pattern to demonstrate the tool
        PencilTool.DrawLine(canvas, centerX - length, centerY, centerX + length, centerY, PencilSize, pencilColor);
        PencilTool.DrawLine(canvas, centerX, centerY - length, centerX, centerY + length, PencilSize, pencilColor);
        
        details = $"Drawn with size {PencilSize}, color RGB({PencilColorR}, {PencilColorG}, {PencilColorB})";
        return canvas;
    }

    private Image ApplyMagicWandSelection(out string details)
    {
        // Use center of image as starting point for demo
        int startX = _originalImageData!.Width / 2;
        int startY = _originalImageData!.Height / 2;
        
        var connectivity = MagicWandEightConnectivity 
            ? MagicWandTool.Connectivity.Eight 
            : MagicWandTool.Connectivity.Four;
        
        MagicWandTool.SelectionMask mask;
        if (MagicWandGlobalMode)
        {
            mask = MagicWandTool.SelectGlobal(
                _originalImageData!, 
                startX, startY, 
                MagicWandToleranceMin, MagicWandToleranceMax, 
                MagicWandMaxPixels);
        }
        else
        {
            mask = MagicWandTool.SelectContiguous(
                _originalImageData!, 
                startX, startY, 
                MagicWandToleranceMin, MagicWandToleranceMax, 
                MagicWandMaxPixels, 
                connectivity);
        }
        
        var highlightColor = new Color(0, 120, 212, 255); // Blue highlight
        details = $"Selected {mask.SelectedCount} pixels";
        return MagicWandTool.VisualizeSelection(_originalImageData!, mask, highlightColor, 0.5);
    }

    private Image ApplyFloodFill(out string details)
    {
        // Use center of image as starting point for demo
        int startX = _originalImageData!.Width / 2;
        int startY = _originalImageData!.Height / 2;
        
        var fillColor = new Color(FillColorR, FillColorG, FillColorB, 255);
        var connectivity = MagicWandEightConnectivity 
            ? MagicWandTool.Connectivity.Eight 
            : MagicWandTool.Connectivity.Four;
        
        var result = MagicWandTool.FloodFill(
            _originalImageData!, 
            startX, startY, 
            fillColor,
            MagicWandToleranceMin, MagicWandToleranceMax, 
            MagicWandMaxPixels, 
            connectivity,
            MagicWandGlobalMode);
        
        details = $"Filled with RGB({FillColorR}, {FillColorG}, {FillColorB})";
        return result;
    }

    private Image ApplyDilation()
    {
        var shape = SelectedMorphShape switch
        {
            "Cross" => MorphologicalOperations.KernelShape.Cross,
            "Circle" => MorphologicalOperations.KernelShape.Circle,
            _ => MorphologicalOperations.KernelShape.Square
        };
        
        return MorphologicalOperations.DilateImage(_originalImageData!, MorphKernelSize, shape);
    }

    private Image ApplyErosion()
    {
        var shape = SelectedMorphShape switch
        {
            "Cross" => MorphologicalOperations.KernelShape.Cross,
            "Circle" => MorphologicalOperations.KernelShape.Circle,
            _ => MorphologicalOperations.KernelShape.Square
        };
        
        return MorphologicalOperations.ErodeImage(_originalImageData!, MorphKernelSize, shape);
    }

    private Image ApplyWatershedSegmentation()
    {
        return WatershedSegmentation.Apply(_originalImageData!);
    }

    private Image ApplyChannelBinarization(Channel channel, Image sourceImage)
    {
        var result = new Image(sourceImage.Width, sourceImage.Height);

        for (int x = 0; x < sourceImage.Width; x++)
        {
            for (int y = 0; y < sourceImage.Height; y++)
            {
                byte value = sourceImage[x, y, channel];
                byte binarized = value > BinarizationThreshold ? byte.MaxValue : byte.MinValue;
                
                result[x, y] = new byte[] { binarized, binarized, binarized };
            }
        }

        return result;
    }

    [RelayCommand]
    private async Task UpdateHistograms()
    {
        if (_originalImageData == null) return;

        try
        {
            IsProcessing = true;
            StatusText = "Calculating histograms...";

            int[][] histogramsOriginal = null!;
            int[][]? histogramsProcessed = null;

            await Task.Run(() =>
            {
                // Always calculate histograms for original
                histogramsOriginal = HistogramCalculator.Calculate(_originalImageData);
                
                // Calculate for processed if it exists
                if (_currentImage != null && ProcessedImage != null)
                {
                    histogramsProcessed = HistogramCalculator.Calculate(_currentImage);
                }
            });

            // Create histogram visualizations for original
            var histRedOrig = HistogramCalculator.CreateVisualization(
                histogramsOriginal[0], Colors.Red, Colors.White, 200);
            var histGreenOrig = HistogramCalculator.CreateVisualization(
                histogramsOriginal[1], Colors.Green, Colors.White, 200);
            var histBlueOrig = HistogramCalculator.CreateVisualization(
                histogramsOriginal[2], Colors.Blue, Colors.White, 200);
            var histAvgOrig = HistogramCalculator.CreateVisualization(
                histogramsOriginal[3], Colors.Gray, Colors.White, 200);

            HistogramRedOriginal = ConvertToBitmap(histRedOrig);
            HistogramGreenOriginal = ConvertToBitmap(histGreenOrig);
            HistogramBlueOriginal = ConvertToBitmap(histBlueOrig);
            HistogramAverageOriginal = ConvertToBitmap(histAvgOrig);

            // Create histogram visualizations for processed if available
            if (histogramsProcessed != null)
            {
                var histRedProc = HistogramCalculator.CreateVisualization(
                    histogramsProcessed[0], Colors.Red, Colors.White, 200);
                var histGreenProc = HistogramCalculator.CreateVisualization(
                    histogramsProcessed[1], Colors.Green, Colors.White, 200);
                var histBlueProc = HistogramCalculator.CreateVisualization(
                    histogramsProcessed[2], Colors.Blue, Colors.White, 200);
                var histAvgProc = HistogramCalculator.CreateVisualization(
                    histogramsProcessed[3], Colors.Gray, Colors.White, 200);

                HistogramRedProcessed = ConvertToBitmap(histRedProc);
                HistogramGreenProcessed = ConvertToBitmap(histGreenProc);
                HistogramBlueProcessed = ConvertToBitmap(histBlueProc);
                HistogramAverageProcessed = ConvertToBitmap(histAvgProc);
            }

            StatusText = "Histograms updated";
            IsProcessing = false;
        }
        catch (Exception ex)
        {
            StatusText = $"Error calculating histograms: {ex.Message}";
            IsProcessing = false;
        }
    }

    [RelayCommand]
    private async Task ResetImage()
    {
        if (_originalImagePath == null || _originalImageData == null)
        {
            StatusText = "No original image to reset to";
            return;
        }

        try
        {
            IsProcessing = true;
            StatusText = "Resetting to original image...";

            await Task.Run(() =>
            {
                // Reset current image to a fresh copy of original
                _currentImage = new Image(_originalImagePath);
            });

            // Clear processed image and update histograms with original data
            ProcessedImage = null;
            _undoStack.Clear();
            await UpdateHistograms();
            
            StatusText = "Image reset to original";
            IsProcessing = false;
        }
        catch (Exception ex)
        {
            StatusText = $"Error resetting image: {ex.Message}";
            IsProcessing = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanUndo))]
    private async Task Undo()
    {
        if (_undoStack.Count == 0)
        {
            StatusText = "Nothing to undo";
            return;
        }

        try
        {
            IsProcessing = true;
            StatusText = "Undoing last operation...";

            await Task.Run(() =>
            {
                _currentImage = _undoStack.Pop();
            });

            if (_currentImage != null)
            {
                ProcessedImage = ConvertToBitmap(_currentImage);
                await UpdateHistograms();
            }

            StatusText = $"Operation undone ({_undoStack.Count} operations in history)";
            IsProcessing = false;
            
            UndoCommand.NotifyCanExecuteChanged();
        }
        catch (Exception ex)
        {
            StatusText = $"Error undoing operation: {ex.Message}";
            IsProcessing = false;
        }
    }

    private bool CanUndo() => _undoStack.Count > 0;

    private void PushToUndoStack(Image image)
    {
        if (image == null) return;

        // Create a deep copy of the image to avoid reference issues
        var copy = new Image(image.Width, image.Height);
        for (int x = 0; x < image.Width; x++)
        {
            for (int y = 0; y < image.Height; y++)
            {
                copy[x, y] = image[x, y];
            }
        }

        _undoStack.Push(copy);

        // Limit stack size
        if (_undoStack.Count > MaxUndoStackSize)
        {
            var tempStack = new Stack<Image>();
            for (int i = 0; i < MaxUndoStackSize; i++)
            {
                tempStack.Push(_undoStack.Pop());
            }
            _undoStack.Clear();
            while (tempStack.Count > 0)
            {
                _undoStack.Push(tempStack.Pop());
            }
        }

        UndoCommand.NotifyCanExecuteChanged();
    }

    private Bitmap LoadBitmap(string path)
    {
        return new Bitmap(path);
    }

    private Bitmap ConvertToBitmap(Image image)
    {
        image.Update();
        
        using var skImage = SkiaSharp.SKImage.FromBitmap(image.RawBitmap);
        using var data = skImage.Encode(SkiaSharp.SKEncodedImageFormat.Png, 100);
        using var memoryStream = new MemoryStream();
        data.SaveTo(memoryStream);
        memoryStream.Position = 0;
        
        return new Bitmap(memoryStream);
    }
}

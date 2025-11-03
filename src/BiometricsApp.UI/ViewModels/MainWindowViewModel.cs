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
    private bool _isProcessing;

    [ObservableProperty]
    private string _statusText = "Ready";

    private Image? _currentImage;
    private Image? _originalImageData;
    private string? _originalImagePath;

    public List<string> Channels { get; } = new() { "Average", "Red", "Green", "Blue" };

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

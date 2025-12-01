using BiometricsApp.Core.Models;

namespace BiometricsApp.Algorithms.Selection;

/// <summary>
/// Watershed segmentation algorithm for image segmentation
/// </summary>
public static class WatershedSegmentation
{
    private const int WATERSHED = -1;
    private const int INIT = -2;
    private const int MASK = -3;

    /// <summary>
    /// Apply watershed segmentation to an image
    /// </summary>
    /// <param name="source">Source image (grayscale recommended)</param>
    /// <returns>Segmented image with colored regions</returns>
    public static Image Apply(Image source)
    {
        // Convert to grayscale for gradient calculation
        var grayscale = ConvertToGrayscale(source);
        
        // Calculate gradient magnitude
        var gradient = CalculateGradient(grayscale);
        
        // Apply watershed algorithm
        var labels = Watershed(gradient);
        
        // Create visualization
        return CreateVisualization(source, labels);
    }

    /// <summary>
    /// Apply watershed segmentation with markers
    /// </summary>
    /// <param name="source">Source image</param>
    /// <param name="markers">Marker positions and labels</param>
    /// <returns>Segmented image</returns>
    public static Image ApplyWithMarkers(Image source, int[,] markers)
    {
        var grayscale = ConvertToGrayscale(source);
        var gradient = CalculateGradient(grayscale);
        var labels = WatershedWithMarkers(gradient, markers);
        return CreateVisualization(source, labels);
    }

    /// <summary>
    /// Get watershed labels for further processing
    /// </summary>
    public static int[,] GetLabels(Image source)
    {
        var grayscale = ConvertToGrayscale(source);
        var gradient = CalculateGradient(grayscale);
        return Watershed(gradient);
    }

    private static byte[,] ConvertToGrayscale(Image source)
    {
        var result = new byte[source.Width, source.Height];
        
        for (int y = 0; y < source.Height; y++)
        {
            for (int x = 0; x < source.Width; x++)
            {
                var pixel = source[x, y];
                result[x, y] = (byte)((pixel[0] + pixel[1] + pixel[2]) / 3);
            }
        }
        
        return result;
    }

    private static byte[,] CalculateGradient(byte[,] grayscale)
    {
        int width = grayscale.GetLength(0);
        int height = grayscale.GetLength(1);
        var gradient = new byte[width, height];

        for (int y = 1; y < height - 1; y++)
        {
            for (int x = 1; x < width - 1; x++)
            {
                // Sobel gradient
                int gx = -grayscale[x - 1, y - 1] + grayscale[x + 1, y - 1]
                       - 2 * grayscale[x - 1, y] + 2 * grayscale[x + 1, y]
                       - grayscale[x - 1, y + 1] + grayscale[x + 1, y + 1];

                int gy = -grayscale[x - 1, y - 1] - 2 * grayscale[x, y - 1] - grayscale[x + 1, y - 1]
                       + grayscale[x - 1, y + 1] + 2 * grayscale[x, y + 1] + grayscale[x + 1, y + 1];

                int magnitude = (int)Math.Sqrt(gx * gx + gy * gy);
                gradient[x, y] = (byte)Math.Min(255, magnitude);
            }
        }

        return gradient;
    }

    private static int[,] Watershed(byte[,] gradient)
    {
        int width = gradient.GetLength(0);
        int height = gradient.GetLength(1);
        var labels = new int[width, height];
        
        // Initialize labels
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                labels[x, y] = INIT;

        // Sort pixels by gradient value
        var sortedPixels = new List<(int x, int y, byte value)>();
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                sortedPixels.Add((x, y, gradient[x, y]));
            }
        }
        sortedPixels.Sort((a, b) => a.value.CompareTo(b.value));

        int currentLabel = 0;
        var queue = new Queue<(int x, int y)>();

        int[] dx = { 0, 1, 0, -1, 1, 1, -1, -1 };
        int[] dy = { -1, 0, 1, 0, -1, 1, 1, -1 };

        foreach (var (px, py, value) in sortedPixels)
        {
            if (labels[px, py] != INIT) continue;

            // Check neighbors
            int neighborLabel = INIT;
            bool hasWatershed = false;

            for (int i = 0; i < 8; i++)
            {
                int nx = px + dx[i];
                int ny = py + dy[i];

                if (nx >= 0 && nx < width && ny >= 0 && ny < height)
                {
                    int nl = labels[nx, ny];
                    if (nl > 0)
                    {
                        if (neighborLabel == INIT)
                            neighborLabel = nl;
                        else if (neighborLabel != nl && nl != WATERSHED)
                            hasWatershed = true;
                    }
                    else if (nl == WATERSHED)
                    {
                        hasWatershed = true;
                    }
                }
            }

            if (hasWatershed && neighborLabel != INIT)
            {
                labels[px, py] = WATERSHED;
            }
            else if (neighborLabel != INIT)
            {
                labels[px, py] = neighborLabel;
            }
            else
            {
                // Start new region
                currentLabel++;
                labels[px, py] = currentLabel;
                queue.Enqueue((px, py));

                // Flood fill
                while (queue.Count > 0)
                {
                    var (x, y) = queue.Dequeue();

                    for (int i = 0; i < 8; i++)
                    {
                        int nx = x + dx[i];
                        int ny = y + dy[i];

                        if (nx >= 0 && nx < width && ny >= 0 && ny < height)
                        {
                            if (labels[nx, ny] == INIT && gradient[nx, ny] == value)
                            {
                                labels[nx, ny] = currentLabel;
                                queue.Enqueue((nx, ny));
                            }
                        }
                    }
                }
            }
        }

        return labels;
    }

    private static int[,] WatershedWithMarkers(byte[,] gradient, int[,] markers)
    {
        int width = gradient.GetLength(0);
        int height = gradient.GetLength(1);
        var labels = new int[width, height];

        // Copy markers to labels
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                labels[x, y] = markers[x, y] > 0 ? markers[x, y] : INIT;

        var priorityQueue = new SortedDictionary<byte, Queue<(int x, int y)>>();
        int[] dx = { 0, 1, 0, -1, 1, 1, -1, -1 };
        int[] dy = { -1, 0, 1, 0, -1, 1, 1, -1 };

        // Add boundary pixels of markers to priority queue
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (labels[x, y] > 0)
                {
                    for (int i = 0; i < 8; i++)
                    {
                        int nx = x + dx[i];
                        int ny = y + dy[i];

                        if (nx >= 0 && nx < width && ny >= 0 && ny < height && labels[nx, ny] == INIT)
                        {
                            byte g = gradient[x, y];
                            if (!priorityQueue.ContainsKey(g))
                                priorityQueue[g] = new Queue<(int, int)>();
                            
                            labels[nx, ny] = MASK;
                            priorityQueue[g].Enqueue((nx, ny));
                        }
                    }
                }
            }
        }

        // Process pixels
        while (priorityQueue.Count > 0)
        {
            var firstKey = priorityQueue.Keys.First();
            var queue = priorityQueue[firstKey];
            
            if (queue.Count == 0)
            {
                priorityQueue.Remove(firstKey);
                continue;
            }

            var (px, py) = queue.Dequeue();
            if (queue.Count == 0) priorityQueue.Remove(firstKey);

            // Find neighboring labels
            int neighborLabel = INIT;
            bool hasWatershed = false;

            for (int i = 0; i < 8; i++)
            {
                int nx = px + dx[i];
                int ny = py + dy[i];

                if (nx >= 0 && nx < width && ny >= 0 && ny < height)
                {
                    int nl = labels[nx, ny];
                    if (nl > 0)
                    {
                        if (neighborLabel == INIT)
                            neighborLabel = nl;
                        else if (neighborLabel != nl && nl != WATERSHED)
                            hasWatershed = true;
                    }
                }
            }

            if (hasWatershed)
            {
                labels[px, py] = WATERSHED;
            }
            else if (neighborLabel > 0)
            {
                labels[px, py] = neighborLabel;

                // Add unlabeled neighbors
                for (int i = 0; i < 8; i++)
                {
                    int nx = px + dx[i];
                    int ny = py + dy[i];

                    if (nx >= 0 && nx < width && ny >= 0 && ny < height && labels[nx, ny] == INIT)
                    {
                        byte g = gradient[nx, ny];
                        if (!priorityQueue.ContainsKey(g))
                            priorityQueue[g] = new Queue<(int, int)>();
                        
                        labels[nx, ny] = MASK;
                        priorityQueue[g].Enqueue((nx, ny));
                    }
                }
            }
        }

        return labels;
    }

    private static Image CreateVisualization(Image source, int[,] labels)
    {
        int width = labels.GetLength(0);
        int height = labels.GetLength(1);
        var result = new Image(width, height);

        // Generate colors for each label
        int maxLabel = 0;
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                if (labels[x, y] > maxLabel) maxLabel = labels[x, y];

        var colors = GenerateColors(maxLabel + 1);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int label = labels[x, y];
                
                if (label == WATERSHED)
                {
                    // Draw watershed lines in white
                    result[x, y] = new byte[] { 255, 255, 255 };
                }
                else if (label > 0 && label < colors.Length)
                {
                    // Blend original with label color
                    var pixel = source[x, y];
                    var color = colors[label];
                    byte r = (byte)((pixel[0] * 0.5) + (color.R * 0.5));
                    byte g = (byte)((pixel[1] * 0.5) + (color.G * 0.5));
                    byte b = (byte)((pixel[2] * 0.5) + (color.B * 0.5));
                    result[x, y] = new byte[] { r, g, b };
                }
                else
                {
                    result[x, y] = source[x, y];
                }
            }
        }

        return result;
    }

    private static Color[] GenerateColors(int count)
    {
        var colors = new Color[count];
        colors[0] = Colors.Black;
        
        for (int i = 1; i < count; i++)
        {
            // Generate distinct colors using HSL
            double hue = (i * 137.508) % 360; // Golden angle
            var (r, g, b) = HslToRgb(hue / 360.0, 0.7, 0.5);
            colors[i] = Color.FromRgb((byte)(r * 255), (byte)(g * 255), (byte)(b * 255));
        }
        
        return colors;
    }

    private static (double r, double g, double b) HslToRgb(double h, double s, double l)
    {
        double c = (1 - Math.Abs(2 * l - 1)) * s;
        double x = c * (1 - Math.Abs((h * 6) % 2 - 1));
        double m = l - c / 2;

        double r, g, b;
        int sector = (int)(h * 6);
        
        switch (sector % 6)
        {
            case 0: r = c; g = x; b = 0; break;
            case 1: r = x; g = c; b = 0; break;
            case 2: r = 0; g = c; b = x; break;
            case 3: r = 0; g = x; b = c; break;
            case 4: r = x; g = 0; b = c; break;
            default: r = c; g = 0; b = x; break;
        }

        return (r + m, g + m, b + m);
    }
}

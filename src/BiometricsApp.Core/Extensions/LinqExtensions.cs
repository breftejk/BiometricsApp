using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace BiometricsApp.Core.Extensions;

public static class LinqExtensions
{
    /// <summary>
    /// Returns the average of given byte array.
    /// </summary>
    public static double Average(this byte[] bytes) => bytes.Average(b => b);

    /// <summary>
    /// Returns the sum of given byte array.
    /// </summary>
    public static int Sum(this byte[] bytes) => bytes.Sum(b => b);

    /// <summary>
    /// Computes the median from given array.
    /// </summary>
    public static T Median<T>(this T[] bytes) =>
        bytes.OrderBy(i => i).ElementAt(bytes.Length >> 1);
}


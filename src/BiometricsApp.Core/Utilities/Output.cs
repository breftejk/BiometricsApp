using System.Diagnostics;

namespace BiometricsApp.Core.Utilities;

/// <summary>
/// Outputs objects to the Console and Output window.
/// </summary>
public static class Output
{
    /// <summary>
    /// Writes the text representation of the specified object to the standard output stream and to the trace listeners
    /// </summary>
    /// <param name="line">An object to write</param>
    public static void Write(object line)
    {
        Console.Write(line);
        Debug.Write(line);
    }

    /// <summary>
    /// If condition is true, writes the text representation of the specified object to the standard output stream and to the trace listeners
    /// </summary>
    /// <param name="condition">Condition to check</param>
    /// <param name="line">An object to write</param>
    public static void Write(bool condition, object line)
    {
        if (condition)
            Console.Write(line);
        Debug.WriteIf(condition, line);
    }

    /// <summary>
    /// Writes the text representation of the specified object to the standard output stream and to the trace listeners and causes a line break.
    /// </summary>
    /// <param name="line">An object to write</param>
    public static void WriteLine(object line)
    {
        Console.WriteLine(line);
        Debug.WriteLine(line);
    }

    /// <summary>
    /// If condition is true, writes the text representation of the specified object to the standard output stream and to the trace listeners and causes a line break.
    /// </summary>
    /// <param name="condition">Condition to check</param>
    /// <param name="line">An object to write</param>
    public static void WriteLine(bool condition, object line)
    {
        if (condition)
            Console.WriteLine(line);
        Debug.WriteLineIf(condition, line);
    }
}


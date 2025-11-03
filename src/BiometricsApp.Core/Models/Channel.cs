namespace BiometricsApp.Core.Models;

/// <summary>
/// Represents the RGBA channels in memory order (RGBA8888 format).
/// </summary>
public enum Channel 
{ 
    R = 0,  // Red is at offset 0 in RGBA8888
    G = 1,  // Green is at offset 1
    B = 2,  // Blue is at offset 2
    A = 3   // Alpha is at offset 3
}


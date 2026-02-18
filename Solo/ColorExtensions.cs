using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace Solo;

public static class ColorExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Color Multiply(this Color a, Color b)
    {
        return new Color(
            (byte)(a.R * b.R / 255),
            (byte)(a.G * b.G / 255),
            (byte)(a.B * b.B / 255),
            (byte)(a.A * b.A / 255));
    }
}

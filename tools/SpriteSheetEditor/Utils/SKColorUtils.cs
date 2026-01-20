using SkiaSharp;

namespace SpriteSheetEditor.Utils;

public static class SKColorUtils
{
    public static bool TryParseHex(string? hexText, out SKColor color)
    {
        color = SKColors.Magenta;

        if (string.IsNullOrWhiteSpace(hexText))
            return false;

        var hex = hexText.TrimStart('#');

        if (hex.Length == 6)
        {
            if (byte.TryParse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber, null, out var r) &&
                byte.TryParse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber, null, out var g) &&
                byte.TryParse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber, null, out var b))
            {
                color = new SKColor(r, g, b);
                return true;
            }
        }
        else if (hex.Length == 8)
        {
            if (byte.TryParse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber, null, out var a) &&
                byte.TryParse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber, null, out var r) &&
                byte.TryParse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber, null, out var g) &&
                byte.TryParse(hex.Substring(6, 2), System.Globalization.NumberStyles.HexNumber, null, out var b))
            {
                color = new SKColor(r, g, b, a);
                return true;
            }
        }

        return false;
    }

    public static bool IsNearPoint(SKPoint a, SKPoint b, float threshold)
    {
        return Math.Abs(a.X - b.X) <= threshold && Math.Abs(a.Y - b.Y) <= threshold;
    }
}

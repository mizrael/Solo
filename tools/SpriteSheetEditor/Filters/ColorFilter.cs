using System.Runtime.InteropServices;
using SkiaSharp;

namespace SpriteSheetEditor.Filters;

public enum BackgroundRemovalMode
{
    Hard,       // Original binary transparent/opaque
    SoftAlpha,  // Gradual alpha based on distance
    ChromaKey   // HSV-based removal
}

public static class ColorFilter
{
    private const float MaxRgbDistance = 441.6729559300637f; // sqrt(255^2 + 255^2 + 255^2)

    public static SKBitmap ApplyColorToTransparent(SKBitmap source, SKColor targetColor, float tolerance)
    {
        var width = source.Width;
        var height = source.Height;
        var result = new SKBitmap(width, height, SKColorType.Bgra8888, SKAlphaType.Premul);

        // Pre-calculate the squared tolerance threshold to avoid sqrt in the loop
        var toleranceThreshold = tolerance * MaxRgbDistance;
        var toleranceThresholdSquared = (int)(toleranceThreshold * toleranceThreshold);

        var targetR = (int)targetColor.Red;
        var targetG = (int)targetColor.Green;
        var targetB = (int)targetColor.Blue;

        // Copy source pixels to array for parallel processing
        var pixelCount = width * height;
        var pixels = new uint[pixelCount];
        var sourceSpan = source.GetPixelSpan();
        MemoryMarshal.Cast<byte, uint>(sourceSpan).CopyTo(pixels);

        // Process rows in parallel for better performance on large images
        Parallel.For(0, height, y =>
        {
            var rowStart = y * width;
            for (var x = 0; x < width; x++)
            {
                var idx = rowStart + x;
                var pixel = pixels[idx];

                // BGRA format: pixel = (A << 24) | (R << 16) | (G << 8) | B
                var a = (pixel >> 24) & 0xFF;

                // Skip already transparent pixels
                if (a == 0) continue;

                var b = (int)(pixel & 0xFF);
                var g = (int)((pixel >> 8) & 0xFF);
                var r = (int)((pixel >> 16) & 0xFF);

                // Calculate squared distance (avoid sqrt for performance)
                var dr = r - targetR;
                var dg = g - targetG;
                var db = b - targetB;
                var distanceSquared = dr * dr + dg * dg + db * db;

                if (distanceSquared <= toleranceThresholdSquared)
                {
                    // Set alpha to 0 (make transparent), keep RGB
                    pixels[idx] = pixel & 0x00FFFFFF;
                }
            }
        });

        // Copy processed pixels back to result bitmap
        var resultPixels = result.GetPixels();
        Marshal.Copy(MemoryMarshal.AsBytes<uint>(pixels).ToArray(), 0, resultPixels, pixelCount * 4);

        return result;
    }

    public static SKBitmap ApplyColorToTransparentSoft(SKBitmap source, SKColor targetColor, float tolerance)
    {
        var width = source.Width;
        var height = source.Height;
        var result = new SKBitmap(width, height, SKColorType.Bgra8888, SKAlphaType.Premul);

        var toleranceThreshold = tolerance * MaxRgbDistance;
        var targetR = (int)targetColor.Red;
        var targetG = (int)targetColor.Green;
        var targetB = (int)targetColor.Blue;

        var pixelCount = width * height;
        var pixels = new uint[pixelCount];
        var sourceSpan = source.GetPixelSpan();
        MemoryMarshal.Cast<byte, uint>(sourceSpan).CopyTo(pixels);

        Parallel.For(0, height, y =>
        {
            var rowStart = y * width;
            for (var x = 0; x < width; x++)
            {
                var idx = rowStart + x;
                var pixel = pixels[idx];

                var a = (int)((pixel >> 24) & 0xFF);
                if (a == 0) continue;

                var b = (int)(pixel & 0xFF);
                var g = (int)((pixel >> 8) & 0xFF);
                var r = (int)((pixel >> 16) & 0xFF);

                var dr = r - targetR;
                var dg = g - targetG;
                var db = b - targetB;
                var distance = MathF.Sqrt(dr * dr + dg * dg + db * db);

                if (distance <= toleranceThreshold)
                {
                    // Gradual alpha: 0 at exact match, fades to original at tolerance edge
                    var factor = distance / toleranceThreshold;
                    var newAlpha = (int)(a * factor);
                    pixels[idx] = (uint)((newAlpha << 24) | (r << 16) | (g << 8) | b);
                }
            }
        });

        var resultPixels = result.GetPixels();
        Marshal.Copy(MemoryMarshal.AsBytes<uint>(pixels).ToArray(), 0, resultPixels, pixelCount * 4);

        return result;
    }

    public static SKBitmap ApplyChromaKey(SKBitmap source, SKColor targetColor, float hueTolerance, float satTolerance = 0.3f)
    {
        var width = source.Width;
        var height = source.Height;
        var result = new SKBitmap(width, height, SKColorType.Bgra8888, SKAlphaType.Premul);

        targetColor.ToHsv(out var targetH, out var targetS, out var targetV);

        var pixelCount = width * height;
        var pixels = new uint[pixelCount];
        var sourceSpan = source.GetPixelSpan();
        MemoryMarshal.Cast<byte, uint>(sourceSpan).CopyTo(pixels);

        // Hue tolerance in degrees (0-360), scale from 0-1 input
        var hueThreshold = hueTolerance * 180f; // Max 180 degrees difference
        var satThreshold = satTolerance;

        Parallel.For(0, height, y =>
        {
            var rowStart = y * width;
            for (var x = 0; x < width; x++)
            {
                var idx = rowStart + x;
                var pixel = pixels[idx];

                var a = (int)((pixel >> 24) & 0xFF);
                if (a == 0) continue;

                var b = (byte)(pixel & 0xFF);
                var g = (byte)((pixel >> 8) & 0xFF);
                var r = (byte)((pixel >> 16) & 0xFF);

                var pixelColor = new SKColor(r, g, b);
                pixelColor.ToHsv(out var h, out var s, out var v);

                // Calculate hue distance (circular, 0-360)
                var hueDiff = MathF.Abs(h - targetH);
                if (hueDiff > 180f) hueDiff = 360f - hueDiff;

                // Check if within hue and saturation tolerance
                var satDiff = MathF.Abs(s - targetS) / 100f;

                if (hueDiff <= hueThreshold && satDiff <= satThreshold)
                {
                    // Soft edge based on hue distance
                    var hueFactor = hueDiff / hueThreshold;
                    var satFactor = satDiff / satThreshold;
                    var factor = MathF.Max(hueFactor, satFactor);
                    var newAlpha = (int)(a * factor);
                    pixels[idx] = (uint)((newAlpha << 24) | (r << 16) | (g << 8) | b);
                }
            }
        });

        var resultPixels = result.GetPixels();
        Marshal.Copy(MemoryMarshal.AsBytes<uint>(pixels).ToArray(), 0, resultPixels, pixelCount * 4);

        return result;
    }

    public static float CalculateColorDistance(SKColor a, SKColor b)
    {
        int dr = a.Red - b.Red;
        int dg = a.Green - b.Green;
        int db = a.Blue - b.Blue;

        float distance = MathF.Sqrt(dr * dr + dg * dg + db * db);
        return distance / MaxRgbDistance;
    }
}

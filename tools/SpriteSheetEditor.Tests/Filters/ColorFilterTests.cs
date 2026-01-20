using SkiaSharp;
using SpriteSheetEditor.Filters;
using Xunit;

namespace SpriteSheetEditor.Tests.Filters;

public class ColorFilterTests
{
    [Fact]
    public void ApplyColorToTransparent_ExactMatch_MakesPixelTransparent()
    {
        var source = new SKBitmap(1, 1);
        source.SetPixel(0, 0, SKColors.Magenta);

        var result = ColorFilter.ApplyColorToTransparent(source, SKColors.Magenta, 0f);

        Assert.Equal(0, result.GetPixel(0, 0).Alpha);
    }

    [Fact]
    public void ApplyColorToTransparent_NoMatch_LeavesPixelUnchanged()
    {
        var source = new SKBitmap(1, 1);
        source.SetPixel(0, 0, SKColors.Blue);

        var result = ColorFilter.ApplyColorToTransparent(source, SKColors.Magenta, 0f);

        var pixel = result.GetPixel(0, 0);
        Assert.Equal(SKColors.Blue.Red, pixel.Red);
        Assert.Equal(SKColors.Blue.Green, pixel.Green);
        Assert.Equal(SKColors.Blue.Blue, pixel.Blue);
        Assert.Equal(255, pixel.Alpha);
    }

    [Fact]
    public void ApplyColorToTransparent_WithTolerance_MatchesSimilarColors()
    {
        var source = new SKBitmap(1, 1);
        var similarColor = new SKColor(255, 10, 255); // Slightly different from magenta (255, 0, 255)
        source.SetPixel(0, 0, similarColor);

        var distance = ColorFilter.CalculateColorDistance(similarColor, SKColors.Magenta);
        var result = ColorFilter.ApplyColorToTransparent(source, SKColors.Magenta, distance + 0.01f);

        Assert.Equal(0, result.GetPixel(0, 0).Alpha);
    }

    [Fact]
    public void ApplyColorToTransparent_PreservesExistingTransparency()
    {
        var source = new SKBitmap(1, 1);
        var transparentPixel = new SKColor(255, 0, 255, 0);
        source.SetPixel(0, 0, transparentPixel);

        var result = ColorFilter.ApplyColorToTransparent(source, SKColors.Blue, 0f);

        Assert.Equal(0, result.GetPixel(0, 0).Alpha);
    }

    [Fact]
    public void CalculateColorDistance_IdenticalColors_ReturnsZero()
    {
        var distance = ColorFilter.CalculateColorDistance(SKColors.Red, SKColors.Red);

        Assert.Equal(0f, distance);
    }

    [Fact]
    public void CalculateColorDistance_OppositeColors_ReturnsOne()
    {
        var distance = ColorFilter.CalculateColorDistance(SKColors.Black, SKColors.White);

        Assert.Equal(1f, distance, precision: 5);
    }

    [Fact]
    public void ApplyColorToTransparent_ReturnsNewBitmap_OriginalUnchanged()
    {
        var source = new SKBitmap(1, 1);
        source.SetPixel(0, 0, SKColors.Magenta);

        var result = ColorFilter.ApplyColorToTransparent(source, SKColors.Magenta, 0f);

        Assert.NotSame(source, result);
        Assert.Equal(255, source.GetPixel(0, 0).Alpha);
        Assert.Equal(0, result.GetPixel(0, 0).Alpha);
    }
}

using SpriteSheetEditor.Services;
using Xunit;

namespace SpriteSheetEditor.Tests.Services;

public class GridGeneratorTests
{
    [Fact]
    public void Generate_ShouldCreateCorrectNumberOfSprites()
    {
        var result = GridGenerator.Generate("test", 1024, 1024, columns: 4, rows: 4);

        Assert.Equal(16, result.Sprites.Count);
    }

    [Fact]
    public void Generate_ShouldCalculateCorrectTileSize()
    {
        var result = GridGenerator.Generate("test", 1024, 512, columns: 4, rows: 2);

        Assert.Equal(256, result.Sprites[0].Width);
        Assert.Equal(256, result.Sprites[0].Height);
    }

    [Fact]
    public void Generate_ShouldPositionSpritesCorrectly()
    {
        var result = GridGenerator.Generate("test", 200, 100, columns: 2, rows: 2);

        Assert.Equal(0, result.Sprites[0].X);
        Assert.Equal(0, result.Sprites[0].Y);
        Assert.Equal(100, result.Sprites[1].X);
        Assert.Equal(0, result.Sprites[1].Y);
        Assert.Equal(0, result.Sprites[2].X);
        Assert.Equal(50, result.Sprites[2].Y);
        Assert.Equal(100, result.Sprites[3].X);
        Assert.Equal(50, result.Sprites[3].Y);
    }

    [Fact]
    public void Generate_ShouldNameSpritesSequentially()
    {
        var result = GridGenerator.Generate("avatars", 256, 256, columns: 2, rows: 2);

        Assert.Equal("avatars_sprite_0", result.Sprites[0].Name);
        Assert.Equal("avatars_sprite_1", result.Sprites[1].Name);
        Assert.Equal("avatars_sprite_2", result.Sprites[2].Name);
        Assert.Equal("avatars_sprite_3", result.Sprites[3].Name);
    }

    [Fact]
    public void CalculateTileSize_ShouldReturnIntegerDivision()
    {
        var (width, height) = GridGenerator.CalculateTileSize(1024, 768, 3, 4);

        Assert.Equal(341, width);
        Assert.Equal(192, height);
    }

    [Fact]
    public void GetUncoveredPixels_ShouldReturnRemainderPixels()
    {
        var (uncoveredX, uncoveredY) = GridGenerator.GetUncoveredPixels(1024, 768, 3, 4);

        Assert.Equal(1, uncoveredX);
        Assert.Equal(0, uncoveredY);
    }

    [Fact]
    public void HasUncoveredPixels_ShouldReturnTrueWhenNotEvenlyDivisible()
    {
        Assert.True(GridGenerator.HasUncoveredPixels(1024, 768, 3, 4));
        Assert.False(GridGenerator.HasUncoveredPixels(1024, 1024, 4, 4));
    }
}

using SpriteSheetEditor.Models;
using Xunit;

namespace SpriteSheetEditor.Tests.Models;

public class SpriteDefinitionTests
{
    [Fact]
    public void SpriteDefinition_ShouldStoreAllProperties()
    {
        var sprite = new SpriteDefinition
        {
            Name = "test_sprite",
            X = 10,
            Y = 20,
            Width = 64,
            Height = 128
        };

        Assert.Equal("test_sprite", sprite.Name);
        Assert.Equal(10, sprite.X);
        Assert.Equal(20, sprite.Y);
        Assert.Equal(64, sprite.Width);
        Assert.Equal(128, sprite.Height);
    }

    [Fact]
    public void SpriteDefinition_Bounds_ShouldReturnCorrectRectangle()
    {
        var sprite = new SpriteDefinition
        {
            X = 10,
            Y = 20,
            Width = 64,
            Height = 128
        };

        var bounds = sprite.Bounds;

        Assert.Equal(10, bounds.X);
        Assert.Equal(20, bounds.Y);
        Assert.Equal(64, bounds.Width);
        Assert.Equal(128, bounds.Height);
    }

    [Fact]
    public void SpriteDefinition_ContainsPoint_ShouldReturnTrueForPointInside()
    {
        var sprite = new SpriteDefinition { X = 10, Y = 20, Width = 64, Height = 128 };

        Assert.True(sprite.ContainsPoint(50, 80));
        Assert.True(sprite.ContainsPoint(10, 20));  // top-left corner
        Assert.True(sprite.ContainsPoint(73, 147)); // bottom-right - 1
    }

    [Fact]
    public void SpriteDefinition_ContainsPoint_ShouldReturnFalseForPointOutside()
    {
        var sprite = new SpriteDefinition { X = 10, Y = 20, Width = 64, Height = 128 };

        Assert.False(sprite.ContainsPoint(5, 80));   // left of bounds
        Assert.False(sprite.ContainsPoint(80, 80));  // right of bounds
        Assert.False(sprite.ContainsPoint(50, 10));  // above bounds
        Assert.False(sprite.ContainsPoint(50, 200)); // below bounds
    }
}

using FluentAssertions;
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

        sprite.Name.Should().Be("test_sprite");
        sprite.X.Should().Be(10);
        sprite.Y.Should().Be(20);
        sprite.Width.Should().Be(64);
        sprite.Height.Should().Be(128);
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

        bounds.X.Should().Be(10);
        bounds.Y.Should().Be(20);
        bounds.Width.Should().Be(64);
        bounds.Height.Should().Be(128);
    }

    [Fact]
    public void SpriteDefinition_ContainsPoint_ShouldReturnTrueForPointInside()
    {
        var sprite = new SpriteDefinition { X = 10, Y = 20, Width = 64, Height = 128 };

        sprite.ContainsPoint(50, 80).Should().BeTrue();
        sprite.ContainsPoint(10, 20).Should().BeTrue();  // top-left corner
        sprite.ContainsPoint(73, 147).Should().BeTrue(); // bottom-right - 1
    }

    [Fact]
    public void SpriteDefinition_ContainsPoint_ShouldReturnFalseForPointOutside()
    {
        var sprite = new SpriteDefinition { X = 10, Y = 20, Width = 64, Height = 128 };

        sprite.ContainsPoint(5, 80).Should().BeFalse();   // left of bounds
        sprite.ContainsPoint(80, 80).Should().BeFalse();  // right of bounds
        sprite.ContainsPoint(50, 10).Should().BeFalse();  // above bounds
        sprite.ContainsPoint(50, 200).Should().BeFalse(); // below bounds
    }
}

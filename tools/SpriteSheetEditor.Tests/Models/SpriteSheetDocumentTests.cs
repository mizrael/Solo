using FluentAssertions;
using SpriteSheetEditor.Models;
using Xunit;

namespace SpriteSheetEditor.Tests.Models;

public class SpriteSheetDocumentTests
{
    [Fact]
    public void SpriteSheetDocument_ShouldInitializeWithEmptySpritesList()
    {
        var doc = new SpriteSheetDocument();

        doc.Sprites.Should().NotBeNull();
        doc.Sprites.Should().BeEmpty();
    }

    [Fact]
    public void SpriteSheetDocument_GenerateSpriteName_ShouldUseSheetNameAndIndex()
    {
        var doc = new SpriteSheetDocument { SpriteSheetName = "avatars" };

        doc.GenerateSpriteName(0).Should().Be("avatars_sprite_0");
        doc.GenerateSpriteName(5).Should().Be("avatars_sprite_5");
    }

    [Fact]
    public void SpriteSheetDocument_GetNextSpriteIndex_ShouldReturnZeroForEmptyList()
    {
        var doc = new SpriteSheetDocument { SpriteSheetName = "test" };

        doc.GetNextSpriteIndex().Should().Be(0);
    }

    [Fact]
    public void SpriteSheetDocument_GetNextSpriteIndex_ShouldReturnNextAfterHighest()
    {
        var doc = new SpriteSheetDocument
        {
            SpriteSheetName = "test",
            Sprites =
            [
                new SpriteDefinition { Name = "test_sprite_0" },
                new SpriteDefinition { Name = "test_sprite_5" },
                new SpriteDefinition { Name = "custom_name" }
            ]
        };

        doc.GetNextSpriteIndex().Should().Be(6);
    }
}

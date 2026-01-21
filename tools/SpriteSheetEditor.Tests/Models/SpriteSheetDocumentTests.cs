using SpriteSheetEditor.Models;
using Xunit;

namespace SpriteSheetEditor.Tests.Models;

public class SpriteSheetDocumentTests
{
    [Fact]
    public void SpriteSheetDocument_ShouldInitializeWithEmptySpritesList()
    {
        var doc = new SpriteSheetDocument();

        Assert.NotNull(doc.Sprites);
        Assert.Empty(doc.Sprites);
    }

    [Fact]
    public void SpriteSheetDocument_GenerateSpriteName_ShouldUseSheetNameAndIndex()
    {
        var doc = new SpriteSheetDocument { SpriteSheetName = "avatars" };

        Assert.Equal("avatars_sprite_0", doc.GenerateSpriteName(0));
        Assert.Equal("avatars_sprite_5", doc.GenerateSpriteName(5));
    }

    [Fact]
    public void SpriteSheetDocument_GetNextSpriteIndex_ShouldReturnZeroForEmptyList()
    {
        var doc = new SpriteSheetDocument { SpriteSheetName = "test" };

        Assert.Equal(0, doc.GetNextSpriteIndex());
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

        Assert.Equal(6, doc.GetNextSpriteIndex());
    }
}

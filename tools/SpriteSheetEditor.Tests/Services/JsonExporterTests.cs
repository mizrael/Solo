using SpriteSheetEditor.Models;
using SpriteSheetEditor.Services;
using Xunit;

namespace SpriteSheetEditor.Tests.Services;

public class JsonExporterTests
{
    [Fact]
    public void Serialize_ShouldProduceCorrectJsonFormat()
    {
        var doc = new SpriteSheetDocument
        {
            SpriteSheetName = "avatars",
            Sprites =
            [
                new SpriteDefinition { Name = "warrior", X = 0, Y = 0, Width = 256, Height = 256 },
                new SpriteDefinition { Name = "mage", X = 256, Y = 0, Width = 256, Height = 256 }
            ]
        };

        var json = JsonExporter.Serialize(doc);

        Assert.Contains("\"spriteSheetName\": \"avatars\"", json);
        Assert.Contains("\"name\": \"warrior\"", json);
        Assert.Contains("\"x\": 0", json);
        Assert.Contains("\"width\": 256", json);
        Assert.DoesNotContain("LoadedImage", json);
        Assert.DoesNotContain("ImageFilePath", json);
        Assert.DoesNotContain("Bounds", json);
    }

    [Fact]
    public void Deserialize_ShouldLoadCorrectValues()
    {
        var json = """
        {
          "spriteSheetName": "test",
          "sprites": [
            { "name": "sprite1", "x": 10, "y": 20, "width": 64, "height": 128 }
          ]
        }
        """;

        var doc = JsonExporter.Deserialize(json);

        Assert.Equal("test", doc.SpriteSheetName);
        Assert.Single(doc.Sprites);
        Assert.Equal("sprite1", doc.Sprites[0].Name);
        Assert.Equal(10, doc.Sprites[0].X);
        Assert.Equal(20, doc.Sprites[0].Y);
        Assert.Equal(64, doc.Sprites[0].Width);
        Assert.Equal(128, doc.Sprites[0].Height);
    }

    [Fact]
    public void RoundTrip_ShouldPreserveAllData()
    {
        var original = new SpriteSheetDocument
        {
            SpriteSheetName = "roundtrip_test",
            Sprites =
            [
                new SpriteDefinition { Name = "s1", X = 1, Y = 2, Width = 3, Height = 4 },
                new SpriteDefinition { Name = "s2", X = 5, Y = 6, Width = 7, Height = 8 }
            ]
        };

        var json = JsonExporter.Serialize(original);
        var restored = JsonExporter.Deserialize(json);

        Assert.Equal(original.SpriteSheetName, restored.SpriteSheetName);
        Assert.Equal(2, restored.Sprites.Count);
        Assert.Equal("s1", restored.Sprites[0].Name);
        Assert.Equal(5, restored.Sprites[1].X);
    }
}

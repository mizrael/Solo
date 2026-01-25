using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using SkiaSharp;

namespace SpriteSheetEditor.Models;

public partial class SpriteSheetDocument
{
    public string SpriteSheetName { get; set; } = string.Empty;
    public ObservableCollection<SpriteDefinition> Sprites { get; set; } = [];
    public ObservableCollection<AnimationDefinition> Animations { get; set; } = [];

    [JsonIgnore]
    public SKBitmap? LoadedImage { get; set; }

    [JsonIgnore]
    public string? ImageFilePath { get; set; }

    public string GenerateSpriteName(int index)
    {
        return $"{SpriteSheetName}_sprite_{index}";
    }

    public int GetNextSpriteIndex()
    {
        var maxIndex = -1;
        var pattern = SpriteNamePattern();

        foreach (var sprite in Sprites)
        {
            var match = pattern.Match(sprite.Name);
            if (match.Success && int.TryParse(match.Groups[1].Value, out var index))
            {
                maxIndex = Math.Max(maxIndex, index);
            }
        }

        return maxIndex + 1;
    }

    [GeneratedRegex(@"_sprite_(\d+)$")]
    private static partial Regex SpriteNamePattern();
}

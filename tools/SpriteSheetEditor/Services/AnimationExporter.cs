using System.Text.Json;
using System.Text.Json.Serialization;
using SpriteSheetEditor.Models;

namespace SpriteSheetEditor.Services;

public static class AnimationExporter
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public static string Serialize(AnimationDefinition animation, string spriteSheetName)
    {
        var exportDto = new AnimationExportDto
        {
            AnimationName = animation.Name,
            SpriteSheetName = spriteSheetName,
            Fps = animation.Fps,
            Frames = animation.Frames.Select(f => new FrameExportDto
            {
                X = f.Sprite.X,
                Y = f.Sprite.Y,
                Width = f.Sprite.Width,
                Height = f.Sprite.Height
            }).ToList()
        };

        return JsonSerializer.Serialize(exportDto, SerializerOptions);
    }

    public static async Task ExportAsync(AnimationDefinition animation, string spriteSheetName, string filePath)
    {
        var json = Serialize(animation, spriteSheetName);
        await File.WriteAllTextAsync(filePath, json);
    }
}

internal class AnimationExportDto
{
    public string AnimationName { get; set; } = string.Empty;
    public string SpriteSheetName { get; set; } = string.Empty;
    public int Fps { get; set; }
    public List<FrameExportDto> Frames { get; set; } = [];
}

internal class FrameExportDto
{
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
}

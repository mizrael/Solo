using System.Text.Json;
using System.Text.Json.Serialization;
using SpriteSheetEditor.Models;

namespace SpriteSheetEditor.Services;

public static class JsonExporter
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public static string Serialize(SpriteSheetDocument document)
    {
        return JsonSerializer.Serialize(document, SerializerOptions);
    }

    public static SpriteSheetDocument Deserialize(string json)
    {
        return JsonSerializer.Deserialize<SpriteSheetDocument>(json, SerializerOptions)
               ?? new SpriteSheetDocument();
    }

    public static async Task SaveAsync(SpriteSheetDocument document, string filePath)
    {
        var json = Serialize(document);
        await File.WriteAllTextAsync(filePath, json);
    }

    public static async Task<SpriteSheetDocument> LoadAsync(string filePath)
    {
        var json = await File.ReadAllTextAsync(filePath);
        return Deserialize(json);
    }
}

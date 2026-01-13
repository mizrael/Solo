using Microsoft.Xna.Framework;
using Solocaster.Entities;
using Solocaster.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Solocaster.Persistence;

public class LevelLoader
{
    private readonly static JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private static TemplateLoader? _templateLoader;

    public static Map LoadFromJson(
        string path,
        Game game,
        EntityManager entityManager)
    {
        // Load templates if not already loaded
        if (_templateLoader == null)
        {
            _templateLoader = new TemplateLoader();
            var templatesPath = Path.Combine(Path.GetDirectoryName(path) ?? "", "..", "templates");
            _templateLoader.LoadAllTemplatesFromFolder(Path.GetFullPath(templatesPath));
        }

        var json = System.IO.File.ReadAllText(path);
        var levelData = JsonSerializer.Deserialize<LevelData>(json, _jsonOptions);
        if (levelData == null)
            throw new Exception("Failed to deserialize map data.");

        var map = new Map(levelData.Map.Cells);

        foreach (var entityData in levelData.Entities)
        {
            var template = _templateLoader.Get(entityData.Template);

            var properties = new Dictionary<string, object>();
            foreach (var kvp in template.Properties)
            {
                properties[kvp.Key] = ConvertJsonElement(kvp.Value);
            }

            if (entityData.Properties is not null)
            {
                foreach (var kvp in entityData.Properties)
                {
                    properties[kvp.Key] = ConvertJsonElement(kvp.Value);
                }
            }

            var definition = new EntityDefinition(
                Type: template.ItemType,
                TileX: entityData.TileX,
                TileY: entityData.TileY,
                Properties: properties
            );

            EntityFactory.CreateEntity(definition, game, entityManager);
        }

        return map;
    }

    private static object ConvertJsonElement(object value)
    {
        if (value is not JsonElement jsonElement)
            return value;

        return jsonElement.ValueKind switch
        {
            JsonValueKind.String => jsonElement.GetString() ?? string.Empty,
            JsonValueKind.Number => jsonElement.TryGetInt32(out var intVal) ? intVal : jsonElement.GetSingle(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null!,
            _ => value
        };
    }

    private class LevelData
    {
        public required MapData Map { get; set; }
        public required List<EntityData> Entities { get; set; }
    }

    private class MapData
    {
        public required int[][] Cells { get; set; }
    }

    private class EntityData
    {
        [JsonPropertyName("template")]
        public required string Template { get; set; }

        public int TileX { get; set; }
        public int TileY { get; set; }
        public Dictionary<string, object> Properties { get; set; } = new();
    }
}
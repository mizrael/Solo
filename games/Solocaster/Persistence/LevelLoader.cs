using Microsoft.Xna.Framework;
using Solocaster.Entities;
using Solocaster.Services;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Solocaster.Persistence;

public class LevelLoader
{
    private readonly static JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public static Map LoadFromJson(
        string path,
        Game game,
        EntityManager entityManager)
    {
        var json = System.IO.File.ReadAllText(path);
        var levelData = JsonSerializer.Deserialize<LevelData>(json, _jsonOptions);
        if (levelData == null)
            throw new Exception("Failed to deserialize map data.");

        var map = new Map(levelData.Map.Cells);

        foreach (var entityData in levelData.Entities)
        {
            var definition = new EntityDefinition(
                Name: entityData.Name,
                Type: entityData.Type,
                TileX: entityData.TileX,
                TileY: entityData.TileY,
                Properties: entityData.Properties
            );

            EntityFactory.CreateEntity(definition, game, entityManager);
        }

        return map;
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
        [JsonPropertyName("name")]
        public required string Name { get; set; }

        [JsonPropertyName("type")]
        public required string Type { get; set; }
        public int TileX { get; set; }
        public int TileY { get; set; }
        public Dictionary<string, string> Properties { get; set; } = new();
    }
}
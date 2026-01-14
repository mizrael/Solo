using Microsoft.Xna.Framework;
using Solocaster.DungeonGenerator;
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

    public static Entities.Map LoadFromJson(
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

        // Generate or load map cells
        int[][] cells;
        if (levelData.Map.Cells != null && levelData.Map.Cells.Length > 0)
        {
            // Use provided cells
            cells = levelData.Map.Cells;
        }
        else
        {
            // Generate random dungeon
            var generator = new DungeonGenerator.DungeonGenerator(
                width: 25,
                height: 25,
                changeDirectionModifier: 30,
                sparsenessModifier: 70,
                deadEndRemovalModifier: 50,
                roomGenerator: new RoomGenerator(10, 1, 5, 1, 5)
            );

            var dungeon = generator.Generate();
            var tiles = dungeon.ExpandToTiles(1);
            cells = ConvertTilesToCells(tiles);
        }

        var map = new Entities.Map(cells);

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

    private static int[][] ConvertTilesToCells(TileType[,] tiles)
    {
        int height = tiles.GetLength(1);
        int width = tiles.GetLength(0);

        int[][] cells = new int[height][];
        for (int row = 0; row < height; row++)
        {
            cells[row] = new int[width];
            for (int col = 0; col < width; col++)
            {
                cells[row][col] = MapTileTypeToCell(tiles[col, row]);
            }
        }

        return cells;
    }

    private static int MapTileTypeToCell(TileType tileType)
    {
        return tileType switch
        {
            TileType.Empty => TileTypes.Floor,
            TileType.Void => TileTypes.Floor,
            TileType.Door => TileTypes.Door,
            TileType.Wall => 1,
            TileType.WallSE => 1,
            TileType.WallSO => 1,
            TileType.WallNE => 1,
            TileType.WallNO => 1,
            TileType.WallNS => 1,
            TileType.WallEO => 1,
            TileType.WallESO => 1,
            TileType.WallNEO => 1,
            TileType.WallNES => 1,
            TileType.WallNSO => 1,
            TileType.WallNESO => 1,
            _ => TileTypes.Floor
        };
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
        public int[][]? Cells { get; set; }
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
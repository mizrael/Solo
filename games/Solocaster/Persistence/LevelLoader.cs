using Microsoft.Xna.Framework;
using Solo.Assets.Loaders;
using Solocaster.DungeonGenerator;
using Solocaster.Entities;
using Solocaster.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Random = System.Random;

namespace Solocaster.Persistence;

public enum MapType
{
    Static,
    Random
}

public class LevelLoader
{
    private readonly static JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly static EntityTemplateLoader _templateLoader;

    static LevelLoader()
    {
        _templateLoader = new EntityTemplateLoader();
        var templatesPath = Path.Combine("./data", "templates");
        _templateLoader.LoadAllTemplatesFromFolder(Path.GetFullPath(templatesPath));
    }

    public static Entities.Level LoadFromJson(
        string path,
        Game game,
        EntityManager entityManager)
    {
        var json = System.IO.File.ReadAllText(path);
        var levelData = JsonSerializer.Deserialize<LevelData>(json, _jsonOptions);
        if (levelData == null)
            throw new Exception("Failed to deserialize map data.");

        var spritesheets = LoadSpritesheets(path, game, levelData);

        // For random maps, resolve wall sprites first
        WallSpritesResult? wallSpritesResult = null;
        if (levelData.Map.Type == MapType.Random)
        {
            wallSpritesResult = ResolveWallSprites(levelData.Map.WallSprites, spritesheets);
        }

        var wallSprites = BuildWallSprites(levelData, spritesheets, wallSpritesResult);
        var doorSprites = BuildDoorSprites(levelData, spritesheets);

        var map = LoadMap(levelData, spritesheets, wallSpritesResult, doorSprites.Length);

        LoadEntities(game, entityManager, levelData);

        // Place random decorations for random maps
        if (levelData.Map.Type == MapType.Random && levelData.Map.Decorations != null)
        {
            PlaceDecorations(game, entityManager, levelData.Map.Decorations, map);
        }

        return new()
        {
            Map = map,
            SpriteSheets = spritesheets,
            WallSprites = wallSprites,
            DoorSprites = doorSprites
        };
    }

    private static Solo.Assets.SpriteSheet[] LoadSpritesheets(string path, Game game, LevelData levelData)
    {
        if (levelData.Spritesheets is null || !levelData.Spritesheets.Any())
            throw new InvalidOperationException($"no spritesheets defined in level {path}");
        var spritesheets = levelData.Spritesheets.Select(s => SpriteSheetLoader.Get(s, game)).ToArray();
        return spritesheets;
    }

    private static Solo.Assets.Sprite[] BuildWallSprites(
        LevelData levelData,
        Solo.Assets.SpriteSheet[] spritesheets,
        WallSpritesResult? wallSpritesResult)
    {
        if (levelData.Map.Type == MapType.Static)
        {
            var sprites = new List<Solo.Assets.Sprite>();

            foreach (var spritesheet in spritesheets)
            {
                foreach (var sprite in spritesheet.Sprites)
                {
                    sprites.Add(sprite);
                }
            }

            return sprites.ToArray();
        }

        // Random maps: use sprites from wallSprites configuration
        if (wallSpritesResult != null)
        {
            return wallSpritesResult.Sprites;
        }

        // Fallback: use all sprites from first spritesheet
        return spritesheets[0].Sprites.ToArray();
    }

    private static Solo.Assets.Sprite[] BuildDoorSprites(
        LevelData levelData,
        Solo.Assets.SpriteSheet[] spritesheets)
    {
        if (levelData.Map.DoorSprites == null || levelData.Map.DoorSprites.Length == 0)
            return Array.Empty<Solo.Assets.Sprite>();

        var doorSprites = new List<Solo.Assets.Sprite>();

        foreach (var doorSpriteName in levelData.Map.DoorSprites)
        {
            var doorSprite = FindSpriteInSheets(doorSpriteName, spritesheets);
            doorSprites.Add(doorSprite);
        }

        return doorSprites.ToArray();
    }

    private static Entities.Map LoadMap(
        LevelData levelData,
        Solo.Assets.SpriteSheet[] spritesheets,
        WallSpritesResult? wallSpritesResult,
        int doorSpriteCount)
    {
        int[][] cells;

        switch (levelData.Map.Type)
        {
            case MapType.Random:
                var roomGenerator = new RoomGenerator(5, 2, 3, 2, 3);
                var generator = new DungeonGenerator.DungeonGenerator(
                    width: 10,
                    height: 10,
                    changeDirectionModifier: 70,
                    sparsenessModifier: 20,
                    deadEndRemovalModifier: 80,
                    roomGenerator: roomGenerator
                );

                var dungeon = generator.Generate();
                var tiles = dungeon.ExpandToTiles(2);
                cells = ConvertTilesToCells(tiles, wallSpritesResult?.Weights);
                break;

            case MapType.Static:
                if (levelData.Map.Cells == null || levelData.Map.Cells.Length == 0)
                    throw new InvalidOperationException("Map type is 'static' but no Cells data provided");

                cells = levelData.Map.Cells;
                break;

            default:
                throw new InvalidOperationException($"Invalid map type '{levelData.Map.Type}'");
        }

        EnsurePerimeterClosed(cells);

        var map = new Entities.Map(cells, doorSpriteCount);
        return map;
    }

    private static void LoadEntities(Game game, EntityManager entityManager, LevelData levelData)
    {
        if (levelData.Entities is null)
            return;

        foreach (var entityData in levelData.Entities)
        {
            var template = _templateLoader.Get(entityData.Template);

            var properties = new Dictionary<string, object>();
            foreach (var kvp in template.Properties)
            {
                properties[kvp.Key] = JsonUtils.ConvertJsonElement(kvp.Value);
            }

            if (entityData.Properties is not null)
            {
                foreach (var kvp in entityData.Properties)
                {
                    properties[kvp.Key] = JsonUtils.ConvertJsonElement(kvp.Value);
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
    }

    private static void PlaceDecorations(
        Game game,
        EntityManager entityManager,
        List<DecorationData> decorations,
        Entities.Map map)
    {
        // Collect floor tiles and wall-adjacent floor tiles
        var floorTiles = new List<(int col, int row)>();
        var wallAdjacentTiles = new List<(int col, int row)>();

        for (int row = 0; row < map.Rows; row++)
        {
            for (int col = 0; col < map.Cols; col++)
            {
                var cell = map.Cells[row][col];

                // Skip non-floor tiles
                if (cell != TileTypes.Floor)
                    continue;

                floorTiles.Add((col, row));

                // Check if adjacent to a wall
                if (IsAdjacentToWall(map, col, row))
                {
                    wallAdjacentTiles.Add((col, row));
                }
            }
        }

        // Track occupied tiles to avoid placing multiple decorations on same tile
        var occupiedTiles = new HashSet<(int, int)>();

        foreach (var decoration in decorations)
        {
            if (decoration.Items == null || decoration.Items.Count == 0)
                continue;

            var eligibleTiles = decoration.Placement == DecorationPlacement.Wall
                ? wallAdjacentTiles
                : floorTiles;

            foreach (var (col, row) in eligibleTiles)
            {
                // Skip if already occupied
                if (occupiedTiles.Contains((col, row)))
                    continue;

                // Use density as probability
                if (Random.Shared.NextDouble() > decoration.Density)
                    continue;

                // Pick a random item using weights
                var template = decoration.Items.WeightedRandom();

                var templateData = _templateLoader.Get(template);
                var properties = new Dictionary<string, object>();
                foreach (var kvp in templateData.Properties)
                {
                    properties[kvp.Key] = JsonUtils.ConvertJsonElement(kvp.Value);
                }

                var definition = new EntityDefinition(
                    Type: templateData.ItemType,
                    TileX: col,
                    TileY: row,
                    Properties: properties
                );

                EntityFactory.CreateEntity(definition, game, entityManager);
                occupiedTiles.Add((col, row));
            }
        }
    }

    private static bool IsAdjacentToWall(Entities.Map map, int col, int row)
    {
        // Check all 4 cardinal directions for walls
        int[][] directions = [[0, -1], [0, 1], [-1, 0], [1, 0]];

        foreach (var dir in directions)
        {
            int newCol = col + dir[0];
            int newRow = row + dir[1];

            if (newCol < 0 || newCol >= map.Cols || newRow < 0 || newRow >= map.Rows)
                continue;

            var cell = map.Cells[newRow][newCol];
            // Wall cells are positive integers (sprite indices) that are not doors
            if (cell > 0 && cell != TileTypes.DoorVertical && cell != TileTypes.DoorHorizontal)
                return true;
        }

        return false;
    }

    private static int[][] ConvertTilesToCells(TileType[,] tiles, Dictionary<int, int>? weightedWallCellIds = null)
    {
        int height = tiles.GetLength(1);
        int width = tiles.GetLength(0);

        int[][] cells = new int[height][];
        for (int row = 0; row < height; row++)
        {
            cells[row] = new int[width];
            for (int col = 0; col < width; col++)
            {
                var tileType = tiles[col, row];
                int cellId = MapTileTypeToCell(tileType);

                // For walls with weighted sprites, apply neighbor-based consistency
                if (tileType.IsWall() && weightedWallCellIds != null && weightedWallCellIds.Count > 0)
                {
                    cellId = PickWallCellId(cells, row, col, weightedWallCellIds);
                }

                cells[row][col] = cellId;
            }
        }

        return cells;
    }

    private static int PickWallCellId(int[][] cells, int row, int col, Dictionary<int, int> weightedCellIds)
    {
        // Collect wall neighbors that have already been assigned
        var neighborWallIds = new List<int>();

        // Check left neighbor (exclude doors)
        int leftCell = col > 0 ? cells[row][col - 1] : 0;
        if (leftCell > 0 && leftCell != TileTypes.DoorVertical && leftCell != TileTypes.DoorHorizontal)
            neighborWallIds.Add(leftCell);

        // Check top neighbor (exclude doors)
        int topCell = row > 0 ? cells[row - 1][col] : 0;
        if (topCell > 0 && topCell != TileTypes.DoorVertical && topCell != TileTypes.DoorHorizontal)
            neighborWallIds.Add(topCell);

        // 80% chance: copy from a neighbor (if any exist)
        // 20% chance: pick new weighted random
        if (neighborWallIds.Count > 0 && Random.Shared.Next(100) < 80)
        {
            return neighborWallIds[Random.Shared.Next(neighborWallIds.Count)];
        }

        // Pick new weighted random cell ID
        return weightedCellIds.WeightedRandom();
    }

    private static int MapTileTypeToCell(TileType tileType)
    {
        return tileType switch
        {
            TileType.Empty => TileTypes.Floor,
            TileType.Void => TileTypes.Floor,
            TileType.DoorVertical => TileTypes.DoorVertical,
            TileType.DoorHorizontal => TileTypes.DoorHorizontal,
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

    private static void EnsurePerimeterClosed(int[][] cells)
    {
        if (cells == null || cells.Length == 0)
            return;

        int height = cells.Length;
        int width = cells[0].Length;

        // Top and bottom rows
        for (int col = 0; col < width; col++)
        {
            cells[0][col] = 1; // Top row
            cells[height - 1][col] = 1; // Bottom row
        }

        // Left and right columns
        for (int row = 0; row < height; row++)
        {
            cells[row][0] = 1; // Left column
            cells[row][width - 1] = 1; // Right column
        }
    }

    private static WallSpritesResult? ResolveWallSprites(
        JsonElement? wallSpritesJson,
        Solo.Assets.SpriteSheet[] spritesheets)
    {
        if (wallSpritesJson == null || wallSpritesJson.Value.ValueKind == JsonValueKind.Null)
            return null;

        var sprites = new List<Solo.Assets.Sprite>();
        var weights = new Dictionary<int, int>();

        // Try to detect format by checking for "base" or "accent" properties
        bool isHybridFormat = wallSpritesJson.Value.ValueKind == JsonValueKind.Object &&
                               (wallSpritesJson.Value.TryGetProperty("base", out _) ||
                                wallSpritesJson.Value.TryGetProperty("accent", out _));

        if (isHybridFormat)
        {
            // New hybrid format
            var hybrid = wallSpritesJson.Value.Deserialize<WallSpritesHybrid>(_jsonOptions);
            if (hybrid == null)
                throw new InvalidOperationException("Failed to parse hybrid wallSprites format");

            // Process base sprites
            if (hybrid.Base != null)
            {
                foreach (var kvp in hybrid.Base)
                {
                    string spriteName = kvp.Key;
                    int weight = kvp.Value;

                    var sprite = FindSpriteInSheets(spriteName, spritesheets);
                    sprites.Add(sprite);
                    weights[sprites.Count - 1] = weight;
                }
            }

            // Process accent sprites (auto-weight as 10% of total base weight)
            if (hybrid.Accent != null && hybrid.Accent.Length > 0)
            {
                int totalBaseWeight = hybrid.Base?.Values.Sum() ?? 100;
                int accentTotalWeight = (int)(totalBaseWeight * 0.10);
                int accentWeightEach = Math.Max(1, accentTotalWeight / hybrid.Accent.Length);

                foreach (var spriteName in hybrid.Accent)
                {
                    var sprite = FindSpriteInSheets(spriteName, spritesheets);
                    sprites.Add(sprite);
                    weights[sprites.Count - 1] = accentWeightEach;
                }
            }
        }
        else
        {
            // Old format: "spritesheet:sprite_name" → weight
            var oldFormat = wallSpritesJson.Value.Deserialize<Dictionary<string, int>>(_jsonOptions);
            if (oldFormat == null || oldFormat.Count == 0)
                return null;

            foreach (var kvp in oldFormat)
            {
                string spriteRef = kvp.Key; // e.g., "wolfenstein:brick_wall"
                int weight = kvp.Value;

                // Parse spritesheet:sprite format
                var parts = spriteRef.Split(':', 2);
                if (parts.Length != 2)
                    throw new InvalidOperationException($"Invalid sprite reference '{spriteRef}'. Expected format: 'spritesheet:sprite'");

                string spritesheetName = parts[0];
                string spriteName = parts[1];

                var spritesheet = spritesheets.FirstOrDefault(s => s.Name == spritesheetName);
                if (spritesheet == null)
                    throw new InvalidOperationException($"Spritesheet '{spritesheetName}' not found in loaded spritesheets");

                var sprite = spritesheet.Get(spriteName);
                sprites.Add(sprite);
                weights[sprites.Count - 1] = weight;
            }
        }

        if (sprites.Count == 0)
            return null;

        return new WallSpritesResult
        {
            Sprites = sprites.ToArray(),
            Weights = weights
        };
    }

    private static Solo.Assets.Sprite FindSpriteInSheets(string spriteName, Solo.Assets.SpriteSheet[] spritesheets)
    {
        foreach (var sheet in spritesheets)
        {
            try
            {
                return sheet.Get(spriteName);
            }
            catch
            {
                // Not in this sheet, try next
            }
        }

        throw new InvalidOperationException($"Sprite '{spriteName}' not found in any loaded spritesheet");
    }

    private class WallSpritesResult
    {
        public required Solo.Assets.Sprite[] Sprites { get; init; }
        public required Dictionary<int, int> Weights { get; init; }
    }

    private class WallSpritesHybrid
    {
        public Dictionary<string, int>? Base { get; init; }
        public string[]? Accent { get; init; }
    }

    private class LevelData
    {
        public required string[] Spritesheets { get; init; }
        public required MapData Map { get; init; }

        public List<EntityData>? Entities { get; init; } = new();
    }

    private class MapData
    {
        public required MapType Type { get; init; } = MapType.Static;
        public int[][]? Cells { get; init; }
        public JsonElement? WallSprites { get; init; }
        public string[]? DoorSprites { get; init; }
        public List<DecorationData>? Decorations { get; init; }
    }

    private enum DecorationPlacement
    {
        Floor,
        Wall
    }

    private class DecorationData
    {
        public float Density { get; init; }
        public DecorationPlacement Placement { get; init; }
        public Dictionary<string, int>? Items { get; init; }
    }

    private class EntityData
    {
        [JsonPropertyName("template")]
        public required string Template { get; init; }

        public int TileX { get; init; }
        public int TileY { get; init; }
        public Dictionary<string, object> Properties { get; init; } = new();
    }
}

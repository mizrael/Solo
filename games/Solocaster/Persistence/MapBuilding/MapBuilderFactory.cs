using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Solo.Assets;

namespace Solocaster.Persistence.MapBuilding;

public class MapBuilderFactory
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public IMapBuilder Create(MapData mapData, SpriteSheet[] spritesheets, Sprite[] doorSprites)
    {
        return mapData.Type switch
        {
            MapType.Static => CreateStaticBuilder(mapData, doorSprites.Length),
            MapType.Random => CreateRandomBuilder(mapData, spritesheets, doorSprites.Length),
            _ => throw new InvalidOperationException($"Unknown map type: {mapData.Type}")
        };
    }

    private static StaticMapBuilder CreateStaticBuilder(MapData mapData, int doorSpriteCount)
    {
        if (mapData.Cells == null || mapData.Cells.Length == 0)
            throw new InvalidOperationException("Static map requires cells data");

        return new StaticMapBuilder(mapData.Cells, doorSpriteCount);
    }

    private RandomMapBuilder CreateRandomBuilder(MapData mapData, SpriteSheet[] spritesheets, int doorSpriteCount)
    {
        var wallSpritesResult = ResolveWallSprites(mapData.WallSprites, spritesheets);

        var config = new RandomMapConfig
        {
            WallSprites = wallSpritesResult?.Sprites ?? spritesheets[0].Sprites.ToArray(),
            WallSpriteWeights = wallSpritesResult?.Weights,
            DoorSpriteCount = doorSpriteCount,
            Decorations = ConvertDecorations(mapData.Decorations),
            PickupableItems = ConvertPickupableItems(mapData.PickupableItems)
        };

        return new RandomMapBuilder(config);
    }

    private static List<DecorationConfig>? ConvertDecorations(List<DecorationData>? decorations)
    {
        if (decorations == null)
            return null;

        return decorations.Select(d => new DecorationConfig
        {
            Density = d.Density,
            Placement = d.Placement,
            Items = d.Items
        }).ToList();
    }

    private static List<PickupableItemConfig>? ConvertPickupableItems(List<PickupableItemData>? items)
    {
        if (items == null)
            return null;

        return items.Select(i => new PickupableItemConfig
        {
            Density = i.Density,
            Items = i.Items,
            MinQuantity = i.MinQuantity,
            MaxQuantity = i.MaxQuantity,
            PickupRadius = i.PickupRadius
        }).ToList();
    }

    private WallSpritesResult? ResolveWallSprites(JsonElement? wallSpritesJson, SpriteSheet[] spritesheets)
    {
        if (wallSpritesJson == null || wallSpritesJson.Value.ValueKind == JsonValueKind.Null)
            return null;

        var sprites = new List<Sprite>();
        var weights = new Dictionary<int, int>();

        bool isHybridFormat = wallSpritesJson.Value.ValueKind == JsonValueKind.Object &&
            (wallSpritesJson.Value.TryGetProperty("base", out _) ||
             wallSpritesJson.Value.TryGetProperty("accent", out _));

        if (isHybridFormat)
        {
            ParseHybridFormat(wallSpritesJson.Value, spritesheets, sprites, weights);
        }
        else
        {
            ParseLegacyFormat(wallSpritesJson.Value, spritesheets, sprites, weights);
        }

        if (sprites.Count == 0)
            return null;

        return new WallSpritesResult
        {
            Sprites = sprites.ToArray(),
            Weights = weights
        };
    }

    private void ParseHybridFormat(
        JsonElement json,
        SpriteSheet[] spritesheets,
        List<Sprite> sprites,
        Dictionary<int, int> weights)
    {
        var hybrid = json.Deserialize<WallSpritesHybrid>(JsonOptions);
        if (hybrid == null)
            throw new InvalidOperationException("Failed to parse hybrid wallSprites format");

        if (hybrid.Base != null)
        {
            foreach (var kvp in hybrid.Base)
            {
                var sprite = FindSpriteInSheets(kvp.Key, spritesheets);
                sprites.Add(sprite);
                weights[sprites.Count - 1] = kvp.Value;
            }
        }

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

    private void ParseLegacyFormat(
        JsonElement json,
        SpriteSheet[] spritesheets,
        List<Sprite> sprites,
        Dictionary<int, int> weights)
    {
        var oldFormat = json.Deserialize<Dictionary<string, int>>(JsonOptions);
        if (oldFormat == null || oldFormat.Count == 0)
            return;

        foreach (var kvp in oldFormat)
        {
            var parts = kvp.Key.Split(':', 2);
            if (parts.Length != 2)
                throw new InvalidOperationException($"Invalid sprite reference '{kvp.Key}'");

            var spritesheet = spritesheets.FirstOrDefault(s => s.Name == parts[0])
                ?? throw new InvalidOperationException($"Spritesheet '{parts[0]}' not found");

            var sprite = spritesheet.Get(parts[1]);
            sprites.Add(sprite);
            weights[sprites.Count - 1] = kvp.Value;
        }
    }

    private static Sprite FindSpriteInSheets(string spriteName, SpriteSheet[] spritesheets)
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

        throw new InvalidOperationException($"Sprite '{spriteName}' not found in any spritesheet");
    }

    private class WallSpritesResult
    {
        public required Sprite[] Sprites { get; init; }
        public required Dictionary<int, int> Weights { get; init; }
    }

    private class WallSpritesHybrid
    {
        public Dictionary<string, int>? Base { get; init; }
        public string[]? Accent { get; init; }
    }
}

public enum MapType
{
    Static,
    Random
}

public class MapData
{
    public required MapType Type { get; init; }
    public int[][]? Cells { get; init; }
    public JsonElement? WallSprites { get; init; }
    public string[]? DoorSprites { get; init; }
    public List<DecorationData>? Decorations { get; init; }
    public List<PickupableItemData>? PickupableItems { get; init; }
}

public class DecorationData
{
    public float Density { get; init; }
    public DecorationPlacement Placement { get; init; }
    public Dictionary<string, int>? Items { get; init; }
}

public class PickupableItemData
{
    public float Density { get; init; } = 0.02f;
    public Dictionary<string, int>? Items { get; init; }
    public int MinQuantity { get; init; } = 1;
    public int MaxQuantity { get; init; } = 1;
    public float PickupRadius { get; init; } = 1.5f;
}

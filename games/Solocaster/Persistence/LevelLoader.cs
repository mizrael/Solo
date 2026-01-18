using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Xna.Framework;
using Solo;
using Solo.Assets;
using Solo.Assets.Loaders;
using Solocaster.Entities;
using Solocaster.Persistence.MapBuilding;

namespace Solocaster.Persistence;

public class LevelLoader
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private static readonly EntityTemplateLoader TemplateLoader;
    private static readonly MapBuilderFactory BuilderFactory = new();

    static LevelLoader()
    {
        TemplateLoader = new EntityTemplateLoader();
        var templatesPath = Path.Combine("./data", "templates");
        TemplateLoader.LoadAllTemplatesFromFolder(Path.GetFullPath(templatesPath));
    }

    public static Level LoadFromJson(string path, Game game, GameObject sceneRoot, SpatialGrid spatialGrid)
    {
        var levelData = ParseLevelFile(path);
        var spritesheets = LoadSpritesheets(path, game, levelData);
        var doorSprites = BuildDoorSprites(levelData.Map, spritesheets);

        var context = new MapBuildContext
        {
            Game = game,
            SceneRoot = sceneRoot,
            SpatialGrid = spatialGrid,
            SpriteSheets = spritesheets,
            TemplateLoader = TemplateLoader
        };

        var builder = BuilderFactory.Create(levelData.Map, spritesheets, doorSprites);
        var mapResult = builder.Build(context);

        LoadEntities(game, sceneRoot, spatialGrid, levelData);

        return new Level
        {
            Map = mapResult.Map,
            SpriteSheets = spritesheets,
            WallSprites = mapResult.WallSprites,
            DoorSprites = doorSprites
        };
    }

    private static LevelData ParseLevelFile(string path)
    {
        var json = File.ReadAllText(path);
        var levelData = JsonSerializer.Deserialize<LevelData>(json, JsonOptions);

        if (levelData == null)
            throw new InvalidOperationException("Failed to deserialize level data");

        return levelData;
    }

    private static SpriteSheet[] LoadSpritesheets(string path, Game game, LevelData levelData)
    {
        if (levelData.Spritesheets == null || levelData.Spritesheets.Length == 0)
            throw new InvalidOperationException($"No spritesheets defined in level {path}");

        return levelData.Spritesheets
            .Select(name => SpriteSheetLoader.Get(name, game))
            .ToArray();
    }

    private static Sprite[] BuildDoorSprites(MapData mapData, SpriteSheet[] spritesheets)
    {
        if (mapData.DoorSprites == null || mapData.DoorSprites.Length == 0)
            return Array.Empty<Sprite>();

        return mapData.DoorSprites
            .Select(name => FindSpriteInSheets(name, spritesheets))
            .ToArray();
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

    private static void LoadEntities(Game game, GameObject sceneRoot, SpatialGrid spatialGrid, LevelData levelData)
    {
        if (levelData.Entities == null)
            return;

        foreach (var entityData in levelData.Entities)
        {
            var template = TemplateLoader.Get(entityData.Template);
            var properties = BuildEntityProperties(template, entityData);

            var definition = new EntityDefinition
            {
                Type = template.ItemType,
                TileX = entityData.TileX,
                TileY = entityData.TileY,
                Properties = properties
            };

            EntityFactory.CreateEntity(definition, game, sceneRoot, spatialGrid);
        }
    }

    private static Dictionary<string, object> BuildEntityProperties(
        TemplateDefinition template,
        EntityData entityData)
    {
        var properties = new Dictionary<string, object>();

        foreach (var kvp in template.Properties)
        {
            properties[kvp.Key] = JsonUtils.ConvertJsonElement(kvp.Value);
        }

        if (entityData.Properties != null)
        {
            foreach (var kvp in entityData.Properties)
            {
                properties[kvp.Key] = JsonUtils.ConvertJsonElement(kvp.Value);
            }
        }

        return properties;
    }

    private class LevelData
    {
        public required string[] Spritesheets { get; init; }
        public required MapData Map { get; init; }
        public List<EntityData>? Entities { get; init; }
    }

    private class EntityData
    {
        [JsonPropertyName("template")]
        public required string Template { get; init; }
        public int TileX { get; init; }
        public int TileY { get; init; }
        public Dictionary<string, object>? Properties { get; init; }
    }
}

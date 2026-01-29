using Solocaster.Character;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Solocaster.Inventory;

public static class ItemTemplateLoader
{
    private static readonly Dictionary<string, ItemTemplate> _cache = new();
    private static bool _loaded = false;

    public static void LoadAllFromFolder(string templatesDirectory)
    {
        if (_loaded)
            return;

        if (!Directory.Exists(templatesDirectory))
            throw new ArgumentException($"Item templates directory not found: {templatesDirectory}", nameof(templatesDirectory));

        var templateFiles = Directory.GetFiles(templatesDirectory, "*.json");
        foreach (var file in templateFiles)
        {
            LoadTemplateFile(file);
        }

        _loaded = true;
    }

    private static void LoadTemplateFile(string path)
    {
        var json = File.ReadAllText(path);
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        };

        var data = JsonSerializer.Deserialize<ItemTemplateFileData>(json, options);
        if (data?.Items == null)
            return;

        foreach (var itemData in data.Items)
        {
            var template = new ItemTemplate
            {
                Id = itemData.Id,
                Name = itemData.Name,
                Description = itemData.Description ?? string.Empty,
                IconPath = itemData.IconPath ?? string.Empty,
                WorldSpritePath = itemData.WorldSpritePath ?? string.Empty,
                WorldSpriteScale = itemData.WorldSpriteScale ?? 1f,
                ItemType = ParseEnum<ItemType>(itemData.ItemType),
                EquipSlot = ParseEnum<EquipSlot>(itemData.EquipSlot),
                Weight = itemData.Weight ?? 1f,
                Stackable = itemData.Stackable ?? false,
                MaxStackSize = itemData.MaxStackSize ?? 1,
                StatModifiers = ParseStatDictionary(itemData.StatModifiers),
                Requirements = ParseStatDictionary(itemData.Requirements),
                AttackSpeed = itemData.AttackSpeed ?? 1.0f
            };

            _cache[template.Id] = template;
        }
    }

    private static T ParseEnum<T>(string? value) where T : struct, Enum
    {
        if (string.IsNullOrEmpty(value))
            return default;
        return Enum.TryParse<T>(value, true, out var result) ? result : default;
    }

    private static Dictionary<Stats, float> ParseStatDictionary(Dictionary<string, float>? source)
    {
        var result = new Dictionary<Stats, float>();
        if (source == null)
            return result;

        foreach (var kvp in source)
        {
            if (Enum.TryParse<Stats>(kvp.Key, true, out var statType))
                result[statType] = kvp.Value;
        }

        return result;
    }

    public static ItemTemplate Get(string templateId)
    {
        if (!_cache.TryGetValue(templateId, out var template) || template is null)
            throw new ArgumentException($"Invalid item template id: {templateId}", nameof(templateId));
        return template;
    }

    public static bool TryGet(string templateId, out ItemTemplate? template)
    {
        return _cache.TryGetValue(templateId, out template);
    }

    public static IEnumerable<ItemTemplate> GetAll() => _cache.Values;

    public static void Clear()
    {
        _cache.Clear();
        _loaded = false;
    }

    private class ItemTemplateFileData
    {
        public List<ItemTemplateData>? Items { get; set; }
    }

    private class ItemTemplateData
    {
        public required string Id { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public string? IconPath { get; set; }
        public string? WorldSpritePath { get; set; }
        public float? WorldSpriteScale { get; set; }
        public string? ItemType { get; set; }
        public string? EquipSlot { get; set; }
        public float? Weight { get; set; }
        public bool? Stackable { get; set; }
        public int? MaxStackSize { get; set; }
        public Dictionary<string, float>? StatModifiers { get; set; }
        public Dictionary<string, float>? Requirements { get; set; }
        public float? AttackSpeed { get; set; }
    }
}

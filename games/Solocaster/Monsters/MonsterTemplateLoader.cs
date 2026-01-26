using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Solocaster.Components;
using Solocaster.Inventory;

namespace Solocaster.Monsters;

public static class MonsterTemplateLoader
{
    private static readonly Dictionary<string, MonsterTemplate> _templates = new();

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public static void LoadAllFromFolder(string folderPath)
    {
        if (!Directory.Exists(folderPath))
            return;

        foreach (var file in Directory.GetFiles(folderPath, "*.json"))
        {
            LoadFromFile(file);
        }
    }

    public static void LoadFromFile(string filePath)
    {
        var json = File.ReadAllText(filePath);
        var dto = JsonSerializer.Deserialize<MonsterTemplateDto>(json, _jsonOptions);

        if (dto == null)
            throw new InvalidOperationException($"Failed to deserialize monster template: {filePath}");

        var template = new MonsterTemplate
        {
            Id = dto.Id,
            Name = dto.Name,
            Stats = ParseStats(dto.Stats),
            Behavior = new MonsterBehavior
            {
                DetectionRange = dto.Behavior?.DetectionRange ?? 8.0f,
                AttackRange = dto.Behavior?.AttackRange ?? 1.2f,
                MoveSpeed = dto.Behavior?.MoveSpeed ?? 2.0f
            },
            SpritesheetBasePath = dto.SpritesheetBasePath ?? string.Empty,
            Scale = dto.Scale,
            Anchor = dto.Anchor
        };

        _templates[template.Id] = template;
    }

    public static MonsterTemplate Get(string id)
    {
        if (_templates.TryGetValue(id, out var template))
            return template;

        throw new KeyNotFoundException($"Monster template not found: {id}");
    }

    public static bool TryGet(string id, out MonsterTemplate template)
    {
        return _templates.TryGetValue(id, out template);
    }

    public static IEnumerable<MonsterTemplate> GetAll() => _templates.Values;

    private static Dictionary<StatType, float> ParseStats(Dictionary<string, float> stats)
    {
        var result = new Dictionary<StatType, float>();
        if (stats == null)
            return result;

        foreach (var (key, value) in stats)
        {
            if (Enum.TryParse<StatType>(key, true, out var statType))
                result[statType] = value;
        }

        return result;
    }

    private class MonsterTemplateDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public Dictionary<string, float> Stats { get; set; }
        public BehaviorDto Behavior { get; set; }
        public string SpritesheetBasePath { get; set; }
        public float Scale { get; set; } = 1.0f;
        public BillboardAnchor Anchor { get; set; } = BillboardAnchor.Bottom;

        public class BehaviorDto
        {
            public float DetectionRange { get; set; } = 8.0f;
            public float AttackRange { get; set; } = 1.2f;
            public float MoveSpeed { get; set; } = 2.0f;
        }
    }
}

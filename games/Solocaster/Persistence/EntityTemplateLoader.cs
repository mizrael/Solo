using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Solocaster.Persistence;

public class EntityTemplateLoader
{
    private readonly Dictionary<string, TemplateDefinition> _cache = new();

    public void LoadAllTemplatesFromFolder(string templatesDirectory)
    {
        if (!Directory.Exists(templatesDirectory))
            throw new ArgumentException($"templates directory not found {templatesDirectory}", nameof(templatesDirectory));

        var templateFiles = Directory.GetFiles(templatesDirectory, "*.json");
        foreach (var file in templateFiles)
        {
            LoadTemplateFile(file);
        }
    }

    private void LoadTemplateFile(string path)
    {
        var json = File.ReadAllText(path);
        var data = JsonSerializer.Deserialize<TemplateFileData>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (data == null || string.IsNullOrWhiteSpace(data.Namespace))
            return;

        foreach (var item in data.Items)
        {
            var key = $"{data.Namespace}:{item.Name}";
            _cache[key] = new TemplateDefinition(
                item.ItemType,
                item.Properties
            );
        }
    }

    public TemplateDefinition Get(string templateName)
    {
        if (!_cache.TryGetValue(templateName, out var template) || template is null)
            throw new ArgumentException($"invalid template name: {templateName}", nameof(templateName));
        return template;
    }

    private class TemplateFileData
    {
        public required string Namespace { get; set; }
        public required List<TemplateItemData> Items { get; set; }
    }

    private class TemplateItemData
    {
        public required string Name { get; set; }
        public required string ItemType { get; set; }
        public Dictionary<string, object> Properties { get; set; } = new();
    }
}

public record TemplateDefinition(
    string ItemType,
    IReadOnlyDictionary<string, object> Properties
);

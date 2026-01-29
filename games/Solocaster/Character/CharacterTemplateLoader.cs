using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Solocaster.Character;

public static class CharacterTemplateLoader
{
    private static readonly Dictionary<string, RaceTemplate> _races = new();
    private static readonly Dictionary<string, ClassTemplate> _classes = new();
    private static bool _loaded = false;

    public static void LoadAll(string templatesDirectory)
    {
        if (_loaded)
            return;

        var racesPath = Path.Combine(templatesDirectory, "races.json");
        var classesPath = Path.Combine(templatesDirectory, "classes.json");

        if (File.Exists(racesPath))
            LoadRaces(racesPath);

        if (File.Exists(classesPath))
            LoadClasses(classesPath);

        _loaded = true;
    }

    private static void LoadRaces(string path)
    {
        var json = File.ReadAllText(path);
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        };

        var data = JsonSerializer.Deserialize<RaceFileData>(json, options);
        if (data?.Races == null)
            return;

        foreach (var raceData in data.Races)
        {
            var template = new RaceTemplate
            {
                Id = raceData.Id,
                Name = raceData.Name,
                Description = raceData.Description ?? string.Empty,
                StatBonuses = ParseStatDictionary(raceData.StatBonuses),
                ProgressRates = ParseStatDictionary(raceData.ProgressRates),
                GainMultipliers = ParseStatDictionary(raceData.GainMultipliers),
                ActionProgress = ParseActionProgressDictionary(raceData.ActionProgress),
                SkillEffectiveness = ParseSkillsDictionary(raceData.SkillEffectiveness)
            };

            _races[template.Id] = template;
        }
    }

    private static void LoadClasses(string path)
    {
        var json = File.ReadAllText(path);
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        };

        var data = JsonSerializer.Deserialize<ClassFileData>(json, options);
        if (data?.Classes == null)
            return;

        foreach (var classData in data.Classes)
        {
            var template = new ClassTemplate
            {
                Id = classData.Id,
                Name = classData.Name,
                Description = classData.Description ?? string.Empty,
                StatBonuses = ParseStatDictionary(classData.StatBonuses),
                ProgressRates = ParseStatDictionary(classData.ProgressRates),
                GainMultipliers = ParseStatDictionary(classData.GainMultipliers),
                SkillEffectiveness = ParseSkillsDictionary(classData.SkillEffectiveness)
            };

            _classes[template.Id] = template;
        }
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

    private static Dictionary<Skills, float> ParseSkillsDictionary(Dictionary<string, float>? source)
    {
        var result = new Dictionary<Skills, float>();
        if (source == null)
            return result;

        foreach (var kvp in source)
        {
            if (Enum.TryParse<Skills>(kvp.Key, true, out var skill))
                result[skill] = kvp.Value;
        }

        return result;
    }

    private static Dictionary<MetricType, Dictionary<Stats, float>> ParseActionProgressDictionary(
        Dictionary<string, Dictionary<string, float>>? source)
    {
        var result = new Dictionary<MetricType, Dictionary<Stats, float>>();
        if (source == null)
            return result;

        foreach (var actionKvp in source)
        {
            if (!Enum.TryParse<MetricType>(actionKvp.Key, true, out var metricType))
                continue;

            var statDict = ParseStatDictionary(actionKvp.Value);
            if (statDict.Count > 0)
                result[metricType] = statDict;
        }

        return result;
    }

    public static RaceTemplate GetRace(string raceId)
    {
        if (!_races.TryGetValue(raceId, out var template) || template is null)
            throw new ArgumentException($"Invalid race id: {raceId}", nameof(raceId));
        return template;
    }

    public static ClassTemplate GetClass(string classId)
    {
        if (!_classes.TryGetValue(classId, out var template) || template is null)
            throw new ArgumentException($"Invalid class id: {classId}", nameof(classId));
        return template;
    }

    public static bool TryGetRace(string raceId, out RaceTemplate? template)
    {
        return _races.TryGetValue(raceId, out template);
    }

    public static bool TryGetClass(string classId, out ClassTemplate? template)
    {
        return _classes.TryGetValue(classId, out template);
    }

    public static IEnumerable<RaceTemplate> GetAllRaces() => _races.Values;
    public static IEnumerable<ClassTemplate> GetAllClasses() => _classes.Values;

    public static void Clear()
    {
        _races.Clear();
        _classes.Clear();
        _loaded = false;
    }

    private class RaceFileData
    {
        public List<RaceData>? Races { get; set; }
    }

    private class RaceData
    {
        public required string Id { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public Dictionary<string, float>? StatBonuses { get; set; }
        public Dictionary<string, float>? ProgressRates { get; set; }
        public Dictionary<string, float>? GainMultipliers { get; set; }
        public Dictionary<string, Dictionary<string, float>>? ActionProgress { get; set; }
        public Dictionary<string, float>? SkillEffectiveness { get; set; }
    }

    private class ClassFileData
    {
        public List<ClassData>? Classes { get; set; }
    }

    private class ClassData
    {
        public required string Id { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public Dictionary<string, float>? StatBonuses { get; set; }
        public Dictionary<string, float>? ProgressRates { get; set; }
        public Dictionary<string, float>? GainMultipliers { get; set; }
        public Dictionary<string, float>? SkillEffectiveness { get; set; }
    }
}

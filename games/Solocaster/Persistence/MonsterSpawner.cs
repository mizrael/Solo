using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Solo;
using Solocaster.Entities;
using Solocaster.Monsters;

namespace Solocaster.Persistence;

public class MonsterSpawner
{
    private readonly Random _random = new();

    public List<GameObject> Spawn(
        MonsterSpawnConfig config,
        Map map,
        Game game,
        GameObject sceneRoot,
        SpatialGrid spatialGrid,
        GameObject player)
    {
        var monsters = new List<GameObject>();
        var occupiedTiles = new HashSet<(int, int)>();

        for (int y = 0; y < map.Rows; y++)
        {
            for (int x = 0; x < map.Cols; x++)
            {
                if (!IsFloor(map, x, y))
                    continue;

                if (occupiedTiles.Contains((x, y)))
                    continue;

                if (_random.NextDouble() > config.Density)
                    continue;

                var encounter = PickEncounter(config.Encounters);
                if (encounter == null)
                    continue;

                var spawnedMonsters = SpawnEncounter(
                    encounter, x, y, map, game, sceneRoot, spatialGrid, player, occupiedTiles);
                monsters.AddRange(spawnedMonsters);
            }
        }

        return monsters;
    }

    private static bool IsFloor(Map map, int x, int y)
    {
        if (x < 0 || x >= map.Cols || y < 0 || y >= map.Rows)
            return false;
        return map.Cells[y][x] == TileTypes.Floor;
    }

    private EncounterConfig PickEncounter(List<EncounterConfig> encounters)
    {
        if (encounters == null || encounters.Count == 0)
            return null;

        var totalWeight = 0;
        foreach (var e in encounters)
            totalWeight += e.Weight;

        var roll = _random.Next(totalWeight);
        var cumulative = 0;

        foreach (var encounter in encounters)
        {
            cumulative += encounter.Weight;
            if (roll < cumulative)
                return encounter;
        }

        return encounters[^1];
    }

    private List<GameObject> SpawnEncounter(
        EncounterConfig encounter,
        int startX, int startY,
        Map map,
        Game game,
        GameObject sceneRoot,
        SpatialGrid spatialGrid,
        GameObject player,
        HashSet<(int, int)> occupiedTiles)
    {
        var monsters = new List<GameObject>();

        foreach (var group in encounter.Groups)
        {
            if (!MonsterTemplateLoader.TryGet(group.Id, out var template))
                continue;

            var count = _random.Next(group.Min, group.Max + 1);

            for (int i = 0; i < count; i++)
            {
                var (tileX, tileY) = FindNearbyFloorTile(startX, startY, map, occupiedTiles);
                if (tileX < 0)
                    continue;

                occupiedTiles.Add((tileX, tileY));

                var worldPos = new Vector2(tileX + 0.5f, tileY + 0.5f);
                var monster = MonsterFactory.Create(template, worldPos, game, sceneRoot, spatialGrid, player);
                monsters.Add(monster);
            }
        }

        return monsters;
    }

    private (int x, int y) FindNearbyFloorTile(int centerX, int centerY, Map map, HashSet<(int, int)> occupied)
    {
        // Spiral outward from center
        for (int radius = 0; radius <= 3; radius++)
        {
            for (int dy = -radius; dy <= radius; dy++)
            {
                for (int dx = -radius; dx <= radius; dx++)
                {
                    if (Math.Abs(dx) != radius && Math.Abs(dy) != radius)
                        continue;

                    var x = centerX + dx;
                    var y = centerY + dy;

                    if (x < 0 || x >= map.Cols || y < 0 || y >= map.Rows)
                        continue;

                    if (!IsFloor(map, x, y))
                        continue;

                    if (occupied.Contains((x, y)))
                        continue;

                    return (x, y);
                }
            }
        }

        return (-1, -1);
    }
}

public class MonsterSpawnConfig
{
    public float Density { get; set; }
    public List<EncounterConfig> Encounters { get; set; } = new();
}

public class EncounterConfig
{
    public int Weight { get; set; }
    public List<MonsterGroupConfig> Groups { get; set; } = new();
}

public class MonsterGroupConfig
{
    public string Id { get; set; }
    public int Min { get; set; } = 1;
    public int Max { get; set; } = 1;
}

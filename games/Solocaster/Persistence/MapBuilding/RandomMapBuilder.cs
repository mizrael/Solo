using System;
using System.Collections.Generic;
using Solo.Assets;
using Solocaster.DungeonGenerator;
using Solocaster.Entities;
using Solocaster.Inventory;
using Random = System.Random;
using Map = Solocaster.Entities.Map;

namespace Solocaster.Persistence.MapBuilding;

public class RandomMapBuilder : IMapBuilder
{
    private readonly RandomMapConfig _config;

    public RandomMapBuilder(RandomMapConfig config)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
    }

    public MapBuildResult Build(MapBuildContext context)
    {
        var cells = GenerateDungeon();
        MapBuildUtils.EnsurePerimeterClosed(cells);

        var map = new Map(cells, _config.DoorSpriteCount);

        PlaceDecorations(context, map);
        PlacePickupableItems(context, map);

        return new MapBuildResult
        {
            Map = map,
            WallSprites = _config.WallSprites
        };
    }

    private int[][] GenerateDungeon()
    {
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

        return ConvertTilesToCells(tiles);
    }

    private int[][] ConvertTilesToCells(TileType[,] tiles)
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

                if (tileType.IsWall() && _config.WallSpriteWeights?.Count > 0)
                {
                    cellId = PickWallCellId(cells, row, col);
                }

                cells[row][col] = cellId;
            }
        }

        return cells;
    }

    private int PickWallCellId(int[][] cells, int row, int col)
    {
        var neighborWallIds = new List<int>();

        int leftCell = col > 0 ? cells[row][col - 1] : 0;
        if (IsWallCell(leftCell))
            neighborWallIds.Add(leftCell);

        int topCell = row > 0 ? cells[row - 1][col] : 0;
        if (IsWallCell(topCell))
            neighborWallIds.Add(topCell);

        if (neighborWallIds.Count > 0 && Random.Shared.Next(100) < 80)
        {
            return neighborWallIds[Random.Shared.Next(neighborWallIds.Count)];
        }

        return _config.WallSpriteWeights!.WeightedRandom();
    }

    private static bool IsWallCell(int cell)
    {
        return cell > 0 && cell != TileTypes.DoorVertical && cell != TileTypes.DoorHorizontal;
    }

    private static int MapTileTypeToCell(TileType tileType)
    {
        return tileType switch
        {
            TileType.Empty or TileType.Void => TileTypes.Floor,
            TileType.DoorVertical => TileTypes.DoorVertical,
            TileType.DoorHorizontal => TileTypes.DoorHorizontal,
            _ when tileType.IsWall() => 1,
            _ => TileTypes.Floor
        };
    }

    private void PlaceDecorations(MapBuildContext context, Map map)
    {
        if (_config.Decorations == null)
            return;

        var floorTiles = CollectFloorTiles(map);
        var wallAdjacentTiles = CollectWallAdjacentTiles(map, floorTiles);
        var occupiedTiles = new HashSet<(int, int)>();

        foreach (var decoration in _config.Decorations)
        {
            if (decoration.Items == null || decoration.Items.Count == 0)
                continue;

            var eligibleTiles = decoration.Placement == DecorationPlacement.Wall
                ? wallAdjacentTiles
                : floorTiles;

            PlaceDecorationsOnTiles(context, map, decoration, eligibleTiles, occupiedTiles);
        }
    }

    private void PlaceDecorationsOnTiles(
        MapBuildContext context,
        Map map,
        DecorationConfig decoration,
        List<(int col, int row)> eligibleTiles,
        HashSet<(int, int)> occupiedTiles)
    {
        foreach (var (col, row) in eligibleTiles)
        {
            if (occupiedTiles.Contains((col, row)))
                continue;

            if (Random.Shared.NextDouble() > decoration.Density)
                continue;

            var templateName = decoration.Items!.WeightedRandom();
            var templateData = context.TemplateLoader.Get(templateName);
            var properties = BuildEntityProperties(templateData);

            if (decoration.Placement == DecorationPlacement.Wall)
            {
                ApplyWallOffset(map, col, row, properties);
            }

            var definition = new EntityDefinition
            {
                Type = templateData.ItemType,
                TileX = col,
                TileY = row,
                Properties = properties
            };

            EntityFactory.CreateEntity(definition, context.Game, context.SceneRoot, context.SpatialGrid);
            occupiedTiles.Add((col, row));
        }
    }

    private static Dictionary<string, object> BuildEntityProperties(TemplateDefinition templateData)
    {
        var properties = new Dictionary<string, object>();
        foreach (var kvp in templateData.Properties)
        {
            properties[kvp.Key] = JsonUtils.ConvertJsonElement(kvp.Value);
        }
        return properties;
    }

    private static void ApplyWallOffset(Map map, int col, int row, Dictionary<string, object> properties)
    {
        var wallDir = GetAdjacentWallDirection(map, col, row);
        if (wallDir.HasValue)
        {
            const float wallOffset = 0.4f;
            properties["offsetX"] = wallDir.Value.colOffset * wallOffset;
            properties["offsetY"] = wallDir.Value.rowOffset * wallOffset;
        }
    }

    private void PlacePickupableItems(MapBuildContext context, Map map)
    {
        if (_config.PickupableItems == null)
            return;

        var floorTiles = CollectFloorTilesExcludingStart(map);
        var occupiedTiles = new HashSet<(int, int)>();

        foreach (var itemConfig in _config.PickupableItems)
        {
            if (itemConfig.Items == null || itemConfig.Items.Count == 0)
                continue;

            PlaceItemsOnTiles(context, itemConfig, floorTiles, occupiedTiles);
        }
    }

    private static void PlaceItemsOnTiles(
        MapBuildContext context,
        PickupableItemConfig itemConfig,
        List<(int col, int row)> floorTiles,
        HashSet<(int, int)> occupiedTiles)
    {
        foreach (var (col, row) in floorTiles)
        {
            if (occupiedTiles.Contains((col, row)))
                continue;

            if (Random.Shared.NextDouble() > itemConfig.Density)
                continue;

            var itemTemplateId = itemConfig.Items!.WeightedRandom();

            if (!ItemTemplateLoader.TryGet(itemTemplateId, out _))
                continue;

            int quantity = itemConfig.MinQuantity > 0 && itemConfig.MaxQuantity > 0
                ? Random.Shared.Next(itemConfig.MinQuantity, itemConfig.MaxQuantity + 1)
                : 1;

            EntityFactory.CreatePickupableItem(
                itemTemplateId,
                col,
                row,
                context.Game,
                context.SceneRoot,
                context.SpatialGrid,
                quantity,
                itemConfig.PickupRadius
            );

            occupiedTiles.Add((col, row));
        }
    }

    private static List<(int col, int row)> CollectFloorTiles(Map map)
    {
        var tiles = new List<(int col, int row)>();

        for (int row = 0; row < map.Rows; row++)
        {
            for (int col = 0; col < map.Cols; col++)
            {
                if (map.Cells[row][col] == TileTypes.Floor)
                {
                    tiles.Add((col, row));
                }
            }
        }

        return tiles;
    }

    private static List<(int col, int row)> CollectFloorTilesExcludingStart(Map map)
    {
        var tiles = new List<(int col, int row)>();
        var startPos = map.GetStartingPosition();
        int startCol = (int)startPos.X;
        int startRow = (int)startPos.Y;

        for (int row = 0; row < map.Rows; row++)
        {
            for (int col = 0; col < map.Cols; col++)
            {
                if (map.Cells[row][col] != TileTypes.Floor)
                    continue;

                if (Math.Abs(col - startCol) <= 1 && Math.Abs(row - startRow) <= 1)
                    continue;

                tiles.Add((col, row));
            }
        }

        return tiles;
    }

    private static List<(int col, int row)> CollectWallAdjacentTiles(Map map, List<(int col, int row)> floorTiles)
    {
        var wallAdjacent = new List<(int col, int row)>();

        foreach (var tile in floorTiles)
        {
            if (GetAdjacentWallDirection(map, tile.col, tile.row).HasValue)
            {
                wallAdjacent.Add(tile);
            }
        }

        return wallAdjacent;
    }

    private static (int colOffset, int rowOffset)? GetAdjacentWallDirection(Map map, int col, int row)
    {
        (int colOffset, int rowOffset)[] directions = [(0, -1), (0, 1), (-1, 0), (1, 0)];

        foreach (var dir in directions)
        {
            int newCol = col + dir.colOffset;
            int newRow = row + dir.rowOffset;

            if (newCol < 0 || newCol >= map.Cols || newRow < 0 || newRow >= map.Rows)
                continue;

            var cell = map.Cells[newRow][newCol];
            if (IsWallCell(cell))
                return dir;
        }

        return null;
    }
}

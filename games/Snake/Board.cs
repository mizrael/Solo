using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Snake;

public class Board
{
    private readonly HashSet<Point> _food = new();
    private readonly int _maxFood;
    private TileType[,] _tiles;

    public Board(int width, int height, int maxFood = 2)
    {
        this.Width = width;
        this.Height = height;
        this._maxFood = maxFood;
        this._tiles = new TileType[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                var type = (x != 0 && x != width - 1 && y != 0 && y != height - 1) ? TileType.Empty : TileType.Wall;
                this._tiles[x, y] = type;
            }
        }
    }

    public Point GetRandomEmptyTile()
    {
        var x = 0;
        var y = 0;
        do
        {
            x = Random.Shared.Next(1, Width - 1);
            y = Random.Shared.Next(1, Height - 1);
        } while (_tiles[x, y] != TileType.Empty);

        return new Point(x, y);
    }

    public void SpawnFood()
    {
        if (_food.Count >= _maxFood)
            return;

        var coords = GetRandomEmptyTile();
        _tiles[coords.X, coords.Y] = TileType.Food;
        _food.Add(coords);
    }

    public TileType GetTileAt(Point point) => _tiles[point.X, point.Y];

    public TileType GetTileAt(int x, int y) => _tiles[x, y];

    public void ClearTile(Point tile)
    {
        _tiles[tile.X, tile.Y] = TileType.Empty;
        _food.Remove(tile);
    }

    public int Width { get; }
    public int Height { get; }
}

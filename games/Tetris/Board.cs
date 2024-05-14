using System;
using Microsoft.Xna.Framework;

namespace Tetris;

public record Tile
{
    public bool IsFilled => Color.HasValue;
    public Color? Color { get; set; }
}

public record Board
{
    private readonly Tile[,] _cells;

    public Board(int width, int height)
    {
        Width = width;
        Height = height;
        _cells = new Tile[width, height];
        for(int x = 0; x < width; x++)
            for(int y = 0; y < height; y++)
                _cells[x, y] = new Tile();
    }

    public bool IsRowFull(int row)
    {
        for (int x = 0; x < Width; x++)
        {
            if (!_cells[x, row].IsFilled)
                return false;
        }

        return true;
    }

    public Tile GetTileAt(int x, int y)
        => _cells[x, y];

    public int Width { get; }
    public int Height { get; }
}

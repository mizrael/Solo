using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Tetris;

public record Board
{
    private readonly Tile[,] _tiles;
    private readonly Dictionary<int, HashSet<Tile>> _tilesByPiece = new();

    public Board(int width, int height)
    {
        Width = width;
        Height = height;
        _tiles = new Tile[width, height];
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                _tiles[x, y] = new Tile();
    }

    public bool IsRowFull(int row)
    {
        for (int x = 0; x < Width; x++)
        {
            if (!_tiles[x, row].IsFilled)
                return false;
        }

        return true;
    }

    public Tile GetTileAt(int x, int y)
        => _tiles[x, y];

    public void Place(Piece piece)
    {
        var shape = piece.CurrentShape;

        var pieceTiles = _tilesByPiece.GetValueOrDefault(piece.Id) ?? new HashSet<Tile>();
        foreach (var tile in pieceTiles)
            tile.Piece = null;
        pieceTiles.Clear();

        for (int y = 0; y < shape.Tiles.GetLength(1); y++)
        {
            for (int x = 0; x < shape.Tiles.GetLength(0); x++)
            {
                var isFilled = shape.Tiles[x, y];
                if (!isFilled)
                    continue;

                var newX = piece.Position.X + x;
                var newY = piece.Position.Y + y;
                if (newX < 0 || newX >= Width || newY < 0 || newY >= Height)
                    continue;

                var tile = _tiles[newX, newY];
                tile.Piece = piece;
                pieceTiles.Add(tile);
            }
        }

        _tilesByPiece[piece.Id] = pieceTiles;
    }

    public bool CanPlace(Piece piece, Point newPosition)
    {
        var shape = piece.CurrentShape;

        for (int y = 0; y < shape.Tiles.GetLength(1); y++)
        for (int x = 0; x < shape.Tiles.GetLength(0); x++)
        {
            var isFilled = shape.Tiles[x, y];
            if (!isFilled)
                continue;

            var newX = newPosition.X + x;
            var newY = newPosition.Y + y;
            if (newX < 0 || newX >= Width || newY < 0 || newY >= Height)
                return false;

            var tile = _tiles[newX, newY];
            if (tile.IsFilled && tile.Piece != piece)
                return false;
        }

        return true;
    }

    public int Width { get; }
    public int Height { get; }
}

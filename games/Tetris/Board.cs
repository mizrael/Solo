using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;

namespace Tetris;

public sealed record Board
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

        for (int row = 0; row < shape.Tiles.GetLength(1); row++)
        {
            for (int col = 0; col < shape.Tiles.GetLength(0); col++)
            {
                var isFilled = shape.Tiles[col, row];
                if (!isFilled)
                    continue;

                var newX = piece.Position.X + col;
                var newY = piece.Position.Y + row;
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

        for (int row = 0; row < shape.Tiles.GetLength(1); row++)
        for (int col = 0; col < shape.Tiles.GetLength(0); col++)
        {
            var isFilled = shape.Tiles[col, row];
            if (!isFilled)
                continue;

            var newX = newPosition.X + col;
            var newY = newPosition.Y + row;
            if (newX < 0 || newX >= Width || newY < 0 || newY >= Height)
                return false;

            var tile = _tiles[newX, newY];
            if (tile.IsFilled && tile.Piece != piece)
                return false;
        }

        return true;
    }

    public bool UpdateRows()
    {
        for (int row = Height - 1; row > -1; row--)
        {
            if (!IsRowFull(row))
                continue;

            for (int col = 0; col < Width; col++)
            {
                var curr = row;
                while (curr >= 0)
                {
                    var prev = curr == 0 ? null : _tiles[col, curr - 1];
                    _tiles[col, curr].Piece = prev?.Piece;
                    if (prev is null)
                        break;
                    curr--;
                }
            }

            return true;
        }

        return false;
    }

    public int Width { get; }
    public int Height { get; }
}

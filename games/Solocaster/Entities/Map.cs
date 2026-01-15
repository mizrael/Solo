using Microsoft.Xna.Framework;
using Solo.Assets;
using System;
using System.Collections.Generic;

namespace Solocaster.Entities;

public static class TileTypes
{
    public const int Floor = -1;
    public const int StartingPosition = 99;
    public const int Door = 100;
}

public class Map
{
    public readonly int[][] Cells;
    public readonly int Cols;
    public readonly int Rows;

    private readonly Dictionary<(int, int), Door> _doors = new();
    private readonly (int col, int row) _startingTile;

    public Map(int[][] cells)
    {
        Cells = cells;
        Rows = Cells.Length;
        Cols = Cells.Length;

        (int col, int row)? startingTile = null;

        for (int row = 0; row != Rows; row++)
        {
            for (int col = 0; col != Cols; col++)
            {
                var cell = Cells[row][col];

                if (cell == TileTypes.StartingPosition)
                {
                    startingTile = (col, row);
                    // Convert the starting position to a floor tile
                    Cells[row][col] = TileTypes.Floor;
                }
                else if (cell == TileTypes.Door)
                {
                    var door = new Door(row, col, true);
                    _doors[(col, row)] = door;
                }
            }
        }

        // If no explicit starting position found, pick a random empty tile
        if (startingTile == null)
        {
            var random = new Random();
            var emptyTiles = new List<(int col, int row)>();

            for (int row = 0; row < Rows; row++)
            {
                for (int col = 0; col < Cols; col++)
                {
                    if (Cells[row][col] == TileTypes.Floor)
                    {
                        emptyTiles.Add((col, row));
                    }
                }
            }

            if (emptyTiles.Count == 0)
                throw new InvalidOperationException("Map has no starting position and no empty floor tiles");

            startingTile = emptyTiles[random.Next(emptyTiles.Count)];
        }

        _startingTile = startingTile.Value;
    }

    public Vector2 GetStartingPosition()
    => new Vector2(_startingTile.col + 0.5f, _startingTile.row + 0.5f);

    public bool IsBlocked(int x, int y)
    {
        if (x < 0 || x >= Cols || y < 0 || y >= Rows)
            return true;

        var cell = Cells[y][x];
        if (cell == TileTypes.Floor || cell == TileTypes.StartingPosition)
            return false;

        if (cell == TileTypes.Door)
        {
            var door = GetDoor(x, y);
            return door!.IsBlocking;
        }

        return true;
    }

    public Door? GetDoor(int x, int y)
    => _doors.TryGetValue((x, y), out var door) ? door : null;

    public void Update(GameTime gameTime)
    {
        foreach (var door in _doors.Values)
        {
            door.Update(gameTime);
        }
    }

    public Vector2 FindInterceptionPoint(Vector2 rayStart, Vector2 rayDir)
    {
        int mapX = (int)rayStart.X;
        int mapY = (int)rayStart.Y;

        float deltaDistX = (rayDir.X == 0) ? 1e30f : Math.Abs(1 / rayDir.X);
        float deltaDistY = (rayDir.Y == 0) ? 1e30f : Math.Abs(1 / rayDir.Y);

        int stepX;
        int stepY;
        float sideDistX;
        float sideDistY;

        if (rayDir.X < 0)
        {
            stepX = -1;
            sideDistX = (rayStart.X - mapX) * deltaDistX;
        }
        else
        {
            stepX = 1;
            sideDistX = (mapX + 1.0f - rayStart.X) * deltaDistX;
        }

        if (rayDir.Y < 0)
        {
            stepY = -1;
            sideDistY = (rayStart.Y - mapY) * deltaDistY;
        }
        else
        {
            stepY = 1;
            sideDistY = (mapY + 1.0f - rayStart.Y) * deltaDistY;
        }

        bool hit = false;
        bool isHorizontalWall = false;
        while (!hit)
        {
            if (sideDistX < sideDistY)
            {
                sideDistX += deltaDistX;
                mapX += stepX;
                isHorizontalWall = false;
            }
            else
            {
                sideDistY += deltaDistY;
                mapY += stepY;
                isHorizontalWall = true;
            }

            // Check bounds - treat out of bounds as hitting a wall
            if (mapX < 0 || mapX >= Cols || mapY < 0 || mapY >= Rows)
            {
                hit = true;
            }
            else if (this.Cells[mapY][mapX] >= 0)
            {
                hit = true;
            }
        }

        float perpWallDist = isHorizontalWall == false
            ? (sideDistX - deltaDistX)
            : (sideDistY - deltaDistY);

        Vector2 pointOnWall;
        if (!isHorizontalWall)
        {
            pointOnWall.X = mapX + (stepX < 0 ? 1.0f : 0.0f);
            pointOnWall.Y = rayStart.Y + perpWallDist * rayDir.Y;
        }
        else
        {
            pointOnWall.X = rayStart.X + perpWallDist * rayDir.X;
            pointOnWall.Y = mapY + (stepY < 0 ? 1.0f : 0.0f);
        }

        return pointOnWall;
    }
}

public class Level
{
    public required Map Map { get; init; }

    public required SpriteSheet[] SpriteSheets { get; init; }

    public required Sprite[] Sprites { get; init; }
}
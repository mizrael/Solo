using System;

namespace Pacman.Components;

public record TileInfo(int Row, int Col, TileTypes Type)
{
    public bool IsWalkable => Type != TileTypes.Wall;

    public static float Distance(TileInfo t1, TileInfo t2)
    {
        int dx = t2.Row - t1.Row;
        int dy = t2.Col - t1.Col;
        return MathF.Sqrt(dx * dx + dy * dy);
    }
}
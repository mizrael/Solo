using System;

namespace Pacman;

public record TilePos(int Row, int Col)
{
    public static float Distance(TilePos t1, TilePos t2)
    {
        int dx = t2.Row - t1.Row;
        int dy = t2.Col - t1.Col;
        return MathF.Sqrt(dx * dx + dy * dy);
    }

    public TilePos Add(int val, Directions direction)
    {
        return direction switch
        {
            Directions.Up => new TilePos(val + this.Row, this.Col),
            Directions.Right => new TilePos(this.Row, val + this.Col),
            Directions.Down => new TilePos(-val + this.Row, this.Col),
            Directions.Left => new TilePos(this.Row, -val + this.Col),
            _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
        };
    }

    public static TilePos operator +(TilePos t1, TilePos t2)
    => new(t1.Row + t2.Row, t1.Col + t2.Col);

    public static TilePos operator -(TilePos t1, TilePos t2)
    => new(t1.Row - t2.Row, t1.Col - t2.Col);

    public static TilePos operator *(TilePos t1, int val)
        => new(t1.Row * val, t1.Col * val);
}

public record TileInfo(int Row, int Col, TileTypes Type) : TilePos(Row, Col)
{
    public bool IsWalkable => Type != TileTypes.Wall;
}
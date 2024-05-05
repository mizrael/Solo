using System;

namespace Snake;

public class Board
{
    public Board(int width, int height)
    {
        this.Width = width;
        this.Height = height;
        this.Tiles = new TileType[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                var type = (x != 0 && x != width - 1 && y != 0 && y != height - 1) ? TileType.Empty : TileType.Wall;
                this.Tiles[x, y] = type;
            }
        }
    }

    public void GenerateFood()
    {
        var x = 0;
        var y = 0;
        do
        {
            x = Random.Shared.Next(1, Width - 1);
            y = Random.Shared.Next(1, Height - 1);
        } while (Tiles[x, y] != TileType.Empty);

        Tiles[x, y] = TileType.Food;
    }

    public int Width { get; }
    public int Height { get; }

    public TileType[,] Tiles { get; }
}

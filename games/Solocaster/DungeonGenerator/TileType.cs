namespace Solocaster.DungeonGenerator;

public enum TileType
{
   
    Door = 43,

    Wall = 178,

    WallSE = 179,
    WallSO = 180,
    WallNE = 181,
    WallNO = 182,

    WallNS = 183,
    WallEO = 184,

    WallESO = 185,
    WallNEO = 186,
    WallNES = 187,
    WallNSO = 188,

    WallNESO = 189,

    Void = 248,
    Empty = 249
}

public static class TileTypeExtensions
{
    public static bool IsWall(this TileType tileType)
    {
        return tileType is TileType.Wall or
            TileType.WallSE or
            TileType.WallSO or
            TileType.WallNE or
            TileType.WallNO or
            TileType.WallNS or
            TileType.WallEO or
            TileType.WallESO or
            TileType.WallNEO or
            TileType.WallNES or
            TileType.WallNSO or
            TileType.WallNESO;
    }
}
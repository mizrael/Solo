namespace Solocaster.DungeonGenerator;

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
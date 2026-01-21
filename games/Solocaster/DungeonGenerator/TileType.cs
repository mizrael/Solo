namespace Solocaster.DungeonGenerator;

public enum TileType
{
    DoorVertical = 44,  // Door spanning N-S, player approaches from E or W
    DoorHorizontal = 45,// Door spanning E-W, player approaches from N or S

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

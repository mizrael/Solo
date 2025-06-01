namespace Pacman;

public enum TileTypes 
{
    Empty = 0,
    Pellet = 1,
    Wall = 2,
    MagicPill = 4
}

public enum GhostTypes
{
    Blinky = 0,
    Pinky = 1,
    Inky = 2,
    Clyde = 3
}

public enum GhostStates
{
    Normal = 0,
    Scared,
    Eaten
}

public enum GhostAnimations
{
    Walk = 0,
    Scared1 = 1,
    Scared2 = 2,
}

public enum Directions
{
    None,
    Up,
    Down,
    Left,
    Right
}
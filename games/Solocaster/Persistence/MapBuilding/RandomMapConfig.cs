using System.Collections.Generic;
using Solo.Assets;

namespace Solocaster.Persistence.MapBuilding;

public class RandomMapConfig
{
    public required Sprite[] WallSprites { get; init; }
    public Dictionary<int, int>? WallSpriteWeights { get; init; }
    public int DoorSpriteCount { get; init; }
    public List<DecorationConfig>? Decorations { get; init; }
    public List<PickupableItemConfig>? PickupableItems { get; init; }
}

public enum DecorationPlacement
{
    Floor,
    Wall
}

public class DecorationConfig
{
    public float Density { get; init; }
    public DecorationPlacement Placement { get; init; }
    public Dictionary<string, int>? Items { get; init; }
}

public class PickupableItemConfig
{
    public float Density { get; init; } = 0.02f;
    public Dictionary<string, int>? Items { get; init; }
    public int MinQuantity { get; init; } = 1;
    public int MaxQuantity { get; init; } = 1;
}

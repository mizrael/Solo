using Solo;
using Solo.Assets;
using System.Collections.Generic;

namespace Solocaster.Entities;

public class Level
{
    public required Map Map { get; init; }

    public required SpriteSheet[] SpriteSheets { get; init; }

    public required Sprite[] WallSprites { get; init; }

    public required Sprite[] DoorSprites { get; init; }

    public List<GameObject> Monsters { get; set; } = new();
}
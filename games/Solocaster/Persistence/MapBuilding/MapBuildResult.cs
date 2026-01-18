using Solo.Assets;

namespace Solocaster.Persistence.MapBuilding;

public class MapBuildResult
{
    public required Entities.Map Map { get; init; }
    public required Sprite[] WallSprites { get; init; }
}

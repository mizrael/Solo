using Microsoft.Xna.Framework;
using Solo;
using Solo.Assets;

namespace Solocaster.Persistence.MapBuilding;

public class MapBuildContext
{
    public required Game Game { get; init; }
    public required GameObject EntityContainer { get; init; }
    public required SpriteSheet[] SpriteSheets { get; init; }
    public required EntityTemplateLoader TemplateLoader { get; init; }
}

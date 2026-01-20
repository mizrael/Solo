using System.IO;
using Microsoft.Xna.Framework;
using Solo.Assets.Loaders;
using Solocaster.Monsters;

namespace Solocaster.Animations;

public static class DirectionalAnimationLoader
{
    private static readonly Direction[] AllDirections = { Direction.Front, Direction.Back, Direction.Left, Direction.Right };

    public static DirectionalAnimation Load(string basePath, Game game)
    {
        var animation = new DirectionalAnimation();

        foreach (var direction in AllDirections)
        {
            var suffix = direction.ToString().ToLower();
            var fullPath = $"{basePath}_{suffix}.json";

            if (File.Exists(fullPath))
            {
                var sheet = AnimatedSpriteSheetLoader.Load(fullPath, game);
                animation.Add(direction, sheet);
            }
        }

        // If no directions loaded, try loading without suffix (backwards compat)
        if (!animation.HasAny)
        {
            var defaultPath = $"{basePath}.json";
            if (File.Exists(defaultPath))
            {
                var sheet = AnimatedSpriteSheetLoader.Load(defaultPath, game);
                animation.Add(Direction.Front, sheet);
            }
        }

        return animation;
    }
}

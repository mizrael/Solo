using System;
using System.IO;
using Microsoft.Xna.Framework;
using Solo.Assets.Loaders;
using Solocaster.Monsters;

namespace Solocaster.Components;

public static class DirectionalSpriteLoader
{
    private static readonly Direction[] AllDirections = { Direction.Front, Direction.Back, Direction.Left, Direction.Right };
    private static readonly Random Random = new();

    public static DirectionalSpriteProvider Load(string basePath, Game game)
    {
        var provider = new DirectionalSpriteProvider();

        foreach (var direction in AllDirections)
        {
            var suffix = direction.ToString().ToLower();
            var sheetPath = $"{basePath}_{suffix}";
            var fullPath = Path.Combine(SpriteSheetLoader.BasePath, sheetPath + ".json");

            if (File.Exists(fullPath))
            {
                var sheet = SpriteSheetLoader.Get(sheetPath, game);
                var randomIndex = Random.Next(sheet.Sprites.Count);
                var sprite = sheet.Sprites[randomIndex];
                provider.AddSprite(direction, sprite);
            }
        }

        return provider;
    }
}

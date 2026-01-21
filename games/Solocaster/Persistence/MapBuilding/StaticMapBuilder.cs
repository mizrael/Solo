using System;
using System.Collections.Generic;
using Solo.Assets;
using Solocaster.Entities;

namespace Solocaster.Persistence.MapBuilding;

public class StaticMapBuilder : IMapBuilder
{
    private readonly int[][] _cells;
    private readonly int _doorSpriteCount;

    public StaticMapBuilder(int[][] cells, int doorSpriteCount)
    {
        _cells = cells ?? throw new ArgumentNullException(nameof(cells));
        _doorSpriteCount = doorSpriteCount;
    }

    public MapBuildResult Build(MapBuildContext context)
    {
        MapBuildUtils.EnsurePerimeterClosed(_cells);

        var wallSprites = BuildWallSprites(context.SpriteSheets);
        var map = new Map(_cells, _doorSpriteCount);

        return new MapBuildResult
        {
            Map = map,
            WallSprites = wallSprites
        };
    }

    private Sprite[] BuildWallSprites(SpriteSheet[] spritesheets)
    {
        var sprites = new List<Sprite>();

        foreach (var spritesheet in spritesheets)
        {
            foreach (var sprite in spritesheet.Sprites)
            {
                sprites.Add(sprite);
            }
        }

        return sprites.ToArray();
    }
}

using Microsoft.Xna.Framework;
using Solo;
using Solo.Assets.Loaders;
using Solo.Components;
using Solocaster.Components;
using Solocaster.Services;
using System;

namespace Solocaster.Entities;

public static class EntityFactory
{
    public static GameObject CreateEntity(
        EntityDefinition definition,
        Game game,
        EntityManager entityManager)
    {
        return definition.Type switch
        {
            "sprite" => CreateSpriteEntity(definition, game, entityManager),
            _ => throw new NotSupportedException($"Entity type '{definition.Type}' not supported")
        };
    }

    private static GameObject CreateSpriteEntity(
        EntityDefinition definition,
        Game game,
        EntityManager entityManager)
    {
        var sheetName = definition.Properties["spritesheet"] as string;
        ArgumentException.ThrowIfNullOrWhiteSpace(sheetName, nameof(definition.Properties));

        var spriteName = definition.Properties["sprite"] as string;
        ArgumentException.ThrowIfNullOrWhiteSpace(spriteName, nameof(definition.Properties));

        var spriteSheet = SpriteSheetLoader.Get(sheetName, game);
        var sprite = spriteSheet.Get(spriteName);

        var entity = new GameObject();
        var transform = entity.Components.Add<TransformComponent>();
        transform.Local.Position = new Vector2(
            definition.TileX + 0.5f, 
            definition.TileY + 0.5f
        );

        var billboard = entity.Components.Add<BillboardComponent>();
        billboard.Sprite = sprite;

        float scaleX = 1f;
        if (definition.Properties.TryGetValue("scaleX", out var scaleXObj) && scaleXObj is float tmpScaleX)
            scaleX = tmpScaleX;
        float scaleY = 1f;
        if (definition.Properties.TryGetValue("scaleY", out var scaleYObj) && scaleYObj is float tmpScaleY)
            scaleY = tmpScaleY;
        billboard.Scale = new Vector2(scaleX, scaleY);

        // Set anchor
        if (definition.Properties.TryGetValue("anchor", out var anchorObj) && anchorObj is string anchorStr)
        {
            billboard.Anchor = anchorStr.ToLowerInvariant() switch
            {
                "bottom" => BillboardAnchor.Bottom,
                "center" => BillboardAnchor.Center,
                "top" => BillboardAnchor.Top,
                _ => BillboardAnchor.Center
            };
        }

        entityManager.Register(entity);

        return entity;
    }
}

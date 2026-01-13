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
        var sheetName = definition.Properties["spritesheet"];
        var spriteName = definition.Properties["sprite"];
        var spriteSheet = new SpriteSheetLoader().Load($"data/spritesheets/{sheetName}.json", game);
        var sprite = spriteSheet.Get(spriteName);

        var entity = new GameObject();
        var transform = entity.Components.Add<TransformComponent>();
        transform.Local.Position = new Vector2(
            definition.TileX + 0.5f,  // Center of tile
            definition.TileY + 0.5f
        );

        var billboard = entity.Components.Add<BillboardComponent>();
        billboard.Sprite = sprite;
        entityManager.Register(entity);

        return entity;
    }
}

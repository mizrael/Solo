using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Solo;
using Solo.Assets.Loaders;
using Solo.Components;
using Solocaster.Components;
using Solocaster.Inventory;

namespace Solocaster.Entities;

public static class EntityFactory
{
    public static GameObject CreateEntity(
        EntityDefinition definition,
        Game game,
        GameObject container,
        SpatialGrid? spatialGrid = null)
    {
        return definition.Type switch
        {
            "sprite" => CreateSpriteEntity(definition, game, container, spatialGrid),
            "pickupable" => CreatePickupableEntity(definition, game, container, spatialGrid),
            _ => throw new NotSupportedException($"Entity type '{definition.Type}' not supported")
        };
    }

    private static GameObject CreateSpriteEntity(
        EntityDefinition definition,
        Game game,
        GameObject container,
        SpatialGrid? spatialGrid)
    {
        var sheetName = definition.Properties["spritesheet"] as string;
        ArgumentException.ThrowIfNullOrWhiteSpace(sheetName, nameof(definition.Properties));

        var spriteName = definition.Properties["sprite"] as string;
        ArgumentException.ThrowIfNullOrWhiteSpace(spriteName, nameof(definition.Properties));

        var spriteSheet = SpriteSheetLoader.Get(sheetName, game);
        var sprite = spriteSheet.Get(spriteName);

        var entity = new GameObject();
        var transform = entity.Components.Add<TransformComponent>();

        // Base position is center of tile
        float posX = definition.TileX + 0.5f;
        float posY = definition.TileY + 0.5f;

        // Apply optional offsets (for wall-adjacent decorations)
        if (definition.Properties.TryGetValue("offsetX", out var offsetXObj) && offsetXObj is float offsetX)
            posX += offsetX;
        if (definition.Properties.TryGetValue("offsetY", out var offsetYObj) && offsetYObj is float offsetY)
            posY += offsetY;

        transform.Local.Position = new Vector2(posX, posY);

        var billboard = new BillboardComponent(entity, new StaticFrameProvider(sprite));
        entity.Components.Add(billboard);

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

        container.AddChild(entity);
        spatialGrid?.Add(entity, transform.Local.Position);

        return entity;
    }

    private static GameObject CreatePickupableEntity(
        EntityDefinition definition,
        Game game,
        GameObject container,
        SpatialGrid? spatialGrid)
    {
        var itemTemplateId = definition.Properties["itemTemplateId"] as string;
        ArgumentException.ThrowIfNullOrWhiteSpace(itemTemplateId, nameof(definition.Properties));

        // Get the item template to get world sprite info
        var itemTemplate = ItemTemplateLoader.Get(itemTemplateId);

        var entity = new GameObject();
        var transform = entity.Components.Add<TransformComponent>();

        // Base position is center of tile
        float posX = definition.TileX + 0.5f;
        float posY = definition.TileY + 0.5f;

        // Apply optional offsets
        if (definition.Properties.TryGetValue("offsetX", out var offsetXObj) && offsetXObj is float offsetX)
            posX += offsetX;
        if (definition.Properties.TryGetValue("offsetY", out var offsetYObj) && offsetYObj is float offsetY)
            posY += offsetY;

        transform.Local.Position = new Vector2(posX, posY);

        // Create billboard from world sprite path
        if (!string.IsNullOrEmpty(itemTemplate.WorldSpritePath))
        {
            var spriteParts = itemTemplate.WorldSpritePath.Split(':');
            if (spriteParts.Length == 2)
            {
                var spriteSheet = SpriteSheetLoader.Get(spriteParts[0], game);
                var sprite = spriteSheet.Get(spriteParts[1]);

                var billboard = new BillboardComponent(entity, new StaticFrameProvider(sprite));
                entity.Components.Add(billboard);
                billboard.Scale = new Vector2(itemTemplate.WorldSpriteScale, itemTemplate.WorldSpriteScale);
                billboard.Anchor = BillboardAnchor.Bottom;
            }
        }

        // Add pickupable component
        int quantity = 1;
        if (definition.Properties.TryGetValue("quantity", out var quantityObj) && quantityObj is int qty)
            quantity = qty;

        var pickupable = new PickupableComponent(entity)
        {
            ItemTemplateId = itemTemplateId,
            Quantity = quantity,
            SpatialGrid = spatialGrid
        };
        entity.Components.Add(pickupable);

        container.AddChild(entity);
        spatialGrid?.Add(entity, transform.Local.Position);

        return entity;
    }

    public static GameObject CreatePickupableItem(
        string itemTemplateId,
        int tileX,
        int tileY,
        Game game,
        GameObject container,
        SpatialGrid? spatialGrid = null,
        int quantity = 1)
    {
        var definition = new EntityDefinition
        {
            Type = "pickupable",
            TileX = tileX,
            TileY = tileY,
            Properties = new Dictionary<string, object>
            {
                ["itemTemplateId"] = itemTemplateId,
                ["quantity"] = quantity
            }
        };

        return CreatePickupableEntity(definition, game, container, spatialGrid);
    }
}

using Microsoft.Xna.Framework;
using Solo;
using Solo.Components;
using Solocaster.Components;
using Solocaster.Monsters;

namespace Solocaster.Entities;

public static class MonsterFactory
{
    public static GameObject Create(
        MonsterTemplate template,
        Vector2 position,
        Game game,
        GameObject sceneRoot,
        SpatialGrid spatialGrid,
        GameObject player)
    {
        var monster = new GameObject();
        sceneRoot.AddChild(monster);

        var transform = monster.Components.Add<TransformComponent>();
        transform.Local.Position = position;

        var spriteProvider = DirectionalSpriteLoader.Load(template.SpritesheetBasePath, game);

        var billboard = new BillboardComponent(monster, spriteProvider);
        monster.Components.Add(billboard);
        billboard.Scale = new Vector2(template.Scale, template.Scale);
        billboard.Anchor = template.Anchor;

        var brain = monster.Components.Add<MonsterBrainComponent>();
        brain.Template = template;
        brain.Initialize(spriteProvider, player);

        spatialGrid.Add(monster, position);

        return monster;
    }
}

using Microsoft.Xna.Framework;
using Solo;
using Solo.Components;
using Solocaster.Animations;
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

        var animationController = CreateAnimationController(template, game);

        var billboard = new BillboardComponent(monster, animationController);
        monster.Components.Add(billboard);
        billboard.Scale = new Vector2(template.Scale, template.Scale);
        billboard.Anchor = template.Anchor;

        var brain = monster.Components.Add<MonsterBrainComponent>();
        brain.Template = template;
        brain.Initialize(animationController, player);

        spatialGrid.Add(monster, position);

        return monster;
    }

    private static AnimatedSpriteProvider CreateAnimationController(MonsterTemplate template, Game game)
    {
        var controller = new AnimatedSpriteProvider();

        foreach (var (state, basePath) in template.Animations)
        {
            var directionalAnim = DirectionalAnimationLoader.Load(basePath, game);
            if (directionalAnim.HasAny)
            {
                controller.AddAnimation(state, directionalAnim);
            }
        }

        controller.SetState("idle");
        return controller;
    }
}

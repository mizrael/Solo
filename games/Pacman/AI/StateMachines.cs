using Solo.AI;
using Solo;
using Solo.Components;
using Pacman.Components;
using Microsoft.Xna.Framework;

namespace Pacman.AI;

public static class StateMachines
{
    public static StateMachine Blinky(
        Game game,
        GameObject ghost,
        GameObject player,
        GameObject map,
        float startDelayMS)
    {
        var transform = ghost.Components.Get<TransformComponent>();
        var ghostBrain = ghost.Components.Get<GhostBrainComponent>();

        var idle = new Idle(ghost, startDelayMS);
        var chase = new Chase(ghost, player, map);
        var frightened = new Scared(ghost);

        var machine = new StateMachine(
            game,
        [
            idle,
            chase,
            frightened
        ]);

        machine.AddTransition(idle, chase, _ => idle.IsCompleted);
        machine.AddTransition(chase, frightened, _ => ghostBrain.IsScared);
        machine.AddTransition(frightened, chase, _ => frightened.IsCompleted);

        return machine;
    }

    public static StateMachine Inky(
        Game game,
        GameObject ghost,
        GameObject player,
        GameObject map,
        float startDelayMS,
        Scenes.PlayScene playScene)
    {
        var transform = ghost.Components.Get<TransformComponent>();

        var idle = new Idle(ghost, startDelayMS);
        var chase = new InkyIntercept(ghost, player, map, playScene);

        var machine = new StateMachine(
            game,
        [
            idle,
            chase
        ]);

        machine.AddTransition(idle, chase, _ => idle.IsCompleted);

        return machine;
    }

    public static StateMachine Pinky(
        Game game,
        GameObject owner,
        GameObject player,
        GameObject map,
        float startDelayMS)
    {
        var transform = owner.Components.Get<TransformComponent>();

        var idle = new Idle(owner, startDelayMS);
        var intercept = new Intercept(owner, player, map);

        var machine = new StateMachine(
            game,
        [
            idle,
            intercept
        ]);

        machine.AddTransition(idle, intercept, _ => idle.IsCompleted);

        return machine;
    }

    public static StateMachine Clyde(
        Game game,
        GameObject owner,
        GameObject player,
        GameObject map,
        float startDelayMS)
    {
        var transform = owner.Components.Get<TransformComponent>();
        var playerTransform = player.Components.Get<TransformComponent>();

        var mapBrain = map.Components.Get<MapLogicComponent>();

        var idle = new Idle(owner, startDelayMS);
        var chase = new Chase(owner, player, map);
        var arrive = new Arrive(owner, mapBrain.GetGhostScatterTile(GhostTypes.Clyde), map);

        var machine = new StateMachine(
            game,
        [
            idle,
            chase,
            arrive
        ]);

        machine.AddTransition(idle, chase, _ => idle.IsCompleted);

        var escapeThreshold = mapBrain.TileSize.LengthSquared() * 4f;
        var chaseThreshold = mapBrain.TileSize.LengthSquared() * 32;
        machine.AddTransition(chase, arrive, _ =>
        {
            var dist = Vector2.DistanceSquared(transform.World.Position, playerTransform.World.Position);
            return dist < escapeThreshold;
        });
        machine.AddTransition(arrive, chase, _ =>
        {
            var dist = Vector2.DistanceSquared(transform.World.Position, playerTransform.World.Position);
            return dist > chaseThreshold;
        });

        return machine;
    }
}
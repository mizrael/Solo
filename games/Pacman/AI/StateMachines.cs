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
        var mapBrain = map.Components.Get<MapLogicComponent>();

        var idle = new Idle(ghost, startDelayMS);
        var chase = new Chase(ghost, player, map);
        var scatter = new Arrive(ghost, mapBrain.GetGhostScatterTile(GhostTypes.Blinky), map);
        var scared = new Scared(ghost, map);

        var machine = new StateMachine(game);

        machine.AddTransition(idle, chase, _ => idle.IsCompleted);
        machine.AddTransition(chase, scatter, _ => chase.ElapsedMilliseconds > 20000f);
        machine.AddTransition(scatter, chase, _ => scatter.ElapsedMilliseconds > 5000f);
        machine.AddTransition(chase, scared, _ => ghostBrain.IsScared);
        machine.AddTransition(scared, chase, _ => scared.IsCompleted);

        machine.SetState(idle);

        return machine;
    }

    public static StateMachine Inky(
        Game game,
        GameObject owner,
        GameObject player,
        GameObject map,
        float startDelayMS,
        Scenes.PlayScene playScene)
    {
        var transform = owner.Components.Get<TransformComponent>();

        var idle = new Idle(owner, startDelayMS);
        var chase = new InkyIntercept(owner, player, map, playScene);

        var machine = new StateMachine(game);

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

        var machine = new StateMachine(game);

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

        var machine = new StateMachine(game);

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
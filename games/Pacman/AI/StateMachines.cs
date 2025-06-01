using Microsoft.Xna.Framework;
using Pacman.Components;
using Solo;
using Solo.AI;
using Solo.Components;

namespace Pacman.AI;

public static class StateMachines
{
    private const float IdleDuration = 2000f;
    private const float ChaseDuration = 20000f;
    private const float ScatterDuration = 5000f;
    private const float ScaredDuration = 10000f;

    public static StateMachine Blinky(
        GameObject ghost,
        GameObject player,
        GameObject map)
    {
        var transform = ghost.Components.Get<TransformComponent>(); 
        var ghostBrain = ghost.Components.Get<GhostBrainComponent>();
        var mapBrain = map.Components.Get<MapLogicComponent>();

        var idle = new Idle(ghost, map, IdleDuration);
        var chase = new Chase(ghost, player, map);
        var scatter = new Arrive(ghost, mapBrain.GetGhostScatterTile(GhostTypes.Blinky), map);
        var scared = new Scared(ghost, map, ScaredDuration);

        var machine = new StateMachine(idle);

        machine.AddTransition(idle, chase, _ => idle.IsCompleted);
        machine.AddTransition(chase, scatter, _ => chase.ElapsedMilliseconds > ChaseDuration);
        machine.AddTransition(scatter, chase, _ => scatter.ElapsedMilliseconds > ScatterDuration);
        machine.AddTransition(chase, scared, _ => ghostBrain.State == GhostStates.Scared);
        machine.AddTransition(scared, idle, _ => ghostBrain.State == GhostStates.Eaten);
        machine.AddTransition(scared, chase, s => s.IsCompleted);

        return machine;
    }

    public static StateMachine Inky(
        GameObject ghost,
        GameObject player,
        GameObject map,
        Scenes.PlayScene playScene)
    {
        var transform = ghost.Components.Get<TransformComponent>();
        var ghostBrain = ghost.Components.Get<GhostBrainComponent>();

        var mapBrain = map.Components.Get<MapLogicComponent>();

        var idle = new Idle(ghost, map, IdleDuration);
        var chase = new InkyIntercept(ghost, player, map, playScene);
        var scatter = new Arrive(ghost, mapBrain.GetGhostScatterTile(GhostTypes.Inky), map);
        var scared = new Scared(ghost, map, ScaredDuration);

        var machine = new StateMachine(idle);

        machine.AddTransition(idle, chase, _ => idle.IsCompleted);
        machine.AddTransition(chase, scatter, _ => chase.ElapsedMilliseconds > ChaseDuration);
        machine.AddTransition(scatter, chase, _ => scatter.ElapsedMilliseconds > ScatterDuration);
        machine.AddTransition(chase, scared, _ => ghostBrain.State == GhostStates.Scared);
        machine.AddTransition(scared, idle, _ => ghostBrain.State == GhostStates.Eaten);
        machine.AddTransition(scared, chase, s => s.IsCompleted);

        return machine;
    }

    public static StateMachine Pinky(
        GameObject ghost,
        GameObject player,
        GameObject map)
    {
        var transform = ghost.Components.Get<TransformComponent>();
        var ghostBrain = ghost.Components.Get<GhostBrainComponent>();

        var mapBrain = map.Components.Get<MapLogicComponent>();

        var idle = new Idle(ghost, map, IdleDuration);
        var intercept = new Intercept(ghost, player, map);
        var scatter = new Arrive(ghost, mapBrain.GetGhostScatterTile(GhostTypes.Pinky), map);
        var scared = new Scared(ghost, map, ScaredDuration);

        var machine = new StateMachine(idle);

        machine.AddTransition(idle, intercept, _ => idle.IsCompleted);
        machine.AddTransition(intercept, scatter, _ => intercept.ElapsedMilliseconds > ChaseDuration);
        machine.AddTransition(scatter, intercept, _ => scatter.ElapsedMilliseconds > ScatterDuration);
        machine.AddTransition(intercept, scared, _ => ghostBrain.State == GhostStates.Scared);
        machine.AddTransition(scared, idle, _ => ghostBrain.State == GhostStates.Eaten);
        machine.AddTransition(scared, intercept, s => s.IsCompleted);

        return machine;
    }

    public static StateMachine Clyde(
        GameObject ghost,
        GameObject player,
        GameObject map)
    {
        var transform = ghost.Components.Get<TransformComponent>();
        var playerTransform = player.Components.Get<TransformComponent>();
        var ghostBrain = ghost.Components.Get<GhostBrainComponent>();

        var mapBrain = map.Components.Get<MapLogicComponent>();

        var idle = new Idle(ghost, map, IdleDuration);
        var chase = new Chase(ghost, player, map);
        var arrive = new Arrive(ghost, mapBrain.GetGhostScatterTile(GhostTypes.Clyde), map);
        var scatter = new Arrive(ghost, mapBrain.GetGhostScatterTile(GhostTypes.Clyde), map);
        var scared = new Scared(ghost, map, ScaredDuration);

        var machine = new StateMachine(idle);

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

        machine.AddTransition(chase, scatter, _ => chase.ElapsedMilliseconds > ChaseDuration);
        machine.AddTransition(scatter, chase, _ => scatter.ElapsedMilliseconds > ScatterDuration);
        machine.AddTransition(chase, scared, _ => ghostBrain.State == GhostStates.Scared);
        machine.AddTransition(scared, idle, _ => ghostBrain.State == GhostStates.Eaten);
        machine.AddTransition(scared, chase, s => s.IsCompleted);

        return machine;
    }
}
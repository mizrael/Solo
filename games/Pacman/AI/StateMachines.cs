using Solo.AI;
using Solo;
using Solo.Components;

namespace Pacman.AI;

public static class StateMachines
{
    public static StateMachine Blinky(
        GameObject owner,
        GameObject target,
        GameObject map,
        float startDelayMS)
    {
        var transform = owner.Components.Get<TransformComponent>();
    
        var idle = new Idle(owner, startDelayMS);
        var chase = new Chase(owner, target, map);

        var machine = new StateMachine(
        [
            idle,
            chase
        ]);

        machine.AddTransition(idle, chase, _ => idle.IsCompleted);

        return machine;
    }

    public static StateMachine Inky(
        GameObject owner,
        GameObject target,
        GameObject map,
        float startDelayMS)
    {
        var transform = owner.Components.Get<TransformComponent>();

        var idle = new Idle(owner, startDelayMS);
        var intercept = new Intercept(owner, target, map, 16f);

        var machine = new StateMachine(
        [
            idle,
            intercept
        ]);

        machine.AddTransition(idle, intercept, _ => idle.IsCompleted);

        return machine;
    }
}
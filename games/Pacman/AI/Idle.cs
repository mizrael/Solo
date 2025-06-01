using Microsoft.Xna.Framework;
using Pacman.Components;
using Solo;
using Solo.AI;
using System;

namespace Pacman.AI;

public record Idle : State
{
    private readonly float _durationMs;
    private readonly bool _hasDuration = false;

    public Idle(GameObject owner) : this(owner, 0f) { }

    public Idle(GameObject owner, float durationMs) : base(owner)
    {
        _durationMs = Math.Abs(durationMs);
        _hasDuration = _durationMs > 0f;
    }

    protected override void OnEnter()
    {
        var brain = this.Owner.Components.Get<GhostBrainComponent>();
        brain.SetAnimation(GhostAnimations.Walk);
    }

    protected override void OnExecute(GameTime gameTime)
    {
        if (_hasDuration && ElapsedMilliseconds > _durationMs)
        {
            IsCompleted = true;
            return;
        }
        base.OnExecute(gameTime);
    }
}

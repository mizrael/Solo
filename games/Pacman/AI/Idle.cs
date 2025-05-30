using Solo.AI;
using Solo;
using System;
using Microsoft.Xna.Framework;
using Solo.Assets.Loaders;
using Pacman.Components;
using Solo.Components;

namespace Pacman.AI;

public record Idle : State
{
    private float _durationMs;
    private bool _hasDuration = false;

    public Idle(GameObject owner) : this(owner, 0f) { }

    public Idle(GameObject owner, float durationMs) : base(owner)
    {
        SetDuration(durationMs);
    }

    protected override void OnEnter(Game game)
    {
        this.Owner.Components.Get<GhostBrainComponent>().SetAnimation(GhostAnimations.Walk, game);
    }

    protected override void OnExecute(Game game, GameTime gameTime)
    {
        if (_hasDuration && ElapsedMilliseconds > _durationMs)
        {
            IsCompleted = true;
            return;
        }
        base.OnExecute(game, gameTime);
    }

    public void SetDuration(float milliseconds)
    {
        _durationMs = Math.Abs(milliseconds);
        _hasDuration = _durationMs > 0f;
    }
}

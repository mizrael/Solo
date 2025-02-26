using Solo.AI;
using Solo;
using System;
using Microsoft.Xna.Framework;

namespace Pacman.AI;

public record Idle : State
{
    private float _duration;
    private bool _hasDuration = false;

    public Idle(GameObject owner) : this(owner, 0f) { }

    public Idle(GameObject owner, float duration) : base(owner)
    {
        SetDuration(duration);
    }

    protected override void OnExecute(GameTime gameTime)
    {
        if (_hasDuration && ElapsedMilliseconds > _duration)
        {
            IsCompleted = true;
            return;
        }
        base.OnExecute(gameTime);
    }

    public void SetDuration(float milliseconds)
    {
        _duration = Math.Abs(milliseconds);
        _hasDuration = _duration > 0f;
    }
}

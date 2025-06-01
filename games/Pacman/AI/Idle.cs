using Microsoft.Xna.Framework;
using Pacman.Components;
using Solo;
using Solo.AI;
using Solo.Components;
using System;

namespace Pacman.AI;

public record Idle : State
{
    private readonly float _durationMs;
    private readonly bool _hasDuration = false;
    private readonly GameObject _map;

    public Idle(GameObject owner, GameObject map, float durationMs) : base(owner)
    {
        _durationMs = Math.Abs(durationMs);
        _hasDuration = _durationMs > 0f;
        _map = map;
    }

    protected override void OnEnter()
    {
        var brain = this.Owner.Components.Get<GhostBrainComponent>();
        brain.SetAnimation(GhostAnimations.Walk);

        var mapLogic = _map.Components.Get<MapLogicComponent>();
        var ghostStartTile = mapLogic.GetGhostStartTile(brain.GhostType);

        var ghostTransform = this.Owner.Components.Get<TransformComponent>();
        ghostTransform.Local.Position = mapLogic.GetTileCenter(ghostStartTile);
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

    protected override void OnExit()
    {
        var brain = this.Owner.Components.Get<GhostBrainComponent>();
        brain.State = GhostStates.Normal;

        base.OnExit();
    }
}

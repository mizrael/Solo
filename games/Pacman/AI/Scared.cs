using Microsoft.Xna.Framework;
using Pacman.Components;
using Solo;
using Solo.AI;

namespace Pacman.AI;

public record Scared : State
{
    private readonly float _durationMs;
    private readonly float _threshold;
    private bool _isAlmostDone = false;

    public Scared(GameObject owner, float durationMs = 1000 * 10) : base(owner)
    {
        _durationMs = durationMs;
        _threshold = durationMs * 0.75f;
    }

    protected override void OnEnter(Game game)
    {
        this.Owner.Components.Get<GhostBrainComponent>().SetAnimation(GhostAnimations.Scared1, game);
    }

    protected override void OnExecute(Game game, GameTime gameTime)
    {
        if (ElapsedMilliseconds > _durationMs)
        {
            IsCompleted = true;
            return;
        }
        else if (ElapsedMilliseconds > _threshold && !_isAlmostDone)
        {
            _isAlmostDone = true;
            this.Owner.Components.Get<GhostBrainComponent>().SetAnimation(GhostAnimations.Scared2, game);
        }

        base.OnExecute(game, gameTime);
    }

    protected override void OnExit(Game game)
    {
        var brain = Owner.Components.Get<GhostBrainComponent>();
        brain.IsScared = false;

        base.OnExit(game);
    }
}
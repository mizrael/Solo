using Microsoft.Xna.Framework;
using Solo;

namespace Solocaster.AI.Player;

public record PlayerExploringState : Solo.AI.State
{
    private readonly PlayerStateContext _ctx;

    public PlayerExploringState(GameObject owner, PlayerStateContext context) : base(owner)
    {
        _ctx = context;
    }

    protected override void OnEnter()
    {
        _ctx.LeftHandRaiseAmount = 0f;
        _ctx.RightHandRaiseAmount = 0f;
        _ctx.SpeedMultiplier = 1.0f;
    }

    protected override void OnExecute(GameTime gameTime)
    {
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _ctx.Stats.UpdateStamina(deltaTime, false);
    }
}

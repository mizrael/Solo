using Microsoft.Xna.Framework;
using Solo;

namespace Solocaster.AI.Player;

public record PlayerRunningState : Solo.AI.State
{
    private readonly PlayerStateContext _ctx;

    public PlayerRunningState(GameObject owner, PlayerStateContext context) : base(owner)
    {
        _ctx = context;
    }

    protected override void OnEnter()
    {
        _ctx.ShowsHands = true;
        _ctx.SpeedMultiplier = 1.8f;
        _ctx.BobSpeed = 3.0f;
        _ctx.LeftHandRaiseAmount = 0f;
        _ctx.RightHandRaiseAmount = 0f;
    }

    protected override void OnExecute(GameTime gameTime)
    {
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _ctx.Stats.DrainStamina(deltaTime);
    }
}

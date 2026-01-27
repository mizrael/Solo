using Microsoft.Xna.Framework;
using Solo;

namespace Solocaster.AI.Player;

public record PlayerRunningState : Solo.AI.State
{
    private const float RunningSpeedMultiplier = 1.8f;

    private readonly PlayerStateContext _ctx;

    public PlayerRunningState(GameObject owner, PlayerStateContext context) : base(owner)
    {
        _ctx = context;
    }

    protected override void OnEnter()
    {
        _ctx.LeftHandRaiseAmount = 0f;
        _ctx.RightHandRaiseAmount = 0f;
        _ctx.SpeedMultiplier = RunningSpeedMultiplier;
    }

    protected override void OnExecute(GameTime gameTime)
    {
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _ctx.Stats.DrainStamina(deltaTime);
    }
}

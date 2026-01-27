using Microsoft.Xna.Framework;
using Solo;

namespace Solocaster.AI.Player;

public record PlayerExhaustedState : Solo.AI.State
{
    private const float ExhaustedSpeedMultiplier = 0.6f;

    private readonly PlayerStateContext _ctx;

    public PlayerExhaustedState(GameObject owner, PlayerStateContext context) : base(owner)
    {
        _ctx = context;
    }

    protected override void OnEnter()
    {
        _ctx.LeftHandRaiseAmount = 0f;
        _ctx.RightHandRaiseAmount = 0f;
        _ctx.SpeedMultiplier = ExhaustedSpeedMultiplier;
    }

    protected override void OnExecute(GameTime gameTime)
    {
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _ctx.Stats.UpdateStamina(deltaTime, false);
    }
}

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Solo;
using Solo.Components;
using Solo.Services;

namespace SpaceInvaders.Logic;

public class PlayerBrain : Component
{
    private RenderService _renderService;
    private TransformComponent _transform;
    private float _velocity = 0f;

    public PlayerBrain(GameObject owner) : base(owner)
    {
    }

    protected override void InitCore()
    {
        _transform = Owner.Components.Get<TransformComponent>();
        _renderService = GameServicesManager.Instance.GetService<RenderService>();
        base.InitCore();
    }

    protected override void UpdateCore(GameTime gameTime)
    {
        var keyboard = Keyboard.GetState();

        if (_transform.World.Position.X < 0)
            _transform.Local.Position.X = _renderService.Graphics.GraphicsDevice.Viewport.Width;
        else if (_transform.World.Position.X >= _renderService.Graphics.GraphicsDevice.Viewport.Width)
            _transform.Local.Position.X = 0;

        var thrust = 0f;
        if (keyboard.IsKeyDown(Keys.Left))
            thrust = -EnginePower;
        else if (keyboard.IsKeyDown(Keys.Right))
            thrust = EnginePower;

        var dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

        var acceleration = thrust / Mass;
        _velocity += acceleration * dt;
        _velocity *= (1f - dt * Drag);

        _transform.Local.Position.X += _velocity * dt;

        base.UpdateCore(gameTime);
    }

    public float Thrust = 0f;
    public float Drag = 5f;
    public float Mass = 1f;
    public float EnginePower = 2000f;
}
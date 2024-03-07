using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Monoroids.Core;
using Monoroids.Core.Components;
using Monoroids.Core.Services;

namespace Monoroids.GameStuff.Components;

public class PlayerBrain : Component
{
    private MovingBody _movingBody;
    private TransformComponent _transform;
    private SpriteRenderComponent _spriteRender;
    private RenderService _renderService;

    private Weapon _weapon;

    public readonly PlayerStats Stats = PlayerStats.Default();

    public PlayerBrain(GameObject owner) : base(owner)
    {
    }

    protected override void InitCore()
    {
        _movingBody = Owner.Components.Get<MovingBody>();
        _transform = Owner.Components.Get<TransformComponent>();
        _spriteRender = Owner.Components.Get<SpriteRenderComponent>();
        _weapon = Owner.Components.Get<Weapon>();

        _renderService = GameServicesManager.Instance.GetService<RenderService>();
    }

    public event OnDeathHandler OnDeath;
    public delegate void OnDeathHandler(GameObject player);

    protected override void UpdateCore(GameTime gameTime)
    {
        var keyboard = Keyboard.GetState();

        HandleMovement(keyboard);

        var isShooting = keyboard.IsKeyDown(Keys.Space);
        if (isShooting)
            _weapon.Shoot(gameTime);
    }

    private void HandleMovement(KeyboardState keyboard)
    {
        if (_transform.World.Position.X < -_spriteRender.Sprite.Bounds.Width)
            _transform.Local.Position.X = _renderService.Graphics.PreferredBackBufferWidth + _spriteRender.Sprite.Center.X;
        else if (_transform.World.Position.X > _renderService.Graphics.PreferredBackBufferWidth + _spriteRender.Sprite.Bounds.Width)
            _transform.Local.Position.X = -_spriteRender.Sprite.Center.X;

        if (_transform.World.Position.Y < -_spriteRender.Sprite.Bounds.Height)
            _transform.Local.Position.Y = _renderService.Graphics.PreferredBackBufferHeight + _spriteRender.Sprite.Center.Y;
        else if (_transform.World.Position.Y > _renderService.Graphics.PreferredBackBufferHeight + _spriteRender.Sprite.Bounds.Height)
            _transform.Local.Position.Y = -_spriteRender.Sprite.Center.Y;

        if (keyboard.IsKeyDown(Keys.Right))
            _movingBody.RotationSpeed = Stats.RotationSpeed;
        else if (keyboard.IsKeyDown(Keys.Left))
            _movingBody.RotationSpeed = -Stats.RotationSpeed;
        else
            _movingBody.RotationSpeed = 0f;

        if (keyboard.IsKeyDown(Keys.Up))
            _movingBody.Thrust = Stats.EnginePower;
        else if (keyboard.IsKeyDown(Keys.Down))
            _movingBody.Thrust = -Stats.EnginePower;
        else
            _movingBody.Thrust = 0f;
    }
}

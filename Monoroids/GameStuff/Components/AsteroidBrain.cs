using Microsoft.Xna.Framework;
using Monoroids.Core;
using Monoroids.Core.Components;
using Monoroids.Core.Services;
using System;

namespace Monoroids.GameStuff.Components;

public class AsteroidBrain : Component
{
    private TransformComponent _transform;
    private BoundingBoxComponent _boundingBox;
    private RenderService _renderService;

    public float RotationSpeed = Random.Shared.NextFloat(-0.005f, 0.005f);
    public Microsoft.Xna.Framework.Vector2 Direction;
    public float Speed = Random.Shared.NextFloat(0.15f, 0.5f);
    
    public event OnDeathHandler OnDeath;
    public delegate void OnDeathHandler(GameObject asteroid, bool HasCollidedWithPlayer);

    private AsteroidBrain(GameObject owner) : base(owner)
    {
    }

    protected override void InitCore()
    {
        _transform = Owner.Components.Get<TransformComponent>();
        _boundingBox = Owner.Components.Get<BoundingBoxComponent>();
        _boundingBox.OnCollision += (sender, collidedWith) =>
        {
            if (!collidedWith.Owner.Components.TryGet<PlayerBrain>(out var playerBrain) &&
                !collidedWith.Owner.Components.TryGet<BulletBrain>(out var _))
                return;

            this.Owner.Enabled = false;
            this.OnDeath?.Invoke(this.Owner, playerBrain is not null);
        };

        _renderService = GameServicesManager.Instance.GetService<RenderService>();
    }

    protected override void UpdateCore(GameTime gameTime)
    {
        var dt = (float)gameTime.ElapsedGameTime.TotalMilliseconds;
        _transform.Local.Rotation += RotationSpeed * dt;
        _transform.Local.Position += Direction * Speed * dt;

        var isOutScreen = _transform.World.Position.X < 0 ||
                          _transform.World.Position.Y < 0 ||
                          _transform.World.Position.X > _renderService.Graphics.PreferredBackBufferWidth ||
                          _transform.World.Position.Y > _renderService.Graphics.PreferredBackBufferHeight;
        if (isOutScreen)
            this.Owner.Enabled = false;
    }
}
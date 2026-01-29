using Microsoft.Xna.Framework;
using Solo;
using Solo.Components;
using Solo.Services;
using Solo.Utils;
using System;

namespace Monoroids.Components;

public class AsteroidBrain : Component
{
    private TransformComponent _transform;
    private BoundingBoxComponent _boundingBox;

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
        _boundingBox.OnCollision += (collidedWith) =>
        {
            var hasPlayerBrain = collidedWith.Owner.Components.Has<PlayerBrain>();
            if (!hasPlayerBrain &&
                !collidedWith.Owner.Components.Has<BulletBrain>())
                return;

            this.Owner.Enabled = false;
            this.OnDeath?.Invoke(this.Owner, hasPlayerBrain);
        };
    }

    protected override void UpdateCore(GameTime gameTime)
    {
        var dt = (float)gameTime.ElapsedGameTime.TotalMilliseconds;
        _transform.Local.Rotation += RotationSpeed * dt;
        _transform.Local.Position += Direction * Speed * dt;

        var viewport = GraphicsDeviceManagerAccessor.Instance.GraphicsDeviceManager.GraphicsDevice.Viewport;
        var isOutScreen = _transform.World.Position.X < 0 ||
                          _transform.World.Position.Y < 0 ||
                          _transform.World.Position.X > viewport.Width ||
                          _transform.World.Position.Y > viewport.Height;
        if (isOutScreen)
            this.Owner.Enabled = false;
    }
}

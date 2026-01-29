using Microsoft.Xna.Framework;
using Solo;
using Solo.Components;
using Solo.Services;

public class BulletBrain : Component
{
    private MovingBody _movingBody;
    private TransformComponent _transformComponent;
    private BoundingBoxComponent _boundingBox;

    public BulletBrain(GameObject owner) : base(owner)
    {

    }

    protected override void InitCore()
    {
        _movingBody = Owner.Components.Get<MovingBody>();
        _transformComponent = Owner.Components.Get<TransformComponent>();
        _boundingBox = Owner.Components.Get<BoundingBoxComponent>();
        _boundingBox.OnCollision += (collidedWith) =>
        {
            //if (collidedWith.Owner.Components.TryGet<AsteroidBrain>(out var _))
            //    this.Owner.Enabled = false;
        };
    }

    protected override void UpdateCore(GameTime gameTime)
    {
        _movingBody.Thrust = this.Speed;

        var viewport = GraphicsDeviceManagerAccessor.Instance.GraphicsDeviceManager.GraphicsDevice.Viewport;
        var isOutScreen = _transformComponent.World.Position.X < 0 ||
                          _transformComponent.World.Position.Y < 0 ||
                          _transformComponent.World.Position.X > viewport.Width ||
                          _transformComponent.World.Position.Y > viewport.Height;
        if (isOutScreen)
            this.Owner.Enabled = false;
    }

    public float Speed { get; set; }
}

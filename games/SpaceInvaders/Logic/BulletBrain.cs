using Microsoft.Xna.Framework;
using Solo;
using Solo.Components;
using Solo.Services;

namespace SpaceInvaders.Logic;

public class BulletBrain : Component
{
    private MovingBody _movingBody;
    private TransformComponent _transformComponent;
    private BoundingBoxComponent _boundingBox;
    private RenderService _renderService;

    public BulletBrain(GameObject owner) : base(owner)
    {

    }

    protected override void InitCore()
    {
        _movingBody = Owner.Components.Get<MovingBody>();
        _transformComponent = Owner.Components.Get<TransformComponent>();
        _boundingBox = Owner.Components.Get<BoundingBoxComponent>();
        _boundingBox.OnCollision += (sender, collidedWith) =>
        {
            this.Owner.Enabled = false;
        };
        _renderService = GameServicesManager.Instance.GetService<RenderService>();
    }

    protected override void UpdateCore(GameTime gameTime)
    {
        _movingBody.Thrust = this.Speed;

        var isOutScreen = _transformComponent.World.Position.X < 0 ||
                          _transformComponent.World.Position.Y < 0 ||
                          _transformComponent.World.Position.X > _renderService.Graphics.GraphicsDevice.Viewport.Width ||
                          _transformComponent.World.Position.Y > _renderService.Graphics.GraphicsDevice.Viewport.Height;
        if (isOutScreen)
            this.Owner.Enabled = false;
    }

    public float Speed { get; set; }
}
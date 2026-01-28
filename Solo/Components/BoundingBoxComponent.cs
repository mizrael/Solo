using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Solo.Services;

namespace Solo.Components;

public class BoundingBoxComponent : Component
#if DEBUG
  //  , Solo.Services.IRenderable
#endif
{
    private TransformComponent _transform;
    private Rectangle _bounds;
    private Vector2 _halfSize;
    private readonly int _hashCode;

#if DEBUG
    private Texture2D _pixelTexture;
#endif

    private BoundingBoxComponent(GameObject owner) : base(owner)
    {
        _hashCode = System.HashCode.Combine(owner.GetHashCode(), nameof(BoundingBoxComponent));
    }

    protected override void InitCore()
    {
        _transform = Owner.Components.Get<TransformComponent>();
    }

    public Microsoft.Xna.Framework.Rectangle Bounds => _bounds;

    public void SetSize(Point size)
    {
        _bounds.Size = size;
        _halfSize = new Vector2(size.X / 2, size.Y / 2);
    }

    protected override void UpdateCore(GameTime gameTime)
    {
        var x = (int)(_transform.World.Position.X - _halfSize.X);
        var y = (int)(_transform.World.Position.Y - _halfSize.Y);

        var changed = _bounds.X != x || _bounds.Y != y;
        _bounds.X = x;
        _bounds.Y = y;
        if (changed)
            OnPositionChanged?.Invoke(this);
    }

#if DEBUG
    public void Render(SpriteBatch spriteBatch)
    {
        if (_pixelTexture is null)
            _pixelTexture = Texture2DUtils.Generate(GraphicsDeviceManagerAccessor.Instance.GraphicsDeviceManager.GraphicsDevice, 1, 1, Color.White);

        spriteBatch.Draw(_pixelTexture, new Rectangle(_bounds.X, _bounds.Y, _bounds.Width, 1), Color.Yellow);
        spriteBatch.Draw(_pixelTexture, new Rectangle(_bounds.X, _bounds.Y, 1, _bounds.Height), Color.Yellow);
        spriteBatch.Draw(_pixelTexture, new Rectangle(_bounds.X + _bounds.Width - 1, _bounds.Y, 1, _bounds.Height), Color.Yellow);
        spriteBatch.Draw(_pixelTexture, new Rectangle(_bounds.X, _bounds.Y + _bounds.Height - 1, _bounds.Width, 1), Color.Yellow);
    }

    public bool Hidden { get; set; }
    public int LayerIndex { get; set; }
#endif

    public event OnPositionChangedHandler OnPositionChanged;
    public delegate void OnPositionChangedHandler(BoundingBoxComponent sender);

    public void CollideWith(BoundingBoxComponent other) => this.OnCollision?.Invoke(other);

    public event OnCollisionHandler OnCollision;
    public delegate void OnCollisionHandler(BoundingBoxComponent otherBBox);

    public override int GetHashCode()
        => _hashCode;
}
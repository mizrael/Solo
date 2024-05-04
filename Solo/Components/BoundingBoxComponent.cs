using Microsoft.Xna.Framework;

namespace Monoroids.Core.Components;

public class BoundingBoxComponent : Component
#if DEBUG
//    , IRenderable
#endif
{
    private TransformComponent _transform;
    private Rectangle _bounds;
    private Vector2 _halfSize;
    private readonly int _hashCode;

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
    //public void Render()
    //{
    //    var tmpW = context.LineWidth;
    //    var tmpS = context.StrokeStyle;

    //    context.BeginPath();

    //    context.StrokeStyle = "rgb(255,255,0)";
    //    context.LineWidth = 3;

    //    context.StrokeRect(_bounds.X, _bounds.Y,
    //        _bounds.Width,
    //        _bounds.Height);

    //    context.StrokeStyle = tmpS;
    //    context.LineWidth = tmpW;
    //}
#endif

    public event OnPositionChangedHandler OnPositionChanged;
    public delegate void OnPositionChangedHandler(BoundingBoxComponent sender);

    public void CollideWith(BoundingBoxComponent other) => this.OnCollision?.Invoke(this, other);

    public event OnCollisionHandler OnCollision;
    public delegate void OnCollisionHandler(BoundingBoxComponent sender, BoundingBoxComponent collidedWith);

    public override int GetHashCode()
        => _hashCode;
}
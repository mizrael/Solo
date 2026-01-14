using Microsoft.Xna.Framework;

namespace Solo;

public class Transform
{
    public Vector2 Position;

    private Vector2 _direction;
    public Vector2 Direction
    {
        get => _direction;
        set
        {
            _direction = value;
            _rotation = MathF.Atan2(_direction.X, -_direction.Y);
        }
    }

    private float _rotation;
    public float Rotation
    {
        get => _rotation;
        set
        {
            _rotation = value;
            _direction = new Vector2(MathF.Sin(_rotation), -MathF.Cos(_rotation));
        }
    }

    public Vector2 Scale;

    public void Clone(Transform source)
    {
        this.Position = source.Position;
        this.Scale = source.Scale;

        _direction = source._direction;
        _rotation = source._rotation;
    }

    public void Reset()
    {
        Position = Vector2.Zero;
        Scale = Vector2.One;
        _direction = Vector2.One;
        _rotation = 0f;
    }

    public static Transform Identity() => new Transform()
    {
        Position = Vector2.Zero,
        Scale = Vector2.One,
        Rotation = 0f
    };
}

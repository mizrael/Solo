using Microsoft.Xna.Framework;

namespace Solo.UI;

public sealed class DragDropManager<T> where T : class
{
    private readonly float _dragThresholdSquared;
    private Vector2 _pressPosition;

    public DragDropManager(float dragThreshold = 6f)
    {
        _dragThresholdSquared = dragThreshold * dragThreshold;
    }

    public T? Payload { get; private set; }
    public object? Source { get; private set; }
    public bool IsDragging { get; private set; }
    public Vector2 Position { get; private set; }

    public void BeginPress(T payload, object source, Vector2 at)
    {
        ArgumentNullException.ThrowIfNull(payload);
        ArgumentNullException.ThrowIfNull(source);

        Payload = payload;
        Source = source;
        IsDragging = false;
        Position = at;
        _pressPosition = at;
    }

    public void Move(Vector2 to)
    {
        if (Payload is null)
            return;

        Position = to;
        if (Vector2.DistanceSquared(_pressPosition, to) >= _dragThresholdSquared)
            IsDragging = true;
    }

    public DropResult<T>? Release(object? target)
    {
        if (Payload is null || Source is null || !IsDragging)
        {
            Cancel();
            return null;
        }

        var result = new DropResult<T>(Payload, Source, target);
        Cancel();
        return result;
    }

    public void Cancel()
    {
        Payload = null;
        Source = null;
        IsDragging = false;
        Position = Vector2.Zero;
        _pressPosition = Vector2.Zero;
    }
}

public readonly record struct DropResult<T>(T Payload, object Source, object? Target);

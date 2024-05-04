using Microsoft.Xna.Framework;
using Solo.Components;

namespace Solo.Services;

internal class CollisionBucket
{
    private readonly HashSet<BoundingBoxComponent> _colliders = new();
    private readonly HashSet<BoundingBoxComponent> _collidersToRemove = new();
    private readonly Queue<BoundingBoxComponent> _collidersToAdd = new();

    public CollisionBucket(Rectangle bounds)
    {
        Bounds = bounds;
    }

    private void ApplyPendingChanges()
    {
        foreach(var collider in _collidersToRemove)
        {
            _colliders.Remove(collider);
        }
        _collidersToRemove.Clear();

        while (_collidersToAdd.Count > 0)
        {
            var collider = _collidersToAdd.Dequeue();
            _colliders.Add(collider);
        }
    }

    public void Add(BoundingBoxComponent bbox) => _collidersToAdd.Enqueue(bbox);

    public void Remove(BoundingBoxComponent bbox) => _collidersToRemove.Remove(bbox);

    public void CheckCollisions(BoundingBoxComponent bbox)
    {
        ApplyPendingChanges();

        foreach (var collider in _colliders)
        {
            if (collider.Owner == bbox.Owner ||
               !collider.Owner.Enabled ||
               !bbox.Bounds.Intersects(collider.Bounds))
                continue;

            collider.CollideWith(bbox);
            bbox.CollideWith(collider);
        }
    }

    public Rectangle Bounds { get; }
}


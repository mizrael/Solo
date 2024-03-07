using Microsoft.Xna.Framework;
using Monoroids.Core.Components;
using System.Collections.Generic;

namespace Monoroids.Core.Services;

internal class CollisionBucket
{
    private readonly HashSet<BoundingBoxComponent> _colliders = new();

    public CollisionBucket(Rectangle bounds)
    {
        Bounds = bounds;
    }

    public Rectangle Bounds { get; }

    public void Add(BoundingBoxComponent bbox) => _colliders.Add(bbox);

    public void Remove(BoundingBoxComponent bbox) => _colliders.Remove(bbox);

    public void CheckCollisions(BoundingBoxComponent bbox)
    {
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
}


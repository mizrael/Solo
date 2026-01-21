using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Solo;

namespace Solocaster;

public class SpatialGrid
{
    private readonly float _bucketSize;
    private readonly Dictionary<(int, int), List<GameObject>> _buckets = new();
    private readonly Dictionary<GameObject, (int, int)> _entityBuckets = new();

    public SpatialGrid(float bucketSize = 1f)
    {
        _bucketSize = bucketSize;
    }

    private (int, int) GetBucketKey(Vector2 position)
    {
        int x = (int)(position.X / _bucketSize);
        int y = (int)(position.Y / _bucketSize);
        return (x, y);
    }

    public void Add(GameObject entity, Vector2 position)
    {
        var key = GetBucketKey(position);

        if (!_buckets.TryGetValue(key, out var bucket))
        {
            bucket = new List<GameObject>();
            _buckets[key] = bucket;
        }

        bucket.Add(entity);
        _entityBuckets[entity] = key;
    }

    public void Remove(GameObject entity)
    {
        if (!_entityBuckets.TryGetValue(entity, out var key))
            return;

        if (_buckets.TryGetValue(key, out var bucket))
        {
            bucket.Remove(entity);
            if (bucket.Count == 0)
                _buckets.Remove(key);
        }

        _entityBuckets.Remove(entity);
    }

    public void UpdatePosition(GameObject entity, Vector2 newPosition)
    {
        if (!_entityBuckets.TryGetValue(entity, out var oldKey))
        {
            Add(entity, newPosition);
            return;
        }

        var newKey = GetBucketKey(newPosition);
        if (oldKey == newKey)
            return;

        if (_buckets.TryGetValue(oldKey, out var oldBucket))
        {
            oldBucket.Remove(entity);
            if (oldBucket.Count == 0)
                _buckets.Remove(oldKey);
        }

        if (!_buckets.TryGetValue(newKey, out var newBucket))
        {
            newBucket = new List<GameObject>();
            _buckets[newKey] = newBucket;
        }

        newBucket.Add(entity);
        _entityBuckets[entity] = newKey;
    }

    public IEnumerable<GameObject> Query(Vector2 point, float radius)
    {
        int minX = (int)((point.X - radius) / _bucketSize);
        int maxX = (int)((point.X + radius) / _bucketSize);
        int minY = (int)((point.Y - radius) / _bucketSize);
        int maxY = (int)((point.Y + radius) / _bucketSize);

        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                if (_buckets.TryGetValue((x, y), out var bucket))
                {
                    foreach (var entity in bucket)
                    {
                        if (entity.Enabled)
                            yield return entity;
                    }
                }
            }
        }
    }

    public void Clear()
    {
        _buckets.Clear();
        _entityBuckets.Clear();
    }
}

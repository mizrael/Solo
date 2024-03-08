using Microsoft.Xna.Framework;
using Monoroids.Core.Components;
using System.Collections.Generic;
using System.Linq;

namespace Monoroids.Core.Services;

public class CollisionService : IGameService
{
    private CollisionBucket[,] _buckets;
    private SceneManager _sceneManager;
    private RenderService _renderService;
    private readonly Point _bucketSize;
    private readonly Dictionary<int, IList<CollisionBucket>> _bucketsByCollider = new();


    public CollisionService(Point bucketSize)
    {
        _bucketSize = bucketSize;
    }

    public void Initialize()
    {
        _sceneManager = GameServicesManager.Instance.GetService<SceneManager>();
        _sceneManager.OnSceneChanged += OnSceneChanged;

        _renderService = GameServicesManager.Instance.GetService<RenderService>();
        _renderService.Graphics.DeviceReset += (s, e) => BuildBuckets();

        BuildBuckets();
    }

    private void OnSceneChanged(Scene currentScene) => BuildBuckets();

    private void BuildBuckets()
    {
        var rows = _renderService.Graphics.PreferredBackBufferHeight / _bucketSize.Y;
        var cols = _renderService.Graphics.PreferredBackBufferWidth / _bucketSize.X;
        _buckets = new CollisionBucket[rows, cols];

        for (int row = 0; row < rows; row++)
            for (int col = 0; col < cols; col++)
            {
                var bounds = new Rectangle(
                    col * _bucketSize.X,
                    row * _bucketSize.Y,
                    _bucketSize.X,
                    _bucketSize.Y);
                _buckets[row, col] = new CollisionBucket(bounds);
            }

        _bucketsByCollider.Clear();

        var colliders = FindAllColliders();
        foreach (var collider in colliders)
        {
            Add(collider);
        }
    }

    private void CheckCollisions(BoundingBoxComponent bbox)
    {
        RefreshColliderBuckets(bbox);

        var buckets = _bucketsByCollider[bbox.Owner.Id];
        foreach (var bucket in buckets)
        {
            bucket.CheckCollisions(bbox);
        }
    }

    private void RefreshColliderBuckets(BoundingBoxComponent collider)
    {
        var rows = _buckets.GetLength(0);
        var cols = _buckets.GetLength(1);
        var startX = (int)(cols * ((float)collider.Bounds.Left / _renderService.Graphics.PreferredBackBufferWidth));
        var startY = (int)(rows * ((float)collider.Bounds.Top / _renderService.Graphics.PreferredBackBufferHeight));

        var endX = (int)(cols * ((float)collider.Bounds.Right / _renderService.Graphics.PreferredBackBufferWidth));
        var endY = (int)(rows * ((float)collider.Bounds.Bottom / _renderService.Graphics.PreferredBackBufferHeight));

        if (!_bucketsByCollider.ContainsKey(collider.Owner.Id))
            _bucketsByCollider[collider.Owner.Id] = new List<CollisionBucket>();
        foreach (var bucket in _bucketsByCollider[collider.Owner.Id])
            bucket.Remove(collider);
        _bucketsByCollider[collider.Owner.Id].Clear();

        for (int row = startY; row <= endY; row++)
            for (int col = startX; col <= endX; col++)
            {
                if (row < 0 || row >= rows)
                    continue;
                if (col < 0 || col >= cols)
                    continue;

                if (_buckets[row, col].Bounds.Intersects(collider.Bounds))
                {
                    _bucketsByCollider[collider.Owner.Id].Add(_buckets[row, col]);
                    _buckets[row, col].Add(collider);
                }
            }
    }

    private IEnumerable<BoundingBoxComponent> FindAllColliders()
    {
        var scene = _sceneManager.Current;
        if(scene?.Root is null)
            return Enumerable.Empty<BoundingBoxComponent>();

        var colliders = new List<BoundingBoxComponent>();

        FindAllColliders(scene.Root, colliders);

        return colliders;
    }

    private void FindAllColliders(GameObject node, IList<BoundingBoxComponent> colliders)
    {
        if (node is null)
            return;

        if (node.Components.TryGet<BoundingBoxComponent>(out var bbox))
            colliders.Add(bbox);

        if (node.Children is not null)
            foreach (var child in node.Children)
                FindAllColliders(child, colliders);
    }

    public void Add(BoundingBoxComponent collider)
    {
        collider.OnPositionChanged -= CheckCollisions;
        collider.OnPositionChanged += CheckCollisions;
        RefreshColliderBuckets(collider);
    }

    public void Remove(BoundingBoxComponent collider)
    {
        collider.OnPositionChanged -= CheckCollisions;
        if (_bucketsByCollider.ContainsKey(collider.Owner.Id))
        {
            foreach (var bucket in _bucketsByCollider[collider.Owner.Id])
                bucket.Remove(collider);
            _bucketsByCollider.Remove(collider.Owner.Id);
        }
    }
}


using Microsoft.Xna.Framework;
using Solo.Components;

namespace Solo.Services;

public class BoundingBoxCollisionService : IGameService
{
    private BoundingBoxCollisionBucket[,] _buckets;
    private SceneManager _sceneManager;
    private readonly Point _bucketSize;
    private readonly Dictionary<int, IList<BoundingBoxCollisionBucket>> _bucketsByCollider = new();

    public BoundingBoxCollisionService(Point bucketSize)
    {
        _bucketSize = bucketSize;
    }

    public void Initialize()
    {
        _sceneManager = SceneManager.Instance;
        _sceneManager.OnSceneChanged += OnSceneChanged;

        GraphicsDeviceManagerAccessor.Instance.GraphicsDeviceManager.DeviceReset += (s, e) => BuildBuckets();

        BuildBuckets();
    }

    private void OnSceneChanged(Scene currentScene) => BuildBuckets();

    private void BuildBuckets()
    {
        var viewport = GraphicsDeviceManagerAccessor.Instance.GraphicsDeviceManager.GraphicsDevice.Viewport;
        var rows = viewport.Height / _bucketSize.Y;
        var cols = viewport.Width / _bucketSize.X;
        _buckets = new BoundingBoxCollisionBucket[rows, cols];

        for (int row = 0; row < rows; row++)
            for (int col = 0; col < cols; col++)
            {
                var bounds = new Rectangle(
                    col * _bucketSize.X,
                    row * _bucketSize.Y,
                    _bucketSize.X,
                    _bucketSize.Y);
                _buckets[row, col] = new BoundingBoxCollisionBucket(bounds);
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
        if(collider is null || _buckets is null)
            return;

        var rows = _buckets.GetLength(0);
        var cols = _buckets.GetLength(1);
        var viewport = GraphicsDeviceManagerAccessor.Instance.GraphicsDeviceManager.GraphicsDevice.Viewport;
        var startX = (int)(cols * ((float)collider.Bounds.Left / viewport.Width));
        var startY = (int)(rows * ((float)collider.Bounds.Top / viewport.Height));

        var endX = (int)(cols * ((float)collider.Bounds.Right / viewport.Width));
        var endY = (int)(rows * ((float)collider.Bounds.Bottom / viewport.Height));

        if (!_bucketsByCollider.ContainsKey(collider.Owner.Id))
            _bucketsByCollider[collider.Owner.Id] = new List<BoundingBoxCollisionBucket>();

        if (!_bucketsByCollider.TryGetValue(collider.Owner.Id, out var colliderBuckets))
        {
            _bucketsByCollider[collider.Owner.Id] = colliderBuckets = new List<BoundingBoxCollisionBucket>();
        }
        else
        {
            foreach (var bucket in colliderBuckets)
                bucket.Remove(collider);
            colliderBuckets.Clear();
        }

        for (int row = startY; row <= endY; row++)
            for (int col = startX; col <= endX; col++)
            {
                if (row < 0 || row >= rows)
                    continue;
                if (col < 0 || col >= cols)
                    continue;

                if (_buckets[row, col].Bounds.Intersects(collider.Bounds))
                {
                    colliderBuckets.Add(_buckets[row, col]);
                    _buckets[row, col].Add(collider);
                }
            }
    }

    private IEnumerable<BoundingBoxComponent> FindAllColliders()
    {
        var scene = _sceneManager.Current;
        if(scene?.ObjectsGraph.Root is null)
            return Enumerable.Empty<BoundingBoxComponent>();

        var colliders = new List<BoundingBoxComponent>();

        FindAllColliders(scene.ObjectsGraph.Root, colliders);

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


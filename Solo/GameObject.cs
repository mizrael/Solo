using Microsoft.Xna.Framework;
using Solo.Components;
using System.Diagnostics.CodeAnalysis;

namespace Solo;

public class GameObject
{
    private readonly IList<GameObject> _children;
    private readonly Queue<GameObject> _childrenToRemove = new();
    private readonly Queue<GameObject> _childrenToAdd = new();
    private readonly HashSet<string> _tags = new();

    private GameObjectName? _name;
    private Dictionary<GameObjectName, GameObject>? _namedChildren;

    public GameObject()
    {
        Id = IdGenerator<GameObject>.Next();

        _children = new List<GameObject>();

        Components = new ComponentsCollection(this);
    }

    public int Id { get; }

    public GameObjectName? Name
    {
        get => _name;
        set
        {
            if (value == _name) return;
            if (Parent != null)
            {
                if (_name.HasValue)
                    Parent.UnregisterChildName(this);
                if (value.HasValue)
                    Parent.RegisterChildName(value.Value, this);
            }
            _name = value;
        }
    }

    public ComponentsCollection Components { get; }

    public IEnumerable<GameObject> Children => _children;
    public GameObject? Parent { get; private set; }

    public OnDisabledHandler OnDisabled;
    public delegate void OnDisabledHandler(GameObject gameObject);

    private bool _enabled = true;
    public bool Enabled
    {
        get => _enabled;
        set
        {
            var oldValue = _enabled;
            _enabled = value;
            if (!_enabled && oldValue)
                OnDisabled?.Invoke(this);
        }
    }

    private void RegisterChildName(GameObjectName name, GameObject child)
    {
        _namedChildren ??= new Dictionary<GameObjectName, GameObject>();
        if (!_namedChildren.TryAdd(name, child))
            throw new InvalidOperationException($"A child named '{name}' already exists.");
    }

    private void UnregisterChildName(GameObject child)
    {
        _namedChildren?.Remove(child._name!.Value);
    }

    private void ApplyPendingChanges()
    {
        while (_childrenToRemove.Count > 0)
        {
            var child = _childrenToRemove.Dequeue();
            if (child._name.HasValue)
                UnregisterChildName(child);
            child.Parent = null;
            _children.Remove(child);
        }

        while (_childrenToAdd.Count > 0)
        {
            var child = _childrenToAdd.Dequeue();
            child.Parent?._children.Remove(child);
            if (child._name.HasValue)
                child.Parent?.UnregisterChildName(child);
            child.Parent = this;
            _children.Add(child);
            if (child._name.HasValue)
                RegisterChildName(child._name.Value, child);
        }
    }

    public void AddChild(GameObject child)
    {
        if (child.Parent is not null && Equals(child.Parent))
            return;
        if (child._name.HasValue && _namedChildren != null && _namedChildren.ContainsKey(child._name.Value))
            throw new InvalidOperationException($"A child named '{child._name.Value}' already exists.");
        _childrenToAdd.Enqueue(child);
    }

    public void RemoveChild(GameObject child)
    {
        if (child.Parent is null || !Equals(child.Parent))
            return;
        _childrenToRemove.Enqueue(child);
    }

    public void Update(GameTime gameTime)
    {
        if (!Enabled)
            return;

        ApplyPendingChanges();

        foreach (var component in Components)
            component.Update(gameTime);

        foreach (var child in _children)
            child.Update(gameTime);
    }

    public bool TryGetChildByName(GameObjectName name, [NotNullWhen(true)] out GameObject? child)
    {
        if (_namedChildren != null && _namedChildren.TryGetValue(name, out child))
            return true;
        child = null;
        return false;
    }

    public bool TryResolvePath(string path, [NotNullWhen(true)] out GameObject? result)
    {
        result = this;
        var segments = GameObjectName.ParsePath(path);
        foreach (var segment in segments)
        {
            if (!result.TryGetChildByName(segment, out var child))
            {
                result = null;
                return false;
            }
            result = child;
        }
        return true;
    }

    public string? GetPath()
    {
        if (!_name.HasValue)
            return null;

        var segments = new Stack<GameObjectName>();
        var current = this;
        while (current != null && current.Parent != null)
        {
            if (!current._name.HasValue)
                return null;
            segments.Push(current._name.Value);
            current = current.Parent;
        }

        var array = new GameObjectName[segments.Count];
        var i = 0;
        while (segments.Count > 0)
            array[i++] = segments.Pop();

        return GameObjectName.BuildPath(array);
    }

    public void AddTag(string tag) => _tags.Add(tag);
    public void RemoveTag(string tag) => _tags.Remove(tag);
    public bool HasTag(string tag) => _tags.Contains(tag);

    public GameObject? FindFirst(Func<GameObject, bool> predicate)
    {
        if (predicate(this))
            return this;

        foreach (var child in this.Children)
        {
            var found = child.FindFirst(predicate);
            if (found != null)
                return found;
        }

        return null;
    }

    public override int GetHashCode() => Id;

    public override bool Equals(object obj) => obj is GameObject node && Id.Equals(node.Id);

    public override string ToString() => $"[{this.GetType().Name}] #{Id}{(_name.HasValue ? $" '{_name.Value}'" : "")}";
}

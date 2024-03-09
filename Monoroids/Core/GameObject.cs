using Microsoft.Xna.Framework;
using Monoroids.Core.Components;
using System.Collections.Generic;

namespace Monoroids.Core;

public class GameObject
{
    private static int _lastId = 0;

    private readonly IList<GameObject> _children;
    private readonly Queue<GameObject> _childrenToRemove = new();
    private readonly Queue<GameObject> _childrenToAdd = new();

    public GameObject()
    {
        Id = ++_lastId;

        _children = new List<GameObject>();

        Components = new ComponentsCollection(this);
    }

    public int Id { get; }

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

    private void ApplyPendingChanges()
    {
        while (_childrenToRemove.Count > 0)
        {
            var child = _childrenToRemove.Dequeue();
            child.Parent = null;
            _children.Remove(child);
        }

        while (_childrenToAdd.Count > 0)
        {
            var child = _childrenToAdd.Dequeue();
            child.Parent?._children.Remove(child);
            child.Parent = this;
            _children.Add(child);
        }
    }

    public void AddChild(GameObject child)
    {
        if (child.Parent is not null && Equals(child.Parent))
            return;
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

    public override int GetHashCode() => Id;

    public override bool Equals(object obj) => obj is GameObject node && Id.Equals(node.Id);

    public override string ToString() => $"GameObject {Id}";
}

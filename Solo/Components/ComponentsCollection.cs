﻿using System.Collections;
using Solo.Exceptions;

namespace Solo.Components;

public class ComponentsCollection : IEnumerable<Component>
{
    private readonly GameObject _owner;
    private readonly IDictionary<Type, Component> _items;

    public ComponentsCollection(GameObject owner)
    {
        _owner = owner;
        _items = new Dictionary<Type, Component>();
    }

    public TC Add<TC>() where TC : Component
    {
        var type = typeof(TC);
        if (!_items.ContainsKey(type))
        {
            var component = ComponentsFactory.Instance.Create<TC>(_owner);
            _items.Add(type, component);
        }

        return _items[type] as TC;
    }

    public T Get<T>() where T : Component
    {
        var type = typeof(T);
        return _items.ContainsKey(type) ? _items[type] as T : throw new ComponentNotFoundException<T>(_owner);
    }

    public bool TryGet<T>(out T result) where T : Component
    {
        var type = typeof(T);
        _items.TryGetValue(type, out var tmp);
        result = tmp as T;
        return result != null;
    }

    public IEnumerator<Component> GetEnumerator() => _items.Values.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
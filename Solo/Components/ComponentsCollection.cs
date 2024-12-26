using System.Collections;
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

        if (_items.TryGetValue(type, out var result))
            return (TC)result;
        
        var component = ComponentsFactory.Instance.Create<TC>(_owner);
        _items.Add(type, component);
        return component;      
    }

    public T Get<T>() where T : Component
    {
        var type = typeof(T);

        if (!_items.TryGetValue(type, out var result))
            throw new ComponentNotFoundException<T>(_owner);

        return (T)result;
    }

    public bool TryGet<T>(out T result) where T : Component
    {
        var type = typeof(T);
        _items.TryGetValue(type, out var tmp);
        
        result = tmp as T;
        return result != null;
    }

    public bool Has<T>() where T : Component => _items.ContainsKey(typeof(T));

    public IEnumerator<Component> GetEnumerator() => _items.Values.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
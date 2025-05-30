using System.Collections;
using System.Diagnostics.CodeAnalysis;
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

    public TC Get<TC>() where TC : Component
    {
        var type = typeof(TC);

        if (!_items.TryGetValue(type, out var result))
            throw new ComponentNotFoundException<TC>(_owner);

        return (TC)result;
    }

    public bool TryGet<TC>([NotNullWhen(true)] out TC? result) 
        where TC : Component
    {
        var type = typeof(TC);
        _items.TryGetValue(type, out var tmp);
        
        result = tmp as TC;
        return result != null;
    }

    public bool Has<T>() where T : Component => _items.ContainsKey(typeof(T));

    public IEnumerator<Component> GetEnumerator() => _items.Values.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
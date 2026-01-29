using Microsoft.Xna.Framework;
using System.Collections;

namespace Solo.Services;

public class GameServicesCollection : IEnumerable<IGameService>
{
    private readonly Dictionary<Type, IGameService> _servicesMap = new();
    private readonly List<IGameService> _services = new();
    private bool _isInitialized;

    public T GetRequired<T>() where T : class, IGameService
    {
        if (!_servicesMap.TryGetValue(typeof(T), out var service))
            throw new Exceptions.ServiceNotFoundException<T>();
        return (T)service;
    }

    public T? Get<T>() where T : class, IGameService
    {
        _servicesMap.TryGetValue(typeof(T), out var service);
        return service as T;
    }

    public void Add(IGameService service)
    {
        if (service == null)
            throw new ArgumentNullException(nameof(service));
        var serviceType = service.GetType();
        if (_servicesMap.ContainsKey(serviceType))
            throw new ArgumentException($"Service of type '{serviceType.Name}' already exists");
        _services.Add(service);
        _servicesMap[serviceType] = service;
    }

    public void Step(GameTime gameTime)
    {
        if (!_isInitialized)
        {
            foreach (var service in _services)
                service.Initialize();
            _isInitialized = true;
        }

        foreach (var service in _services)
            service.Update(gameTime);
    }

    public IEnumerator<IGameService> GetEnumerator()
    => _services.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}

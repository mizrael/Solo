namespace Solo.Services;

public sealed class GameServicesManager
{
    private bool _isInitialized = false;

    private Dictionary<Type, IGameService> _servicesMap = new();
    private List<IGameService> _services = new();

    private GameServicesManager() { }
    
    private static Lazy<GameServicesManager> _instance = new(() => new GameServicesManager());
    public static GameServicesManager Instance => _instance.Value;

    public T GetRequired<T>() where T : class, IGameService
    {
        if (!_servicesMap.TryGetValue(typeof(T), out var service))
            throw new Exceptions.ServiceNotFoundException<T>();

        return service as T;
    }

    public void AddService(IGameService service)
    {
        if (service == null)
            throw new ArgumentNullException(nameof(service));
        var serviceType = service.GetType();
        if (_servicesMap.ContainsKey(serviceType))
            throw new ArgumentException($"there is already a service of type '{serviceType.Name}'");
        _services.Add(service);
        _servicesMap[serviceType] = service;
    }

    public void Initialize()
    {
        foreach(var service in _services)
            service.Initialize();

        _isInitialized = true;
    }

    public void Step(Microsoft.Xna.Framework.GameTime gameTime)
    {
        if (!_isInitialized)
            return;

        foreach (var service in _services)
            service.Step(gameTime);
    }
}
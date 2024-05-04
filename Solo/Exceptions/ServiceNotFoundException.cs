using Solo.Services;

namespace Solo.Exceptions;

public class ServiceNotFoundException<TC> : Exception where TC : IGameService
{
    public ServiceNotFoundException() : base($"service {typeof(TC).Name} not found")
    {
    }
}

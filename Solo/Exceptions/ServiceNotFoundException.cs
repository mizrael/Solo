using System;
using Monoroids.Core.Services;

namespace Monoroids.Core.Exceptions;

public class ServiceNotFoundException<TC> : Exception where TC : IGameService
{
    public ServiceNotFoundException() : base($"service {typeof(TC).Name} not found")
    {
    }
}

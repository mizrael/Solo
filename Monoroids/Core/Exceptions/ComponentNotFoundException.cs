using System;
using Monoroids.Core.Components;

namespace Monoroids.Core.Exceptions;

public class ComponentNotFoundException<TC> : Exception where TC : Component
{
    public ComponentNotFoundException(GameObject owner) : base($"Component {typeof(TC).Name} not found on owner {owner.Id}.")
    {
    }
}

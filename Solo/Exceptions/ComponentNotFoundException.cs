using Solo.Components;

namespace Solo.Exceptions;

public class ComponentNotFoundException<TC> : Exception where TC : Component
{
    public ComponentNotFoundException(GameObject owner) : base($"Component {typeof(TC).Name} not found on owner {owner.Id}.")
    {
    }
}

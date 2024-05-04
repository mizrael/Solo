using Solo.Components;

namespace Solo.Exceptions;

public class ComponentAlreadyAddedException : Exception
{
    public ComponentAlreadyAddedException(GameObject owner, Component component)
         : base($"Component {component.GetType().Name} was already added on owner {owner.Id}.")
    {
    }
}
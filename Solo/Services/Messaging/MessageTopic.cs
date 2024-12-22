namespace Solo.Services.Messaging;

public class MessageTopic
{
    private readonly List<Subscription> _subscriptions = new();

    public MessageTopic(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name, nameof(name));
        Name = name;
    }

    public string Name { get; }

    public void Subscribe(GameObject gameObject, Action<GameObject, IMessage> handler)
    {
        ArgumentNullException.ThrowIfNull(gameObject, nameof(gameObject));
        ArgumentNullException.ThrowIfNull(handler, nameof(handler));
        _subscriptions.Add(new Subscription(gameObject, handler));
    }

    public void Publish()
    {
        Publish(NullMessage.Instance);
    }

    public void Publish(IMessage message)
    {
        foreach (var subscription in _subscriptions)
        {
            subscription.Handler(subscription.GameObject, message);
        }
    }

    private record Subscription(GameObject GameObject, Action<GameObject, IMessage> Handler);
}

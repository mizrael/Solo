namespace Solo.Services.Messaging;

public class MessageTopic<TM>
    where TM : IMessage
{
    private readonly List<Subscription> _subscriptions = new();

    public void Subscribe(GameObject gameObject, Action<GameObject, TM> handler)
    {
        ArgumentNullException.ThrowIfNull(gameObject, nameof(gameObject));
        ArgumentNullException.ThrowIfNull(handler, nameof(handler));
        _subscriptions.Add(new Subscription(gameObject, handler));
    }

    public void Publish(TM message)
    {
        int end = _subscriptions.Count;
        int i = 0;
        while (i != end)
        {
            var sub = _subscriptions[i++];
            if(sub.GameObject.Enabled)
                sub.Handler(sub.GameObject, message);
        }
    }

    private record struct Subscription(GameObject GameObject, Action<GameObject, TM> Handler);
}

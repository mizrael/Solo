namespace Solo.Services.Messaging;

public class MessageBus : IGameService
{
    private readonly Dictionary<string, object> _topics = new();

    public MessageTopic<TM> GetTopic<TM>()
        where TM : IMessage
    {
        var messageName = typeof(TM).Name;
        if(!_topics.TryGetValue(messageName, out var topic))
        {
            topic = new MessageTopic<TM>();
            _topics[messageName] = topic;
        }

        return (MessageTopic<TM>)topic;
    }
}

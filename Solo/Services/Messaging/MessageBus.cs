namespace Solo.Services.Messaging;

public class MessageBus : IGameService
{
    private readonly Dictionary<string, MessageTopic> _topics = new();

    public MessageTopic GetTopic(string name)
    {
        if (!_topics.ContainsKey(name))
            _topics.Add(name, new MessageTopic(name));
        return _topics[name];
    }
}

namespace Solo.Services.Messaging;

public interface IMessage { }

public struct NullMessage<TM> : IMessage
    where TM : IMessage
{ 
    public static readonly NullMessage<TM> Instance = new();
}
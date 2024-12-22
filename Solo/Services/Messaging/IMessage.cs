namespace Solo.Services.Messaging;

public interface IMessage { }

public struct NullMessage : IMessage { 
    public static readonly NullMessage Instance = new();
}
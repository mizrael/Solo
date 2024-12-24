using Solo.Services.Messaging;

namespace SpaceInvaders.Logic.Messages;

public record struct SetDirection(int NewDirection) : IMessage;
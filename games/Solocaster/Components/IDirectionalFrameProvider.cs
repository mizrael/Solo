using Solocaster.Monsters;

namespace Solocaster.Components;

public interface IDirectionalFrameProvider : IFrameProvider
{
    void SetDirection(Direction direction);
    Direction CurrentDirection { get; }
}

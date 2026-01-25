using SpriteSheetEditor.Models;

namespace SpriteSheetEditor.UndoRedo.Commands;

public class ReorderFrameCommand : IUndoableCommand
{
    private readonly AnimationDefinition _animation;
    private readonly int _oldIndex;
    private readonly int _newIndex;

    public string Description => "Reorder animation frame";

    public ReorderFrameCommand(AnimationDefinition animation, int oldIndex, int newIndex)
    {
        _animation = animation;
        _oldIndex = oldIndex;
        _newIndex = newIndex;
    }

    public void Execute()
    {
        _animation.Frames.Move(_oldIndex, _newIndex);
    }

    public void Undo()
    {
        _animation.Frames.Move(_newIndex, _oldIndex);
    }

    public void Dispose()
    {
    }
}

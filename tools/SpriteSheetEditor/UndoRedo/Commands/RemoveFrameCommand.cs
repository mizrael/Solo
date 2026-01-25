using SpriteSheetEditor.Models;

namespace SpriteSheetEditor.UndoRedo.Commands;

public class RemoveFrameCommand : IUndoableCommand
{
    private readonly AnimationDefinition _animation;
    private readonly AnimationFrame _frame;
    private int _originalIndex;

    public string Description => $"Remove frame from '{_animation.Name}'";

    public RemoveFrameCommand(AnimationDefinition animation, AnimationFrame frame)
    {
        _animation = animation;
        _frame = frame;
    }

    public void Execute()
    {
        _originalIndex = _animation.Frames.IndexOf(_frame);
        _animation.Frames.Remove(_frame);
    }

    public void Undo()
    {
        if (_originalIndex >= 0 && _originalIndex <= _animation.Frames.Count)
        {
            _animation.Frames.Insert(_originalIndex, _frame);
        }
        else
        {
            _animation.Frames.Add(_frame);
        }
    }

    public void Dispose()
    {
    }
}

using SpriteSheetEditor.Models;

namespace SpriteSheetEditor.UndoRedo.Commands;

public class RemoveAnimationCommand : IUndoableCommand
{
    private readonly SpriteSheetDocument _document;
    private readonly AnimationDefinition _animation;
    private int _originalIndex;

    public string Description => $"Remove animation '{_animation.Name}'";

    public RemoveAnimationCommand(SpriteSheetDocument document, AnimationDefinition animation)
    {
        _document = document;
        _animation = animation;
    }

    public void Execute()
    {
        _originalIndex = _document.Animations.IndexOf(_animation);
        _document.Animations.Remove(_animation);
    }

    public void Undo()
    {
        if (_originalIndex >= 0 && _originalIndex <= _document.Animations.Count)
        {
            _document.Animations.Insert(_originalIndex, _animation);
        }
        else
        {
            _document.Animations.Add(_animation);
        }
    }

    public void Dispose()
    {
    }
}

using SpriteSheetEditor.Models;

namespace SpriteSheetEditor.UndoRedo.Commands;

public class AddAnimationCommand : IUndoableCommand
{
    private readonly SpriteSheetDocument _document;
    private readonly AnimationDefinition _animation;

    public string Description => $"Add animation '{_animation.Name}'";

    public AddAnimationCommand(SpriteSheetDocument document, AnimationDefinition animation)
    {
        _document = document;
        _animation = animation;
    }

    public void Execute()
    {
        if (!_document.Animations.Contains(_animation))
        {
            _document.Animations.Add(_animation);
        }
    }

    public void Undo()
    {
        _document.Animations.Remove(_animation);
    }

    public void Dispose()
    {
    }
}

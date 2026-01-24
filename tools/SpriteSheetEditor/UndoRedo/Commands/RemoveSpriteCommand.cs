using SpriteSheetEditor.Models;

namespace SpriteSheetEditor.UndoRedo.Commands;

public class RemoveSpriteCommand : IUndoableCommand
{
    private readonly SpriteSheetDocument _document;
    private readonly SpriteDefinition _sprite;
    private int _originalIndex;

    public string Description => $"Remove sprite '{_sprite.Name}'";

    public RemoveSpriteCommand(SpriteSheetDocument document, SpriteDefinition sprite)
    {
        _document = document;
        _sprite = sprite;
    }

    public void Execute()
    {
        _originalIndex = _document.Sprites.IndexOf(_sprite);
        _document.Sprites.Remove(_sprite);
    }

    public void Undo()
    {
        if (_originalIndex >= 0 && _originalIndex <= _document.Sprites.Count)
        {
            _document.Sprites.Insert(_originalIndex, _sprite);
        }
        else
        {
            _document.Sprites.Add(_sprite);
        }
    }

    public void Dispose()
    {
    }
}

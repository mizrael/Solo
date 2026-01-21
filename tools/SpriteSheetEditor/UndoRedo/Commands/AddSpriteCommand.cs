using SpriteSheetEditor.Models;

namespace SpriteSheetEditor.UndoRedo.Commands;

public class AddSpriteCommand : IUndoableCommand
{
    private readonly SpriteSheetDocument _document;
    private readonly SpriteDefinition _sprite;

    public string Description => $"Add sprite '{_sprite.Name}'";

    public AddSpriteCommand(SpriteSheetDocument document, SpriteDefinition sprite)
    {
        _document = document;
        _sprite = sprite;
    }

    public void Execute()
    {
        if (!_document.Sprites.Contains(_sprite))
        {
            _document.Sprites.Add(_sprite);
        }
    }

    public void Undo()
    {
        _document.Sprites.Remove(_sprite);
    }
}

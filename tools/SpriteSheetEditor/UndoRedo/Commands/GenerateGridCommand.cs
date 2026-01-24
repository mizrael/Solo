using SpriteSheetEditor.Models;

namespace SpriteSheetEditor.UndoRedo.Commands;

public class GenerateGridCommand : IUndoableCommand
{
    private readonly SpriteSheetDocument _document;
    private readonly List<SpriteDefinition> _oldSprites;
    private readonly List<SpriteDefinition> _newSprites;

    public string Description => "Generate grid";

    public GenerateGridCommand(SpriteSheetDocument document, List<SpriteDefinition> newSprites)
    {
        _document = document;
        _oldSprites = document.Sprites.ToList();
        _newSprites = newSprites.ToList();
    }

    public void Execute()
    {
        _document.Sprites.Clear();
        foreach (var sprite in _newSprites)
        {
            _document.Sprites.Add(sprite);
        }
    }

    public void Undo()
    {
        _document.Sprites.Clear();
        foreach (var sprite in _oldSprites)
        {
            _document.Sprites.Add(sprite);
        }
    }

    public void Dispose()
    {
    }
}

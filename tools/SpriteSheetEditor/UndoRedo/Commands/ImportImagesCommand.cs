using SkiaSharp;
using SpriteSheetEditor.Models;

namespace SpriteSheetEditor.UndoRedo.Commands;

public class ImportImagesCommand : IUndoableCommand
{
    private readonly SpriteSheetDocument _document;

    private readonly SKBitmap? _previousImage;
    private readonly IReadOnlyList<SpriteDefinition> _previousSprites;

    private readonly SKBitmap _newImage;
    private readonly IReadOnlyList<SpriteDefinition> _spritesToAdd;

    public string Description => "Import Images";

    public ImportImagesCommand(
        SpriteSheetDocument document,
        SKBitmap newExpandedImage,
        IReadOnlyList<SpriteDefinition> spritesToAdd)
    {
        _document = document;

        _previousImage = document.LoadedImage?.Copy();
        _previousSprites = document.Sprites.ToList();

        _newImage = newExpandedImage;
        _spritesToAdd = spritesToAdd.ToList();
    }

    public void Execute()
    {
        _document.LoadedImage?.Dispose();
        _document.LoadedImage = _newImage.Copy();

        foreach (var sprite in _spritesToAdd)
        {
            _document.Sprites.Add(new SpriteDefinition
            {
                Name = sprite.Name,
                X = sprite.X,
                Y = sprite.Y,
                Width = sprite.Width,
                Height = sprite.Height
            });
        }
    }

    public void Undo()
    {
        _document.LoadedImage?.Dispose();
        _document.LoadedImage = _previousImage?.Copy();

        _document.Sprites.Clear();
        foreach (var sprite in _previousSprites)
        {
            _document.Sprites.Add(new SpriteDefinition
            {
                Name = sprite.Name,
                X = sprite.X,
                Y = sprite.Y,
                Width = sprite.Width,
                Height = sprite.Height
            });
        }
    }

    public void Dispose()
    {
        _previousImage?.Dispose();
        _newImage.Dispose();
    }
}

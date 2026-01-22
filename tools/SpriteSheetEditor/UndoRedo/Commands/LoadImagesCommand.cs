using SkiaSharp;
using SpriteSheetEditor.Models;

namespace SpriteSheetEditor.UndoRedo.Commands;

public class LoadImagesCommand : IUndoableCommand
{
    private readonly SpriteSheetDocument _document;

    private readonly SKBitmap? _previousImage;
    private readonly IReadOnlyList<SpriteDefinition> _previousSprites;
    private readonly string _previousSheetName;
    private readonly string? _previousImageFilePath;

    private readonly SKBitmap _newImage;
    private readonly IReadOnlyList<SpriteDefinition> _newSprites;
    private readonly string _newSheetName;

    public string Description => "Load Images";

    public LoadImagesCommand(
        SpriteSheetDocument document,
        SKBitmap newImage,
        IReadOnlyList<SpriteDefinition> newSprites,
        string newSheetName)
    {
        _document = document;

        _previousImage = document.LoadedImage?.Copy();
        _previousSprites = document.Sprites.ToList();
        _previousSheetName = document.SpriteSheetName;
        _previousImageFilePath = document.ImageFilePath;

        _newImage = newImage;
        _newSprites = newSprites.ToList();
        _newSheetName = newSheetName;
    }

    public void Execute()
    {
        _document.LoadedImage?.Dispose();
        _document.LoadedImage = _newImage.Copy();
        _document.SpriteSheetName = _newSheetName;
        _document.ImageFilePath = null;

        _document.Sprites.Clear();
        foreach (var sprite in _newSprites)
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
        _document.SpriteSheetName = _previousSheetName;
        _document.ImageFilePath = _previousImageFilePath;

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

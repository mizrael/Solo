using SkiaSharp;
using SpriteSheetEditor.Models;

namespace SpriteSheetEditor.UndoRedo.Commands;

public class RearrangeLayoutCommand : IUndoableCommand
{
    private readonly SpriteSheetDocument _document;

    private readonly SKBitmap _previousImage;
    private readonly IReadOnlyList<SpriteDefinition> _previousSprites;

    private readonly SKBitmap _newImage;
    private readonly IReadOnlyList<SpriteDefinition> _newSprites;

    public string Description => "Rearrange layout";

    public RearrangeLayoutCommand(
        SpriteSheetDocument document,
        SKBitmap newImage,
        IReadOnlyList<SpriteDefinition> newSprites)
    {
        _document = document;

        _previousImage = document.LoadedImage!.Copy();
        _previousSprites = document.Sprites.Select(s => new SpriteDefinition
        {
            Name = s.Name,
            X = s.X,
            Y = s.Y,
            Width = s.Width,
            Height = s.Height
        }).ToList();

        _newImage = newImage;
        _newSprites = newSprites.ToList();
    }

    public void Execute()
    {
        _document.LoadedImage?.Dispose();
        _document.LoadedImage = _newImage.Copy();

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
        _document.LoadedImage = _previousImage.Copy();

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
}

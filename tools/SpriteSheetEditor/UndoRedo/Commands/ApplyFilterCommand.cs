using SkiaSharp;
using SpriteSheetEditor.Models;

namespace SpriteSheetEditor.UndoRedo.Commands;

public class ApplyFilterCommand : IUndoableCommand
{
    private readonly SpriteSheetDocument _document;
    private readonly SKBitmap _originalImage;
    private readonly SKBitmap _filteredImage;

    public string Description { get; }

    public ApplyFilterCommand(SpriteSheetDocument document, SKBitmap originalImage, SKBitmap filteredImage, string filterName)
    {
        _document = document;
        _originalImage = originalImage.Copy();
        _filteredImage = filteredImage.Copy();
        Description = $"Apply filter '{filterName}'";
    }

    public void Execute()
    {
        _document.LoadedImage?.Dispose();
        _document.LoadedImage = _filteredImage.Copy();
    }

    public void Undo()
    {
        _document.LoadedImage?.Dispose();
        _document.LoadedImage = _originalImage.Copy();
    }
}

using SkiaSharp;
using SpriteSheetEditor.Models;

namespace SpriteSheetEditor.Services;

public record ImportResult(SpriteSheetDocument Document, SKBitmap CompositeImage);

public static class ImageImporter
{
    public static async Task<ImportResult> ImportImagesAsync(IEnumerable<string> filePaths, int padding = 0)
    {
        var packingItems = await LoadImagesAsync(filePaths);
        if (packingItems.Count == 0)
        {
            throw new InvalidOperationException("No valid images to import.");
        }

        var packedResult = BinPacker.Pack(packingItems, padding);
        var compositeImage = CreateCompositeImage(packedResult);
        var document = CreateDocument(packedResult);

        foreach (var item in packingItems)
        {
            item.Image.Dispose();
        }

        return new ImportResult(document, compositeImage);
    }

    private static async Task<List<PackingItem>> LoadImagesAsync(IEnumerable<string> filePaths)
    {
        var items = new List<PackingItem>();

        foreach (var filePath in filePaths)
        {
            try
            {
                var bytes = await File.ReadAllBytesAsync(filePath);
                var bitmap = SKBitmap.Decode(bytes);
                if (bitmap is not null)
                {
                    var name = Path.GetFileNameWithoutExtension(filePath);
                    items.Add(new PackingItem(name, bitmap.Width, bitmap.Height, bitmap));
                }
            }
            catch
            {
                // Skip files that can't be loaded as images
            }
        }

        return items;
    }

    private static SKBitmap CreateCompositeImage(PackedResult packedResult)
    {
        var bitmap = new SKBitmap(packedResult.CanvasWidth, packedResult.CanvasHeight, SKColorType.Bgra8888, SKAlphaType.Premul);
        using var canvas = new SKCanvas(bitmap);
        canvas.Clear(SKColors.Transparent);

        foreach (var item in packedResult.Items)
        {
            canvas.DrawBitmap(item.Image, item.X, item.Y);
        }

        return bitmap;
    }

    private static SpriteSheetDocument CreateDocument(PackedResult packedResult)
    {
        var document = new SpriteSheetDocument
        {
            SpriteSheetName = "imported_spritesheet"
        };

        foreach (var item in packedResult.Items)
        {
            document.Sprites.Add(new SpriteDefinition
            {
                Name = item.Name,
                X = item.X,
                Y = item.Y,
                Width = item.Width,
                Height = item.Height
            });
        }

        return document;
    }
}

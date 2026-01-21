using SkiaSharp;
using SpriteSheetEditor.Models;

namespace SpriteSheetEditor.Services;

public record ImportResult(SpriteSheetDocument Document, SKBitmap CompositeImage);

public record AppendResult(IReadOnlyList<SpriteDefinition> NewSprites, SKBitmap ExpandedImage);

public static class ImageImporter
{
    public static async Task<ImportResult> LoadImagesAsync(
        IEnumerable<string> filePaths,
        int padding = 0,
        PackingLayout layout = PackingLayout.Grid)
    {
        var packingItems = await LoadImagesFromFilesAsync(filePaths);
        if (packingItems.Count == 0)
        {
            throw new InvalidOperationException("No valid images to load.");
        }

        SKBitmap compositeImage;
        SpriteSheetDocument document;

        if (packingItems.Count == 1)
        {
            var item = packingItems[0];
            compositeImage = item.Image.Copy();
            document = new SpriteSheetDocument { SpriteSheetName = item.Name };
            document.Sprites.Add(new SpriteDefinition
            {
                Name = item.Name,
                X = 0,
                Y = 0,
                Width = item.Width,
                Height = item.Height
            });
        }
        else
        {
            var packedResult = BinPacker.Pack(packingItems, padding, layout);
            compositeImage = CreateCompositeImage(packedResult);
            document = CreateDocument(packedResult);
        }

        foreach (var item in packingItems)
        {
            item.Image.Dispose();
        }

        return new ImportResult(document, compositeImage);
    }

    public static async Task<AppendResult> AppendImagesAsync(
        IEnumerable<string> filePaths,
        SKBitmap existingImage,
        IEnumerable<SpriteDefinition> existingSprites,
        int padding = 0,
        PackingLayout layout = PackingLayout.Grid)
    {
        var packingItems = await LoadImagesFromFilesAsync(filePaths);
        if (packingItems.Count == 0)
        {
            throw new InvalidOperationException("No valid images to import.");
        }

        var offsetX = existingSprites.Any()
            ? existingSprites.Max(s => s.X + s.Width)
            : 0;
        int appendWidth;
        int appendHeight;
        IReadOnlyList<PackedItem> packedItems;

        if (packingItems.Count == 1)
        {
            var item = packingItems[0];
            appendWidth = item.Width;
            appendHeight = item.Height;
            packedItems = [new PackedItem(item.Name, 0, 0, item.Width, item.Height, item.Image)];
        }
        else
        {
            var packedResult = BinPacker.Pack(packingItems, padding, layout);
            appendWidth = packedResult.CanvasWidth;
            appendHeight = packedResult.CanvasHeight;
            packedItems = packedResult.Items;
        }

        var newWidth = offsetX + appendWidth;
        var newHeight = Math.Max(existingImage.Height, appendHeight);

        var expandedImage = new SKBitmap(newWidth, newHeight, SKColorType.Bgra8888, SKAlphaType.Premul);
        using var canvas = new SKCanvas(expandedImage);
        canvas.Clear(SKColors.Transparent);
        canvas.DrawBitmap(existingImage, 0, 0);

        var newSprites = new List<SpriteDefinition>();
        foreach (var item in packedItems)
        {
            var adjustedX = item.X + offsetX;
            canvas.DrawBitmap(item.Image, adjustedX, item.Y);
            newSprites.Add(new SpriteDefinition
            {
                Name = item.Name,
                X = adjustedX,
                Y = item.Y,
                Width = item.Width,
                Height = item.Height
            });
        }

        foreach (var item in packingItems)
        {
            item.Image.Dispose();
        }

        return new AppendResult(newSprites, expandedImage);
    }

    private static async Task<List<PackingItem>> LoadImagesFromFilesAsync(IEnumerable<string> filePaths)
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
            SpriteSheetName = "spritesheet"
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

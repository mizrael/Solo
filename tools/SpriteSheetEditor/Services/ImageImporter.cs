using SkiaSharp;
using SpriteSheetEditor.Models;

namespace SpriteSheetEditor.Services;

public record ImportResult(SpriteSheetDocument Document, SKBitmap CompositeImage);

public record AppendResult(IReadOnlyList<SpriteDefinition> AllSprites, SKBitmap Image);

public record RearrangeResult(IReadOnlyList<SpriteDefinition> Sprites, SKBitmap Image);

public static class ImageImporter
{
    public static async Task<ImportResult> LoadImagesAsync(
        IEnumerable<string> filePaths,
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
            var packedResult = BinPacker.Pack(packingItems, layout);
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
        IEnumerable<SpriteDefinition> existingSprites)
    {
        var existingSpritesList = existingSprites.ToList();
        var existingNames = new HashSet<string>(existingSpritesList.Select(s => s.Name));
        var newPackingItems = await LoadImagesFromFilesAsync(filePaths, existingNames);
        if (newPackingItems.Count == 0)
        {
            throw new InvalidOperationException("No valid images to import.");
        }

        // Extract existing sprites as packing items
        var allPackingItems = new List<PackingItem>();
        foreach (var sprite in existingSpritesList)
        {
            var spriteBitmap = new SKBitmap(sprite.Width, sprite.Height, SKColorType.Bgra8888, SKAlphaType.Premul);
            using var canvas = new SKCanvas(spriteBitmap);
            canvas.Clear(SKColors.Transparent);
            canvas.DrawBitmap(existingImage,
                new SKRect(sprite.X, sprite.Y, sprite.X + sprite.Width, sprite.Y + sprite.Height),
                new SKRect(0, 0, sprite.Width, sprite.Height));
            allPackingItems.Add(new PackingItem(sprite.Name, sprite.Width, sprite.Height, spriteBitmap));
        }

        // Add new images
        allPackingItems.AddRange(newPackingItems);

        // Pack everything together using Grid layout
        var packedResult = BinPacker.Pack(allPackingItems, PackingLayout.Grid);

        // Create new composite image
        var newImage = CreateCompositeImage(packedResult);

        // Create sprite definitions for all sprites
        var allSprites = packedResult.Items.Select(item => new SpriteDefinition
        {
            Name = item.Name,
            X = item.X,
            Y = item.Y,
            Width = item.Width,
            Height = item.Height
        }).ToList();

        // Dispose extracted bitmaps
        foreach (var item in allPackingItems)
        {
            item.Image.Dispose();
        }

        return new AppendResult(allSprites, newImage);
    }

    public static RearrangeResult RearrangeLayout(
        SKBitmap sourceImage,
        IEnumerable<SpriteDefinition> sprites,
        PackingLayout layout = PackingLayout.Grid)
    {
        var spriteList = sprites.ToList();
        if (spriteList.Count == 0)
        {
            throw new InvalidOperationException("No sprites to rearrange.");
        }

        var packingItems = new List<PackingItem>();

        try
        {
            foreach (var sprite in spriteList)
            {
                var spriteBitmap = new SKBitmap(sprite.Width, sprite.Height, SKColorType.Bgra8888, SKAlphaType.Premul);
                using var canvas = new SKCanvas(spriteBitmap);
                canvas.Clear(SKColors.Transparent);
                canvas.DrawBitmap(sourceImage,
                    new SKRect(sprite.X, sprite.Y, sprite.X + sprite.Width, sprite.Y + sprite.Height),
                    new SKRect(0, 0, sprite.Width, sprite.Height));
                packingItems.Add(new PackingItem(sprite.Name, sprite.Width, sprite.Height, spriteBitmap));
            }

            var packedResult = BinPacker.Pack(packingItems, layout);
            var newImage = CreateCompositeImage(packedResult);
            var newSprites = packedResult.Items.Select(item => new SpriteDefinition
            {
                Name = item.Name,
                X = item.X,
                Y = item.Y,
                Width = item.Width,
                Height = item.Height
            }).ToList();

            return new RearrangeResult(newSprites, newImage);
        }
        finally
        {
            foreach (var item in packingItems)
                item.Image.Dispose();
        }
    }

    private static async Task<List<PackingItem>> LoadImagesFromFilesAsync(
        IEnumerable<string> filePaths,
        HashSet<string>? existingNames = null)
    {
        var items = new List<PackingItem>();
        var usedNames = existingNames ?? [];

        foreach (var filePath in filePaths)
        {
            try
            {
                var bytes = await File.ReadAllBytesAsync(filePath);
                var bitmap = SKBitmap.Decode(bytes);
                if (bitmap is not null)
                {
                    var baseName = Path.GetFileNameWithoutExtension(filePath);
                    var uniqueName = GetUniqueName(baseName, usedNames);
                    items.Add(new PackingItem(uniqueName, bitmap.Width, bitmap.Height, bitmap));
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

    private static string GetUniqueName(string baseName, HashSet<string> usedNames)
    {
        if (usedNames.Add(baseName))
        {
            return baseName;
        }

        var counter = 1;
        string candidateName;
        do
        {
            candidateName = $"{baseName}_{counter}";
            counter++;
        } while (!usedNames.Add(candidateName));

        return candidateName;
    }
}

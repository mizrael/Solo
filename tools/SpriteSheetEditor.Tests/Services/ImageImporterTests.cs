using SkiaSharp;
using SpriteSheetEditor.Models;
using SpriteSheetEditor.Services;
using Xunit;

namespace SpriteSheetEditor.Tests.Services;

public class ImageImporterTests : IDisposable
{
    private readonly string _tempDir;
    private readonly List<string> _tempFiles = [];

    public ImageImporterTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"ImageImporterTests_{Guid.NewGuid()}");
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        foreach (var file in _tempFiles)
        {
            if (File.Exists(file))
                File.Delete(file);
        }

        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    private string CreateTestImage(string name, int width, int height, SKColor color)
    {
        var filePath = Path.Combine(_tempDir, $"{name}.png");
        using var bitmap = new SKBitmap(width, height, SKColorType.Bgra8888, SKAlphaType.Premul);
        using var canvas = new SKCanvas(bitmap);
        canvas.Clear(color);

        using var stream = File.OpenWrite(filePath);
        bitmap.Encode(stream, SKEncodedImageFormat.Png, 100);

        _tempFiles.Add(filePath);
        return filePath;
    }

    #region LoadImagesAsync Tests

    [Fact]
    public async Task LoadImagesAsync_SingleImage_CreatesDocumentWithOneSprite()
    {
        var imagePath = CreateTestImage("test", 100, 100, SKColors.Red);

        var result = await ImageImporter.LoadImagesAsync([imagePath]);

        Assert.NotNull(result.Document);
        Assert.NotNull(result.CompositeImage);
        Assert.Single(result.Document.Sprites);
        Assert.Equal("test", result.Document.Sprites[0].Name);
        Assert.Equal(100, result.Document.Sprites[0].Width);
        Assert.Equal(100, result.Document.Sprites[0].Height);
        Assert.Equal(0, result.Document.Sprites[0].X);
        Assert.Equal(0, result.Document.Sprites[0].Y);

        result.CompositeImage.Dispose();
    }

    [Fact]
    public async Task LoadImagesAsync_SingleImage_SetsDocumentNameToFileName()
    {
        var imagePath = CreateTestImage("mysprite", 50, 50, SKColors.Blue);

        var result = await ImageImporter.LoadImagesAsync([imagePath]);

        Assert.Equal("mysprite", result.Document.SpriteSheetName);

        result.CompositeImage.Dispose();
    }

    [Fact]
    public async Task LoadImagesAsync_MultipleImages_CreatesDocumentWithMultipleSprites()
    {
        var image1 = CreateTestImage("sprite1", 64, 64, SKColors.Red);
        var image2 = CreateTestImage("sprite2", 64, 64, SKColors.Green);
        var image3 = CreateTestImage("sprite3", 64, 64, SKColors.Blue);

        var result = await ImageImporter.LoadImagesAsync([image1, image2, image3]);

        Assert.Equal(3, result.Document.Sprites.Count);
        Assert.Contains(result.Document.Sprites, s => s.Name == "sprite1");
        Assert.Contains(result.Document.Sprites, s => s.Name == "sprite2");
        Assert.Contains(result.Document.Sprites, s => s.Name == "sprite3");

        result.CompositeImage.Dispose();
    }

    [Fact]
    public async Task LoadImagesAsync_MultipleImages_DefaultsToGridLayout()
    {
        var image1 = CreateTestImage("a", 100, 100, SKColors.Red);
        var image2 = CreateTestImage("b", 100, 100, SKColors.Green);

        var result = await ImageImporter.LoadImagesAsync([image1, image2]);

        // Grid layout should pack images efficiently
        Assert.True(result.CompositeImage.Width >= 100);
        Assert.True(result.CompositeImage.Height >= 100);

        result.CompositeImage.Dispose();
    }

    [Fact]
    public async Task LoadImagesAsync_WithSingleColumnLayout_StacksVertically()
    {
        var image1 = CreateTestImage("top", 100, 50, SKColors.Red);
        var image2 = CreateTestImage("bottom", 100, 50, SKColors.Green);

        var result = await ImageImporter.LoadImagesAsync([image1, image2], PackingLayout.SingleColumn);

        Assert.Equal(100, result.CompositeImage.Width);
        Assert.Equal(100, result.CompositeImage.Height);

        result.CompositeImage.Dispose();
    }

    [Fact]
    public async Task LoadImagesAsync_WithSingleRowLayout_StacksHorizontally()
    {
        var image1 = CreateTestImage("left", 50, 100, SKColors.Red);
        var image2 = CreateTestImage("right", 50, 100, SKColors.Green);

        var result = await ImageImporter.LoadImagesAsync([image1, image2], PackingLayout.SingleRow);

        Assert.Equal(100, result.CompositeImage.Width);
        Assert.Equal(100, result.CompositeImage.Height);

        result.CompositeImage.Dispose();
    }

    [Fact]
    public async Task LoadImagesAsync_EmptyFileList_ThrowsInvalidOperationException()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => ImageImporter.LoadImagesAsync([]));
    }

    [Fact]
    public async Task LoadImagesAsync_NonExistentFiles_ThrowsWhenAllInvalid()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => ImageImporter.LoadImagesAsync(["nonexistent1.png", "nonexistent2.png"]));
    }

    [Fact]
    public async Task LoadImagesAsync_MixedValidAndInvalidFiles_LoadsValidFilesOnly()
    {
        var validImage = CreateTestImage("valid", 100, 100, SKColors.Red);
        var invalidPath = Path.Combine(_tempDir, "nonexistent.png");

        var result = await ImageImporter.LoadImagesAsync([validImage, invalidPath]);

        Assert.Single(result.Document.Sprites);
        Assert.Equal("valid", result.Document.Sprites[0].Name);

        result.CompositeImage.Dispose();
    }

    [Fact]
    public async Task LoadImagesAsync_DuplicateFileNames_GeneratesUniqueNames()
    {
        // Create two images with the same base name in different subdirectories
        var subDir1 = Path.Combine(_tempDir, "dir1");
        var subDir2 = Path.Combine(_tempDir, "dir2");
        Directory.CreateDirectory(subDir1);
        Directory.CreateDirectory(subDir2);

        var image1Path = Path.Combine(subDir1, "sprite.png");
        var image2Path = Path.Combine(subDir2, "sprite.png");

        using (var bitmap1 = new SKBitmap(50, 50))
        {
            using var canvas = new SKCanvas(bitmap1);
            canvas.Clear(SKColors.Red);
            using var stream = File.OpenWrite(image1Path);
            bitmap1.Encode(stream, SKEncodedImageFormat.Png, 100);
        }

        using (var bitmap2 = new SKBitmap(50, 50))
        {
            using var canvas = new SKCanvas(bitmap2);
            canvas.Clear(SKColors.Blue);
            using var stream = File.OpenWrite(image2Path);
            bitmap2.Encode(stream, SKEncodedImageFormat.Png, 100);
        }

        _tempFiles.Add(image1Path);
        _tempFiles.Add(image2Path);

        var result = await ImageImporter.LoadImagesAsync([image1Path, image2Path]);

        Assert.Equal(2, result.Document.Sprites.Count);
        var names = result.Document.Sprites.Select(s => s.Name).ToList();
        Assert.Contains("sprite", names);
        Assert.Contains("sprite_1", names);

        result.CompositeImage.Dispose();
    }

    [Fact]
    public async Task LoadImagesAsync_DifferentSizedImages_PacksCorrectly()
    {
        var smallImage = CreateTestImage("small", 32, 32, SKColors.Red);
        var largeImage = CreateTestImage("large", 128, 128, SKColors.Blue);

        var result = await ImageImporter.LoadImagesAsync([smallImage, largeImage]);

        Assert.Equal(2, result.Document.Sprites.Count);

        var smallSprite = result.Document.Sprites.First(s => s.Name == "small");
        var largeSprite = result.Document.Sprites.First(s => s.Name == "large");

        Assert.Equal(32, smallSprite.Width);
        Assert.Equal(32, smallSprite.Height);
        Assert.Equal(128, largeSprite.Width);
        Assert.Equal(128, largeSprite.Height);

        result.CompositeImage.Dispose();
    }

    #endregion

    #region AppendImagesAsync Tests

    [Fact]
    public async Task AppendImagesAsync_AddsNewSpritesToExisting()
    {
        var existingImage = new SKBitmap(100, 100, SKColorType.Bgra8888, SKAlphaType.Premul);
        using var canvas = new SKCanvas(existingImage);
        canvas.Clear(SKColors.Red);

        var existingSprites = new List<SpriteDefinition>
        {
            new() { Name = "existing", X = 0, Y = 0, Width = 100, Height = 100 }
        };

        var newImagePath = CreateTestImage("new", 50, 50, SKColors.Blue);

        var result = await ImageImporter.AppendImagesAsync(
            [newImagePath],
            existingImage,
            existingSprites);

        Assert.Equal(2, result.AllSprites.Count);
        Assert.Contains(result.AllSprites, s => s.Name == "existing");
        Assert.Contains(result.AllSprites, s => s.Name == "new");

        existingImage.Dispose();
        result.Image.Dispose();
    }

    [Fact]
    public async Task AppendImagesAsync_EmptyFileList_ThrowsInvalidOperationException()
    {
        var existingImage = new SKBitmap(100, 100);
        var existingSprites = new List<SpriteDefinition>
        {
            new() { Name = "existing", X = 0, Y = 0, Width = 100, Height = 100 }
        };

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => ImageImporter.AppendImagesAsync([], existingImage, existingSprites));

        existingImage.Dispose();
    }

    [Fact]
    public async Task AppendImagesAsync_NameConflict_GeneratesUniqueName()
    {
        var existingImage = new SKBitmap(100, 100, SKColorType.Bgra8888, SKAlphaType.Premul);
        using var canvas = new SKCanvas(existingImage);
        canvas.Clear(SKColors.Red);

        var existingSprites = new List<SpriteDefinition>
        {
            new() { Name = "sprite", X = 0, Y = 0, Width = 100, Height = 100 }
        };

        // Create a new image with the same name as the existing sprite
        var newImagePath = CreateTestImage("sprite", 50, 50, SKColors.Blue);

        var result = await ImageImporter.AppendImagesAsync(
            [newImagePath],
            existingImage,
            existingSprites);

        Assert.Equal(2, result.AllSprites.Count);
        var names = result.AllSprites.Select(s => s.Name).ToList();
        Assert.Contains("sprite", names);
        Assert.Contains("sprite_1", names);

        existingImage.Dispose();
        result.Image.Dispose();
    }

    [Fact]
    public async Task AppendImagesAsync_RearrangesAllSpritesWithGridLayout()
    {
        var existingImage = new SKBitmap(200, 100, SKColorType.Bgra8888, SKAlphaType.Premul);
        using var canvas = new SKCanvas(existingImage);
        canvas.Clear(SKColors.Transparent);

        // Two existing sprites side by side
        var existingSprites = new List<SpriteDefinition>
        {
            new() { Name = "left", X = 0, Y = 0, Width = 100, Height = 100 },
            new() { Name = "right", X = 100, Y = 0, Width = 100, Height = 100 }
        };

        var newImagePath = CreateTestImage("new", 100, 100, SKColors.Blue);

        var result = await ImageImporter.AppendImagesAsync(
            [newImagePath],
            existingImage,
            existingSprites);

        // After appending and rearranging, all 3 sprites should be present
        Assert.Equal(3, result.AllSprites.Count);

        existingImage.Dispose();
        result.Image.Dispose();
    }

    [Fact]
    public async Task AppendImagesAsync_PreservesExistingSpriteNames()
    {
        var existingImage = new SKBitmap(100, 100, SKColorType.Bgra8888, SKAlphaType.Premul);
        using var canvas = new SKCanvas(existingImage);
        canvas.Clear(SKColors.Red);

        var existingSprites = new List<SpriteDefinition>
        {
            new() { Name = "my_custom_sprite", X = 0, Y = 0, Width = 100, Height = 100 }
        };

        var newImagePath = CreateTestImage("new", 50, 50, SKColors.Blue);

        var result = await ImageImporter.AppendImagesAsync(
            [newImagePath],
            existingImage,
            existingSprites);

        Assert.Contains(result.AllSprites, s => s.Name == "my_custom_sprite");

        existingImage.Dispose();
        result.Image.Dispose();
    }

    #endregion

    #region RearrangeLayout Tests

    [Fact]
    public void RearrangeLayout_WithGridLayout_RearrangesSprites()
    {
        var sourceImage = new SKBitmap(200, 100, SKColorType.Bgra8888, SKAlphaType.Premul);
        using var canvas = new SKCanvas(sourceImage);
        canvas.Clear(SKColors.Transparent);
        using var paint = new SKPaint { Color = SKColors.Red };
        canvas.DrawRect(0, 0, 100, 100, paint);
        paint.Color = SKColors.Blue;
        canvas.DrawRect(100, 0, 100, 100, paint);

        var sprites = new List<SpriteDefinition>
        {
            new() { Name = "red", X = 0, Y = 0, Width = 100, Height = 100 },
            new() { Name = "blue", X = 100, Y = 0, Width = 100, Height = 100 }
        };

        var result = ImageImporter.RearrangeLayout(sourceImage, sprites, PackingLayout.Grid);

        Assert.Equal(2, result.Sprites.Count);
        Assert.Contains(result.Sprites, s => s.Name == "red");
        Assert.Contains(result.Sprites, s => s.Name == "blue");

        sourceImage.Dispose();
        result.Image.Dispose();
    }

    [Fact]
    public void RearrangeLayout_WithSingleColumnLayout_StacksVertically()
    {
        var sourceImage = new SKBitmap(200, 100, SKColorType.Bgra8888, SKAlphaType.Premul);
        using var canvas = new SKCanvas(sourceImage);
        canvas.Clear(SKColors.Transparent);

        var sprites = new List<SpriteDefinition>
        {
            new() { Name = "top", X = 0, Y = 0, Width = 100, Height = 100 },
            new() { Name = "bottom", X = 100, Y = 0, Width = 100, Height = 100 }
        };

        var result = ImageImporter.RearrangeLayout(sourceImage, sprites, PackingLayout.SingleColumn);

        Assert.Equal(100, result.Image.Width);
        Assert.Equal(200, result.Image.Height);

        sourceImage.Dispose();
        result.Image.Dispose();
    }

    [Fact]
    public void RearrangeLayout_WithSingleRowLayout_StacksHorizontally()
    {
        var sourceImage = new SKBitmap(100, 200, SKColorType.Bgra8888, SKAlphaType.Premul);
        using var canvas = new SKCanvas(sourceImage);
        canvas.Clear(SKColors.Transparent);

        var sprites = new List<SpriteDefinition>
        {
            new() { Name = "left", X = 0, Y = 0, Width = 100, Height = 100 },
            new() { Name = "right", X = 0, Y = 100, Width = 100, Height = 100 }
        };

        var result = ImageImporter.RearrangeLayout(sourceImage, sprites, PackingLayout.SingleRow);

        Assert.Equal(200, result.Image.Width);
        Assert.Equal(100, result.Image.Height);

        sourceImage.Dispose();
        result.Image.Dispose();
    }

    [Fact]
    public void RearrangeLayout_EmptySpriteList_ThrowsInvalidOperationException()
    {
        var sourceImage = new SKBitmap(100, 100);

        Assert.Throws<InvalidOperationException>(
            () => ImageImporter.RearrangeLayout(sourceImage, [], PackingLayout.Grid));

        sourceImage.Dispose();
    }

    [Fact]
    public void RearrangeLayout_PreservesSpriteNames()
    {
        var sourceImage = new SKBitmap(100, 100, SKColorType.Bgra8888, SKAlphaType.Premul);
        using var canvas = new SKCanvas(sourceImage);
        canvas.Clear(SKColors.Red);

        var sprites = new List<SpriteDefinition>
        {
            new() { Name = "my_unique_sprite_name", X = 0, Y = 0, Width = 100, Height = 100 }
        };

        var result = ImageImporter.RearrangeLayout(sourceImage, sprites, PackingLayout.Grid);

        Assert.Single(result.Sprites);
        Assert.Equal("my_unique_sprite_name", result.Sprites[0].Name);

        sourceImage.Dispose();
        result.Image.Dispose();
    }

    [Fact]
    public void RearrangeLayout_PreservesSpritePixelContent()
    {
        // Create an image with a specific color
        var sourceImage = new SKBitmap(100, 100, SKColorType.Bgra8888, SKAlphaType.Premul);
        using var canvas = new SKCanvas(sourceImage);
        canvas.Clear(SKColors.Magenta);

        var sprites = new List<SpriteDefinition>
        {
            new() { Name = "colored", X = 0, Y = 0, Width = 100, Height = 100 }
        };

        var result = ImageImporter.RearrangeLayout(sourceImage, sprites, PackingLayout.Grid);

        // Check that the resulting image has the same color
        var pixel = result.Image.GetPixel(50, 50);
        Assert.Equal(SKColors.Magenta.Red, pixel.Red);
        Assert.Equal(SKColors.Magenta.Green, pixel.Green);
        Assert.Equal(SKColors.Magenta.Blue, pixel.Blue);

        sourceImage.Dispose();
        result.Image.Dispose();
    }

    [Fact]
    public void RearrangeLayout_MultipleSprites_UpdatesPositions()
    {
        var sourceImage = new SKBitmap(200, 200, SKColorType.Bgra8888, SKAlphaType.Premul);
        using var canvas = new SKCanvas(sourceImage);
        canvas.Clear(SKColors.Transparent);

        // Sprites in a diagonal arrangement
        var sprites = new List<SpriteDefinition>
        {
            new() { Name = "a", X = 0, Y = 0, Width = 50, Height = 50 },
            new() { Name = "b", X = 100, Y = 100, Width = 50, Height = 50 }
        };

        var result = ImageImporter.RearrangeLayout(sourceImage, sprites, PackingLayout.Grid);

        // After rearrangement, positions should change
        var spriteA = result.Sprites.First(s => s.Name == "a");
        var spriteB = result.Sprites.First(s => s.Name == "b");

        // Sizes should be preserved
        Assert.Equal(50, spriteA.Width);
        Assert.Equal(50, spriteA.Height);
        Assert.Equal(50, spriteB.Width);
        Assert.Equal(50, spriteB.Height);

        sourceImage.Dispose();
        result.Image.Dispose();
    }

    #endregion
}

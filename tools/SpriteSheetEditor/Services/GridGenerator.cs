using SpriteSheetEditor.Models;

namespace SpriteSheetEditor.Services;

public static class GridGenerator
{
    public static SpriteSheetDocument Generate(string sheetName, int imageWidth, int imageHeight, int columns, int rows)
    {
        var doc = new SpriteSheetDocument { SpriteSheetName = sheetName };
        var sprites = GenerateSprites(sheetName, imageWidth, imageHeight, columns, rows);
        foreach (var sprite in sprites)
        {
            doc.Sprites.Add(sprite);
        }
        return doc;
    }

    public static List<SpriteDefinition> GenerateSprites(string sheetName, int imageWidth, int imageHeight, int columns, int rows)
    {
        var (tileWidth, tileHeight) = CalculateTileSize(imageWidth, imageHeight, columns, rows);
        var sprites = new List<SpriteDefinition>();
        var index = 0;

        for (var row = 0; row < rows; row++)
        {
            for (var col = 0; col < columns; col++)
            {
                sprites.Add(new SpriteDefinition
                {
                    Name = $"{sheetName}_sprite_{index}",
                    X = col * tileWidth,
                    Y = row * tileHeight,
                    Width = tileWidth,
                    Height = tileHeight
                });
                index++;
            }
        }

        return sprites;
    }

    public static (int width, int height) CalculateTileSize(int imageWidth, int imageHeight, int columns, int rows)
    {
        return (imageWidth / columns, imageHeight / rows);
    }

    public static (int uncoveredX, int uncoveredY) GetUncoveredPixels(int imageWidth, int imageHeight, int columns, int rows)
    {
        return (imageWidth % columns, imageHeight % rows);
    }

    public static bool HasUncoveredPixels(int imageWidth, int imageHeight, int columns, int rows)
    {
        var (uncoveredX, uncoveredY) = GetUncoveredPixels(imageWidth, imageHeight, columns, rows);
        return uncoveredX > 0 || uncoveredY > 0;
    }
}

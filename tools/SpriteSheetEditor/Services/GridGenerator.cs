using SpriteSheetEditor.Models;

namespace SpriteSheetEditor.Services;

public static class GridGenerator
{
    public static SpriteSheetDocument Generate(string sheetName, int imageWidth, int imageHeight, int columns, int rows)
    {
        var (tileWidth, tileHeight) = CalculateTileSize(imageWidth, imageHeight, columns, rows);

        var doc = new SpriteSheetDocument { SpriteSheetName = sheetName };
        var index = 0;

        for (var row = 0; row < rows; row++)
        {
            for (var col = 0; col < columns; col++)
            {
                doc.Sprites.Add(new SpriteDefinition
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

        return doc;
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

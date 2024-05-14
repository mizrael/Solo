using Microsoft.Xna.Framework;

namespace Tetris.Components;

public sealed class Constants
{
    static Constants()
    {
        TileTextureData = new Color[TileTextureWidth * TileTextureHeight];

        var borderColor = Color.Green;
        var insideColor = Color.Red;

        for (int i = 0; i != TileTextureData.Length; i++)
            TileTextureData[i] = insideColor;

        for (int i = 0; i != TileTextureWidth; i++)
        {
            TileTextureData[i] = borderColor;
            TileTextureData[TileTextureData.Length - i - 1] = borderColor;
        }

        for (int i = 0; i < TileTextureData.Length; i += TileTextureWidth)
        {
            TileTextureData[i] = borderColor;
            TileTextureData[TileTextureWidth + i - 1] = borderColor;
        }

    }

    public const int TileTextureWidth = 64;
    public const int TileTextureHeight = 64;
    public static Color[] TileTextureData { get; private set; }
}
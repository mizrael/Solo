using Microsoft.Xna.Framework;

namespace Tetris.Components;

public sealed class Constants
{
    static Constants()
    {
        TileTextureData = new Color[TileTextureWidth * TileTextureHeight];

        for (int i = 0; i != TileTextureData.Length; i++)
            TileTextureData[i] = Color.White;
    }

    public const int TileTextureWidth = 1;
    public const int TileTextureHeight = 1;
    public static Color[] TileTextureData { get; private set; }
}
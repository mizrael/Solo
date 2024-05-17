using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace Tetris.Components;

public sealed class Constants
{
    static Constants()
    {
        var borderWidth = 1;
        TileTextureData = new Color[TileTextureSize.Width * TileTextureSize.Height];

        for (int y = 0; y < TileTextureSize.Height; y++)
        {
            for (int x = 0; x < TileTextureSize.Width; x++)
            {
                var index = y * TileTextureSize.Width + x;
                var color = (x < borderWidth || y < borderWidth || x >= TileTextureSize.Width - borderWidth || y >= TileTextureSize.Height - borderWidth)
                            ? Color.Black : Color.White;
                TileTextureData[index] = color;
            }
        }
    }

    public static readonly Rectangle TileTextureSize = new Rectangle(0, 0, 16, 16);
    public static readonly Color[] TileTextureData;
}
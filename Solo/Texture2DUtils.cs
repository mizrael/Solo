using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Solo;

public static class Texture2DUtils
{
    public static Texture2D Generate(GraphicsDevice graphicsDevice, int width, int height, Color color)
    {
        var data = new Color[width * height];
        for (int i = 0; i < data.Length; ++i)
            data[i] = color;

        return Generate(graphicsDevice, width, height, data);
    }

    public static Texture2D Generate(GraphicsDevice graphicsDevice, int width, int height, Color[] data)
    {
        var texture = new Texture2D(graphicsDevice, width, height);
        texture.SetData(data);
        return texture;
    }
}
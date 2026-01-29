using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Buffers;

namespace Solo.Utils;

public static class GraphicsDeviceExtensions
{
    public static Texture2D Generate(this GraphicsDevice graphicsDevice, int width, int height, Color color)
    {
        var size = width * height;
        var data = ArrayPool<Color>.Shared.Rent(size);
        Array.Fill(data, color, 0, size);
        var texture = new Texture2D(graphicsDevice, width, height);
        texture.SetData(data, 0, size);

        ArrayPool<Color>.Shared.Return(data);

        return texture;
    }

    public static Texture2D Generate(this GraphicsDevice graphicsDevice, int width, int height, Color[] data)
    {
        var texture = new Texture2D(graphicsDevice, width, height);
        texture.SetData(data);
        return texture;
    }
}
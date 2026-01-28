using System.Buffers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Solo.Services;

namespace Solo.Utils;

//TODO: check if needs refactoring
public static class RenderServiceExtensions
{
    public static Texture2D CreateTexture(this RenderService renderService, int width, int height, Color color)
    {
        var texture = new Texture2D(GraphicsDeviceManagerAccessor.Instance.GraphicsDeviceManager.GraphicsDevice, width, height);
        var size = width * height;
        var data = ArrayPool<Color>.Shared.Rent(size);
        for (var i = 0; i < data.Length; i++)
            data[i] = color;
        
        texture.SetData(data, 0, size);

        ArrayPool<Color>.Shared.Return(data);

        return texture;
    }

    public static Texture2D CreateTexture(this RenderService renderService, int width, int height, Color[] data)
    {
        var texture = new Texture2D(GraphicsDeviceManagerAccessor.Instance.GraphicsDeviceManager.GraphicsDevice, width, height);
        
        texture.SetData(data);

        return texture;
    }
}

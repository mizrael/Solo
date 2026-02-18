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

    public static Texture2D GenerateCircle(this GraphicsDevice graphicsDevice, int size, Color color)
    {
        var pixelCount = size * size;
        var data = ArrayPool<Color>.Shared.Rent(pixelCount);
        var center = size * 0.5f;
        var radius = center - 1f;
        var radiusSq = radius * radius;

        for (var y = 0; y < size; y++)
        {
            for (var x = 0; x < size; x++)
            {
                var dx = x - center + 0.5f;
                var dy = y - center + 0.5f;
                data[y * size + x] = (dx * dx + dy * dy <= radiusSq)
                    ? color
                    : Color.Transparent;
            }
        }

        var texture = new Texture2D(graphicsDevice, size, size);
        texture.SetData(data, 0, pixelCount);
        ArrayPool<Color>.Shared.Return(data);
        return texture;
    }

    public static Texture2D GenerateRing(this GraphicsDevice graphicsDevice, int size, Color color, float thickness = 1.5f)
    {
        var pixelCount = size * size;
        var data = ArrayPool<Color>.Shared.Rent(pixelCount);
        var center = size * 0.5f;
        var outerRadius = center - 1f;
        var innerRadius = outerRadius - thickness;
        var outerRadiusSq = outerRadius * outerRadius;
        var innerRadiusSq = innerRadius * innerRadius;

        for (var y = 0; y < size; y++)
        {
            for (var x = 0; x < size; x++)
            {
                var dx = x - center + 0.5f;
                var dy = y - center + 0.5f;
                var distSq = dx * dx + dy * dy;
                data[y * size + x] = (distSq <= outerRadiusSq && distSq >= innerRadiusSq)
                    ? color
                    : Color.Transparent;
            }
        }

        var texture = new Texture2D(graphicsDevice, size, size);
        texture.SetData(data, 0, pixelCount);
        ArrayPool<Color>.Shared.Return(data);
        return texture;
    }
}
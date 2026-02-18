using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Solo.Utils;

namespace Solo.UI;

public static class UIResources
{
    private static Texture2D? _pixelTexture;

    public static Texture2D GetPixelTexture(GraphicsDevice graphicsDevice)
    {
        if (_pixelTexture == null || _pixelTexture.IsDisposed)
            _pixelTexture = graphicsDevice.Generate(1, 1, Color.White);

        return _pixelTexture;
    }
}

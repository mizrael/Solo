using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Solocaster.UI.Widgets;

public class PanelWidget : Widget
{
    private static Texture2D? _pixelTexture;

    public PanelWidget()
    {
    }

    public Color BackgroundColor { get; set; } = new Color(40, 40, 40, 220);
    public Color BorderColor { get; set; } = new Color(80, 80, 80);
    public int BorderWidth { get; set; } = 2;

    protected static Texture2D GetPixelTexture(GraphicsDevice graphicsDevice)
    {
        if (_pixelTexture == null)
        {
            _pixelTexture = new Texture2D(graphicsDevice, 1, 1);
            _pixelTexture.SetData(new[] { Color.White });
        }
        return _pixelTexture;
    }

    protected override void RenderCore(SpriteBatch spriteBatch)
    {
        var pixel = GetPixelTexture(spriteBatch.GraphicsDevice);
        var bounds = Bounds;

        // Draw background
        spriteBatch.Draw(pixel, bounds, BackgroundColor);

        // Draw border
        if (BorderWidth > 0)
        {
            // Top
            spriteBatch.Draw(pixel, new Rectangle(bounds.X, bounds.Y, bounds.Width, BorderWidth), BorderColor);
            // Bottom
            spriteBatch.Draw(pixel, new Rectangle(bounds.X, bounds.Bottom - BorderWidth, bounds.Width, BorderWidth), BorderColor);
            // Left
            spriteBatch.Draw(pixel, new Rectangle(bounds.X, bounds.Y, BorderWidth, bounds.Height), BorderColor);
            // Right
            spriteBatch.Draw(pixel, new Rectangle(bounds.Right - BorderWidth, bounds.Y, BorderWidth, bounds.Height), BorderColor);
        }
    }
}

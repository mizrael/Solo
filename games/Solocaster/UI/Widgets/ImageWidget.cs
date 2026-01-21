using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Solocaster.UI.Widgets;

public class ImageWidget : Widget
{
    public ImageWidget()
    {
    }

    public Texture2D? Texture { get; set; }
    public Rectangle? SourceRectangle { get; set; }
    public Color Tint { get; set; } = Color.White;
    public bool ScaleToFit { get; set; } = true;

    protected override void RenderCore(SpriteBatch spriteBatch)
    {
        if (Texture == null)
            return;

        var destRect = Bounds;

        if (!ScaleToFit)
        {
            var sourceSize = SourceRectangle?.Size ?? new Point(Texture.Width, Texture.Height);
            destRect = new Rectangle(
                (int)ScreenPosition.X,
                (int)ScreenPosition.Y,
                sourceSize.X,
                sourceSize.Y
            );
        }

        spriteBatch.Draw(
            Texture,
            destRect,
            SourceRectangle,
            Tint
        );
    }
}

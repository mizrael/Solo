using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Solo.UI.Widgets;

public class ImageWidget : Widget
{
    private Texture2D? _texture;

    public ImageWidget()
    {
    }

    public Texture2D? Texture
    {
        get => _texture;
        set
        {
            if (_texture != value)
            {
                _texture = value;
                InvalidateMeasure();
            }
        }
    }
    public Rectangle? SourceRectangle { get; set; }
    public Color Tint { get; set; } = Color.White;
    public bool ScaleToFit { get; set; } = true;

    protected override Vector2 MeasureCore(float availableWidth, float availableHeight)
    {
        if (ScaleToFit)
            return Size;

        if (_texture != null)
            return new Vector2(_texture.Width, _texture.Height);

        return Size;
    }

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

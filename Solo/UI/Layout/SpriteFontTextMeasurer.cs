using Microsoft.Xna.Framework.Graphics;
using System.Text;

namespace Solo.UI.Layout;

public sealed class SpriteFontTextMeasurer : ITextMeasurer
{
    public SpriteFontTextMeasurer(SpriteFont font)
    {
        Font = font ?? throw new ArgumentNullException(nameof(font));
    }

    public SpriteFont Font { get; }

    public float LineHeight => Font.LineSpacing;

    public float MeasureWidth(StringBuilder text) => Font.MeasureString(text).X;
}

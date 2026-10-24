using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Solo.UI.Layout;

namespace Solo.UI.Widgets;

/// <summary>
/// Label that wraps to the narrower of <see cref="MaxWidth"/> and the parent's offered width,
/// sizing itself to the wrapped text.
/// </summary>
public class WrappingLabelWidget : Widget
{
    private readonly WrappedText _wrapped = new();
    private readonly ITextMeasurer? _customMeasurer;
    private SpriteFontTextMeasurer? _fontMeasurer;
    private string _text = string.Empty;
    private SpriteFont? _font;
    private float _maxWidth;
    private int _maxLines;

    public WrappingLabelWidget()
    {
        ConsumesClicks = false;
    }

    /// <summary>Measures with <paramref name="measurer"/> instead of the font, e.g. a fake in tests.</summary>
    public WrappingLabelWidget(ITextMeasurer measurer) : this()
    {
        _customMeasurer = measurer ?? throw new ArgumentNullException(nameof(measurer));
    }

    public string Text
    {
        get => _text;
        set
        {
            value ??= string.Empty;
            if (_text == value)
                return;
            _text = value;
            InvalidateMeasure();
        }
    }

    /// <summary>Font used to draw and measure; falls back to <see cref="UITheme.Font"/>.</summary>
    public SpriteFont? Font
    {
        get => _font;
        set
        {
            if (_font == value)
                return;
            _font = value;
            InvalidateMeasure();
        }
    }

    /// <summary>Maximum wrap width; zero or less means only the parent's width applies.</summary>
    public float MaxWidth
    {
        get => _maxWidth;
        set
        {
            if (_maxWidth == value)
                return;
            _maxWidth = value;
            InvalidateMeasure();
        }
    }

    /// <summary>Maximum number of lines; zero or less means unlimited. Truncated text ends with an ellipsis.</summary>
    public int MaxLines
    {
        get => _maxLines;
        set
        {
            if (_maxLines == value)
                return;
            _maxLines = value;
            InvalidateMeasure();
        }
    }

    public Color TextColor { get; set; } = UITheme.Text.Primary;
    public bool CenterHorizontally { get; set; }

    public IReadOnlyList<string> Lines => _wrapped.Lines;

    /// <summary>Size of the wrapped text as of the last measure.</summary>
    public Vector2 TextSize => _wrapped.Size;

    private SpriteFont? ActiveFont => _font ?? UITheme.Font;

    protected override Vector2 MeasureCore(float availableWidth, float availableHeight)
    {
        var measurer = ResolveMeasurer();
        // No font is loaded in headless contexts such as unit tests.
        if (measurer == null)
            return Vector2.Zero;

        _wrapped.Update(_text, ResolveWrapWidth(availableWidth), _maxLines, measurer);
        return _wrapped.Size;
    }

    private float ResolveWrapWidth(float availableWidth)
    {
        if (_maxWidth > 0 && availableWidth > 0)
            return Math.Min(_maxWidth, availableWidth);
        return _maxWidth > 0 ? _maxWidth : availableWidth;
    }

    private ITextMeasurer? ResolveMeasurer()
    {
        if (_customMeasurer != null)
            return _customMeasurer;

        var font = ActiveFont;
        if (font == null)
            return null;
        if (_fontMeasurer?.Font != font)
            _fontMeasurer = new SpriteFontTextMeasurer(font);
        return _fontMeasurer;
    }

    protected override void RenderCore(SpriteBatch spriteBatch)
    {
        var font = ActiveFont;
        if (font == null)
            return;

        var lines = _wrapped.Lines;
        var widths = _wrapped.LineWidths;
        var position = ScreenPosition;
        for (int i = 0; i < lines.Count; i++)
        {
            var linePosition = position;
            if (CenterHorizontally)
                linePosition.X += (Size.X - widths[i]) / 2;
            spriteBatch.DrawString(font, lines[i], linePosition, TextColor);
            position.Y += font.LineSpacing;
        }
    }
}

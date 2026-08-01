using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace Solo.UI.Widgets;

public class ButtonWidget : PanelWidget
{
    private bool _isHovered;
    private string _text = string.Empty;

    public ButtonWidget()
    {
        ShowCloseButton = false;
        BackgroundColor = UITheme.Button.BackgroundColor;
        BorderColor = UITheme.Button.BorderColor;
        BorderWidth = UITheme.Button.BorderWidth;
    }

    public string Text
    {
        get => _text;
        set
        {
            _text = value;
            if (AutoSize)
                FitToText();
            InvalidateMeasure();
        }
    }

    public bool AutoSize { get; set; } = true;
    public Color TextColor { get; set; } = UITheme.Text.Primary;
    public Color HoverTextColor { get; set; } = UITheme.Text.Title;
    public Color HoverBackgroundColor { get; set; } = UITheme.Button.HoverBackgroundColor;
    public Color HoverBorderColor { get; set; } = UITheme.Button.HoverBorderColor;
    public Color DisabledTextColor { get; set; } = UITheme.Text.Muted;
    public Color DisabledBackgroundColor { get; set; } = UITheme.Button.DisabledBackgroundColor;
    public Color DisabledBorderColor { get; set; } = UITheme.Button.DisabledBorderColor;

    public bool IsHovered => _isHovered && Enabled;

    protected override void UpdateCore(GameTime gameTime, MouseState mouseState, MouseState previousMouseState)
    {
        var mousePoint = new Point(mouseState.X, mouseState.Y);
        _isHovered = Enabled && Bounds.Contains(mousePoint);

        base.UpdateCore(gameTime, mouseState, previousMouseState);
    }

    protected override void RenderCore(SpriteBatch spriteBatch)
    {
        var originalBackgroundColor = BackgroundColor;
        var originalBorderColor = BorderColor;

        if (!Enabled)
        {
            BackgroundColor = DisabledBackgroundColor;
            BorderColor = DisabledBorderColor;
        }
        else if (_isHovered)
        {
            BackgroundColor = HoverBackgroundColor;
            BorderColor = HoverBorderColor;
        }

        base.RenderCore(spriteBatch);

        BackgroundColor = originalBackgroundColor;
        BorderColor = originalBorderColor;

        // Draw text centered
        if (!string.IsNullOrEmpty(Text))
        {
            var textSize = UITheme.Font.MeasureString(Text);
            var textPos = ScreenPosition + (Size - textSize) / 2;
            var currentTextColor = !Enabled ? DisabledTextColor : (_isHovered ? HoverTextColor : TextColor);
            spriteBatch.DrawString(UITheme.Font, Text, textPos, currentTextColor);
        }
    }

    /// <summary>
    /// Computes the size a button needs. An authored size is treated as a floor rather
    /// than a ceiling, so a label can never render wider than the button drawn around it.
    /// </summary>
    public static Vector2 ComputeButtonSize(
        Vector2 textSize,
        Vector2 authoredSize,
        bool autoSize,
        int contentPadding,
        int borderWidth)
    {
        float chrome = contentPadding * 2f + borderWidth * 2f;
        var fitted = new Vector2(textSize.X + chrome, textSize.Y + chrome);

        if (autoSize)
            return fitted;

        // Width grows because that is the axis text overflows on in practice. Height
        // stays as authored so fixed-height button rows keep their alignment, falling
        // back to the fitted height only when no height was ever set.
        float height = authoredSize.Y > 0 ? authoredSize.Y : fitted.Y;
        return new Vector2(Math.Max(authoredSize.X, fitted.X), height);
    }

    protected override Vector2 MeasureCore(float availableWidth, float availableHeight)
    {
        if (string.IsNullOrEmpty(_text))
            return Size;

        // No font is loaded in headless contexts such as unit tests. Falling back to the
        // authored size keeps measurement side-effect free there rather than throwing.
        var font = UITheme.Font;
        if (font == null)
            return Size;

        return ComputeButtonSize(
            font.MeasureString(_text),
            Size,
            AutoSize,
            UITheme.Button.ContentPadding,
            BorderWidth);
    }

    private void FitToText()
    {
        if (string.IsNullOrEmpty(_text))
            return;

        var textSize = UITheme.Font.MeasureString(_text);
        var padding = UITheme.Button.ContentPadding;
        Size = new Vector2(
            textSize.X + (float)padding * 2f + (float)BorderWidth * 2f,
            textSize.Y + (float)padding * 2f + (float)BorderWidth * 2f
        );
    }

    protected override void OnMouseClick(Point mousePosition)
    {
        OnClick?.Invoke();
        base.OnMouseClick(mousePosition);
    }

    public event Action? OnClick;
}

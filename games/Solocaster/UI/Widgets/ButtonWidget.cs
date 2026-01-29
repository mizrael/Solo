using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Solocaster.UI.Widgets;

public class ButtonWidget : PanelWidget
{
    private bool _isHovered;

    public ButtonWidget()
    {
        ShowCloseButton = false;
        BackgroundColor = UITheme.Button.BackgroundColor;
        BorderColor = UITheme.Button.BorderColor;
        BorderWidth = UITheme.Button.BorderWidth;
    }

    public string Text { get; set; } = string.Empty;
    public SpriteFont? Font { get; set; }
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
        if (Font != null && !string.IsNullOrEmpty(Text))
        {
            var textSize = Font.MeasureString(Text);
            var textPos = ScreenPosition + (Size - textSize) / 2;
            var currentTextColor = !Enabled ? DisabledTextColor : (_isHovered ? HoverTextColor : TextColor);
            spriteBatch.DrawString(Font, Text, textPos, currentTextColor);
        }
    }

    protected override void OnMouseClick(Point mousePosition)
    {
        OnClick?.Invoke();
        base.OnMouseClick(mousePosition);
    }

    public event Action? OnClick;
}

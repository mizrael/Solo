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
    public Color HoverColor { get; set; } = UITheme.Selection.HoverBackground;

    protected override void UpdateCore(GameTime gameTime, MouseState mouseState, MouseState previousMouseState)
    {
        var mousePoint = new Point(mouseState.X, mouseState.Y);
        _isHovered = Bounds.Contains(mousePoint);

        base.UpdateCore(gameTime, mouseState, previousMouseState);
    }

    protected override void RenderCore(SpriteBatch spriteBatch)
    {
        var originalColor = BackgroundColor;
        if (_isHovered)
            BackgroundColor = HoverColor;

        base.RenderCore(spriteBatch);

        BackgroundColor = originalColor;

        // Draw text centered
        if (Font != null && !string.IsNullOrEmpty(Text))
        {
            var textSize = Font.MeasureString(Text);
            var textPos = ScreenPosition + (Size - textSize) / 2;
            spriteBatch.DrawString(Font, Text, textPos, TextColor);
        }
    }

    protected override void OnMouseClick(Point mousePosition)
    {
        OnClick?.Invoke();
        base.OnMouseClick(mousePosition);
    }

    public event Action? OnClick;
}

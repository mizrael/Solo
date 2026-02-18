using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace Solo.UI.Widgets;

public class MenuItemWidget : Widget
{
    private bool _isHovered;
    private bool _isSelected;
    private string _text = string.Empty;

    public string Text
    {
        get => _text;
        set
        {
            if (_text != value)
            {
                _text = value;
                InvalidateMeasure();
            }
        }
    }
    public Color TextColor { get; set; } = UITheme.Text.Primary;
    public Color HoverTextColor { get; set; } = UITheme.Text.Title;
    public Color SelectedTextColor { get; set; } = UITheme.Text.Title;
    public Color BackgroundColor { get; set; } = Color.Transparent;
    public Color HoverBackgroundColor { get; set; } = UITheme.Selection.HoverBackground;
    public Color SelectedBackgroundColor { get; set; } = UITheme.Selection.SelectedBackground;
    public int Padding { get; set; } = 8;
    public bool CenterHorizontally { get; set; }

    public bool IsSelected
    {
        get => _isSelected;
        set => _isSelected = value;
    }

    public event Action? OnClick;
    public event Action? OnHover;

    protected override Vector2 MeasureCore(float availableWidth, float availableHeight)
    {
        return new Vector2(availableWidth, UITheme.MenuItemHeight);
    }

    public void Click()
    {
        OnClick?.Invoke();
    }

    protected override void UpdateCore(GameTime gameTime, MouseState mouseState, MouseState previousMouseState)
    {
        var wasHovered = _isHovered;
        _isHovered = Bounds.Contains(mouseState.X, mouseState.Y);

        if (_isHovered && !wasHovered)
            OnHover?.Invoke();

        if (_isHovered &&
            mouseState.LeftButton == ButtonState.Pressed &&
            previousMouseState.LeftButton == ButtonState.Released)
        {
            OnClick?.Invoke();
        }
    }

    protected override void RenderCore(SpriteBatch spriteBatch)
    {
        var pixel = UIResources.GetPixelTexture(spriteBatch.GraphicsDevice);
        var bounds = Bounds;

        Color bgColor;
        Color textColor;

        if (_isSelected)
        {
            bgColor = SelectedBackgroundColor;
            textColor = SelectedTextColor;
        }
        else if (_isHovered)
        {
            bgColor = HoverBackgroundColor;
            textColor = HoverTextColor;
        }
        else
        {
            bgColor = BackgroundColor;
            textColor = TextColor;
        }

        if (bgColor != Color.Transparent)
            spriteBatch.Draw(pixel, bounds, bgColor);

        if (!string.IsNullOrEmpty(Text))
        {
            var textSize = UITheme.Font.MeasureString(Text);
            float textX = CenterHorizontally
                ? bounds.X + (bounds.Width - textSize.X) / 2
                : bounds.X + Padding;
            var textPos = new Vector2(
                textX,
                bounds.Y + (bounds.Height - textSize.Y) / 2
            );
            spriteBatch.DrawString(UITheme.Font, Text, textPos, textColor);
        }
    }

}

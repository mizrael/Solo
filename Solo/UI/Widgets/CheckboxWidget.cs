using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace Solo.UI.Widgets;

public class CheckboxWidget : Widget
{
    private bool _isHovered;
    private string _label = string.Empty;

    public string Label
    {
        get => _label;
        set
        {
            if (_label != value)
            {
                _label = value;
                InvalidateMeasure();
            }
        }
    }
    public bool Checked { get; set; }
    public Color TextColor { get; set; } = UITheme.Text.Primary;
    public Color BoxBorderColor { get; set; } = UITheme.Button.BorderColor;
    public Color BoxBackgroundColor { get; set; } = UITheme.Button.BackgroundColor;
    public Color CheckColor { get; set; } = UITheme.Text.Highlight;
    public Color HoverBorderColor { get; set; } = UITheme.Button.HoverBorderColor;

    public event Action<bool>? OnChanged;

    protected override Vector2 MeasureCore(float availableWidth, float availableHeight)
    {
        float boxSize = UITheme.LineHeight;
        float gap = 8;
        float textWidth = string.IsNullOrEmpty(_label) ? 0 : UITheme.Font.MeasureString(_label).X;
        float width = boxSize + gap + textWidth;
        float height = Math.Max(UITheme.LineHeight, UITheme.Font.LineSpacing);
        return new Vector2(width, height);
    }

    protected override void UpdateCore(GameTime gameTime, MouseState mouseState, MouseState previousMouseState)
    {
        _isHovered = Bounds.Contains(mouseState.X, mouseState.Y);

        if (_isHovered &&
            mouseState.LeftButton == ButtonState.Pressed &&
            previousMouseState.LeftButton == ButtonState.Released)
        {
            Checked = !Checked;
            OnChanged?.Invoke(Checked);
        }
    }

    protected override void RenderCore(SpriteBatch spriteBatch)
    {
        var pixel = UIResources.GetPixelTexture(spriteBatch.GraphicsDevice);
        var pos = ScreenPosition;
        int boxSize = UITheme.LineHeight;
        int border = 2;
        int padding = 8;
        float labelX = pos.X + boxSize + padding;
        float centerY = pos.Y + (Size.Y - boxSize) / 2;

        var borderColor = _isHovered ? HoverBorderColor : BoxBorderColor;

        // Box background
        var boxRect = new Rectangle((int)pos.X, (int)centerY, boxSize, boxSize);
        spriteBatch.Draw(pixel, boxRect, BoxBackgroundColor);

        // Box border
        spriteBatch.Draw(pixel, new Rectangle(boxRect.X, boxRect.Y, boxRect.Width, border), borderColor);
        spriteBatch.Draw(pixel, new Rectangle(boxRect.X, boxRect.Bottom - border, boxRect.Width, border), borderColor);
        spriteBatch.Draw(pixel, new Rectangle(boxRect.X, boxRect.Y, border, boxRect.Height), borderColor);
        spriteBatch.Draw(pixel, new Rectangle(boxRect.Right - border, boxRect.Y, border, boxRect.Height), borderColor);

        // Checkmark (drawn as two diagonal lines)
        if (Checked)
        {
            int inset = boxSize / 4;
            int thickness = Math.Max(2, boxSize / 8);
            int innerSize = boxSize - inset * 2;

            // Short leg of checkmark (bottom-left to mid)
            int shortLen = innerSize / 3;
            for (int i = 0; i < shortLen; i++)
            {
                int x = boxRect.X + inset + i;
                int y = boxRect.Y + inset + innerSize - shortLen + i;
                spriteBatch.Draw(pixel, new Rectangle(x, y, thickness, thickness), CheckColor);
            }

            // Long leg of checkmark (mid to top-right)
            int longLen = innerSize - shortLen;
            for (int i = 0; i <= longLen; i++)
            {
                int x = boxRect.X + inset + shortLen + i;
                int y = boxRect.Y + inset + innerSize - i;
                spriteBatch.Draw(pixel, new Rectangle(x, y, thickness, thickness), CheckColor);
            }
        }

        // Label text
        if (!string.IsNullOrEmpty(Label))
        {
            var textSize = UITheme.Font.MeasureString(Label);
            var textPos = new Vector2(labelX, pos.Y + (Size.Y - textSize.Y) / 2);
            spriteBatch.DrawString(UITheme.Font, Label, textPos, TextColor);
        }
    }

    protected override void OnMouseClick(Point mousePosition)
    {
    }
}

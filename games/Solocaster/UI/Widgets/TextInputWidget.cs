using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Solocaster.UI.Widgets;

public class TextInputWidget : PanelWidget
{
    private static Texture2D? _pixelTexture;
    private double _cursorBlinkTimer;
    private bool _cursorVisible = true;
    private const double CursorBlinkRate = 0.5;
    private KeyboardState _previousKeyboardState;

    public TextInputWidget()
    {
        ShowCloseButton = false;
        ContentPadding = 0;
        BackgroundColor = UITheme.ItemSlot.BackgroundColor;
        BorderColor = UITheme.ItemSlot.BorderColor;
        BorderWidth = UITheme.ItemSlot.BorderWidth;
    }

    public string Text { get; set; } = string.Empty;
    public int MaxLength { get; set; } = 20;
    public SpriteFont? Font { get; set; }
    public Color TextColor { get; set; } = UITheme.Text.Primary;
    public Color PlaceholderColor { get; set; } = UITheme.Text.Placeholder;
    public Color CursorColor { get; set; } = UITheme.Text.Highlight;
    public string PlaceholderText { get; set; } = "Enter name...";
    public bool IsFocused { get; set; } = true;
    public int Padding { get; set; } = 8;

    public event Action<string>? OnTextChanged;

    private static Texture2D GetInputPixelTexture(GraphicsDevice graphicsDevice)
    {
        if (_pixelTexture == null)
        {
            _pixelTexture = new Texture2D(graphicsDevice, 1, 1);
            _pixelTexture.SetData(new[] { Color.White });
        }
        return _pixelTexture;
    }

    protected override void UpdateCore(GameTime gameTime, MouseState mouseState, MouseState previousMouseState)
    {
        base.UpdateCore(gameTime, mouseState, previousMouseState);

        if (!IsFocused)
            return;

        // Cursor blink
        _cursorBlinkTimer += gameTime.ElapsedGameTime.TotalSeconds;
        if (_cursorBlinkTimer >= CursorBlinkRate)
        {
            _cursorBlinkTimer = 0;
            _cursorVisible = !_cursorVisible;
        }

        // Handle keyboard input
        var keyboardState = Keyboard.GetState();
        var pressedKeys = keyboardState.GetPressedKeys();

        foreach (var key in pressedKeys)
        {
            if (!_previousKeyboardState.IsKeyDown(key))
            {
                HandleKeyPress(key, keyboardState);
            }
        }

        _previousKeyboardState = keyboardState;
    }

    private void HandleKeyPress(Keys key, KeyboardState keyboardState)
    {
        bool shift = keyboardState.IsKeyDown(Keys.LeftShift) || keyboardState.IsKeyDown(Keys.RightShift);

        if (key == Keys.Back && Text.Length > 0)
        {
            Text = Text[..^1];
            OnTextChanged?.Invoke(Text);
            _cursorVisible = true;
            _cursorBlinkTimer = 0;
        }
        else if (key == Keys.Delete)
        {
            Text = string.Empty;
            OnTextChanged?.Invoke(Text);
        }
        else if (Text.Length < MaxLength)
        {
            char? c = KeyToChar(key, shift);
            if (c.HasValue)
            {
                Text += c.Value;
                OnTextChanged?.Invoke(Text);
                _cursorVisible = true;
                _cursorBlinkTimer = 0;
            }
        }
    }

    private static char? KeyToChar(Keys key, bool shift)
    {
        // Letters
        if (key >= Keys.A && key <= Keys.Z)
        {
            char c = (char)('a' + (key - Keys.A));
            return shift ? char.ToUpper(c) : c;
        }

        // Numbers
        if (key >= Keys.D0 && key <= Keys.D9 && !shift)
            return (char)('0' + (key - Keys.D0));

        // Space
        if (key == Keys.Space)
            return ' ';

        // Common punctuation
        if (key == Keys.OemMinus)
            return shift ? '_' : '-';
        if (key == Keys.OemPeriod)
            return '.';

        return null;
    }

    protected override void RenderCore(SpriteBatch spriteBatch)
    {
        base.RenderCore(spriteBatch);

        if (Font == null)
            return;

        var pixel = GetInputPixelTexture(spriteBatch.GraphicsDevice);
        var textY = ScreenPosition.Y + (Size.Y - Font.LineSpacing) / 2;
        var textX = ScreenPosition.X + Padding;

        if (string.IsNullOrEmpty(Text))
        {
            // Draw placeholder
            spriteBatch.DrawString(Font, PlaceholderText, new Vector2(textX, textY), PlaceholderColor);
        }
        else
        {
            // Draw text
            spriteBatch.DrawString(Font, Text, new Vector2(textX, textY), TextColor);
        }

        // Draw cursor
        if (IsFocused && _cursorVisible)
        {
            var textWidth = string.IsNullOrEmpty(Text) ? 0 : Font.MeasureString(Text).X;
            var cursorX = textX + textWidth + 2;
            spriteBatch.Draw(pixel, new Rectangle((int)cursorX, (int)textY, 2, Font.LineSpacing), CursorColor);
        }
    }
}

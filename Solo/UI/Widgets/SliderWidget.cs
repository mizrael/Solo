using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace Solo.UI.Widgets;

public class SliderWidget : Widget
{
    private const int TrackHeight = 6;
    private const int ThumbWidth = 12;
    private const int ThumbHeight = 20;

    private bool _isDragging;
    private int _value;

    public SliderWidget()
    {
        Size = new Vector2(200, ThumbHeight);
    }

    public int MinValue { get; set; } = 0;
    public int MaxValue { get; set; } = 100;

    public int Value
    {
        get => _value;
        set
        {
            var clamped = Math.Clamp(value, MinValue, MaxValue);
            if (_value != clamped)
            {
                _value = clamped;
                OnValueChanged?.Invoke(_value);
            }
        }
    }

    public Color TrackColor { get; set; } = UITheme.Scrollbar.Track;
    public Color ThumbColor { get; set; } = UITheme.Scrollbar.Thumb;
    public Color ThumbHoverColor { get; set; } = UITheme.Selection.SlotHover;

    public event Action<int>? OnValueChanged;

    protected override Vector2 MeasureCore(float availableWidth, float availableHeight)
    {
        return Size;
    }

    protected override void UpdateCore(GameTime gameTime, MouseState mouseState, MouseState previousMouseState)
    {
        var mousePoint = new Point(mouseState.X, mouseState.Y);
        var thumbRect = GetThumbRect();
        bool isOverThumb = thumbRect.Contains(mousePoint);
        bool isOverTrack = Bounds.Contains(mousePoint);

        if (mouseState.LeftButton == ButtonState.Pressed)
        {
            if (previousMouseState.LeftButton == ButtonState.Released && (isOverThumb || isOverTrack))
            {
                _isDragging = true;
            }

            if (_isDragging)
            {
                UpdateValueFromMouse(mouseState.X);
            }
        }
        else
        {
            _isDragging = false;
        }

        base.UpdateCore(gameTime, mouseState, previousMouseState);
    }

    private void UpdateValueFromMouse(int mouseX)
    {
        float trackStartX = ScreenPosition.X + ThumbWidth / 2f;
        float trackEndX = ScreenPosition.X + Size.X - ThumbWidth / 2f;
        float trackWidth = trackEndX - trackStartX;

        if (trackWidth <= 0)
            return;

        float relativeX = mouseX - trackStartX;
        float ratio = Math.Clamp(relativeX / trackWidth, 0f, 1f);

        int range = MaxValue - MinValue;
        Value = MinValue + (int)MathF.Round(ratio * range);
    }

    private Rectangle GetThumbRect()
    {
        float trackStartX = ScreenPosition.X + ThumbWidth / 2f;
        float trackEndX = ScreenPosition.X + Size.X - ThumbWidth / 2f;
        float trackWidth = trackEndX - trackStartX;

        int range = MaxValue - MinValue;
        float ratio = range > 0 ? (float)(_value - MinValue) / range : 0f;

        float thumbCenterX = trackStartX + ratio * trackWidth;
        float thumbY = ScreenPosition.Y + (Size.Y - ThumbHeight) / 2f;

        return new Rectangle(
            (int)(thumbCenterX - ThumbWidth / 2f),
            (int)thumbY,
            ThumbWidth,
            ThumbHeight
        );
    }

    protected override void RenderCore(SpriteBatch spriteBatch)
    {
        var pixel = UIResources.GetPixelTexture(spriteBatch.GraphicsDevice);

        // Draw track
        float trackY = ScreenPosition.Y + (Size.Y - TrackHeight) / 2f;
        var trackRect = new Rectangle(
            (int)ScreenPosition.X,
            (int)trackY,
            (int)Size.X,
            TrackHeight
        );
        spriteBatch.Draw(pixel, trackRect, TrackColor);

        // Draw thumb
        var thumbRect = GetThumbRect();
        var thumbColor = _isDragging ? ThumbHoverColor : ThumbColor;
        spriteBatch.Draw(pixel, thumbRect, thumbColor);
    }
}

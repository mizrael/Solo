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
    private int _step = 1;

    // Baseline for OnValueCommitted: captured lazily when the first subscriber
    // attaches, then advanced after every successful commit. This means
    // subscribers are only notified about value changes that occur AFTER they
    // subscribe — which matches both the production "mouse-up after drag" flow
    // and ignores any object-initializer Value assignment performed before any
    // listener was attached.
    private int _committedValue;
    private bool _committedBaselineSet;
    private Action<int>? _onValueCommitted;

    public SliderWidget()
    {
        Size = new Vector2(200, ThumbHeight);
        ValueFormatter = v => v.ToString();
    }

    public int MinValue { get; set; } = 0;
    public int MaxValue { get; set; } = 100;

    public int Step
    {
        get => _step;
        set => _step = value < 1 ? 1 : value;
    }

    public string? Label { get; set; }
    public Func<int, string>? ValueFormatter { get; set; }

    public int Value
    {
        get => _value;
        set
        {
            var snapped = SnapToStep(Math.Clamp(value, MinValue, MaxValue));
            if (_value != snapped)
            {
                _value = snapped;
                OnValueChanged?.Invoke(_value);
            }
        }
    }

    public Color TrackColor { get; set; } = UITheme.Scrollbar.Track;
    public Color ThumbColor { get; set; } = UITheme.Scrollbar.Thumb;
    public Color ThumbHoverColor { get; set; } = UITheme.Selection.SlotHover;
    public Color LabelColor { get; set; } = UITheme.Text.Secondary;
    public Color ValueColor { get; set; } = UITheme.Text.Primary;

    public event Action<int>? OnValueChanged;

    public event Action<int>? OnValueCommitted
    {
        add
        {
            if (!_committedBaselineSet)
            {
                _committedValue = _value;
                _committedBaselineSet = true;
            }
            _onValueCommitted += value;
        }
        remove => _onValueCommitted -= value;
    }

    /// <summary>Test seam: simulates a mouse-release commit after a drag.</summary>
    public void CommitForTesting() => CommitIfChanged();

    private int SnapToStep(int v)
    {
        if (_step <= 1) return v;
        int offset = v - MinValue;
        int snapped = MinValue + ((offset + _step / 2) / _step) * _step;
        return Math.Clamp(snapped, MinValue, MaxValue);
    }

    private void CommitIfChanged()
    {
        if (!_committedBaselineSet) return;
        if (_committedValue != _value)
        {
            _committedValue = _value;
            _onValueCommitted?.Invoke(_value);
        }
    }

    protected override Vector2 MeasureCore(float availableWidth, float availableHeight)
    {
        return Size;
    }

    protected override void UpdateCore(GameTime gameTime, MouseState mouseState, MouseState previousMouseState)
    {
        var mousePoint = new Point(mouseState.X, mouseState.Y);
        var trackHit = GetTrackBounds();
        bool isOverThumb = GetThumbRect().Contains(mousePoint);
        bool isOverTrack = trackHit.Contains(mousePoint);

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
            if (_isDragging)
            {
                _isDragging = false;
                CommitIfChanged();
            }
        }

        base.UpdateCore(gameTime, mouseState, previousMouseState);
    }

    private Rectangle GetTrackBounds()
    {
        // Track region excludes label/value text columns.
        var (trackLeft, trackWidth) = GetTrackHorizontalExtent();
        return new Rectangle((int)trackLeft, (int)ScreenPosition.Y, (int)trackWidth, (int)Size.Y);
    }

    private (float left, float width) GetTrackHorizontalExtent()
    {
        float left = ScreenPosition.X;
        float right = ScreenPosition.X + Size.X;

        if (!string.IsNullOrEmpty(Label))
        {
            var labelSize = UITheme.Font.MeasureString(Label);
            left += labelSize.X + 8;
        }

        if (ValueFormatter != null)
        {
            // Reserve space for the widest possible formatted value (use MaxValue as proxy).
            var sample = ValueFormatter(MaxValue);
            var sampleSize = UITheme.Font.MeasureString(sample);
            right -= sampleSize.X + 8;
        }

        return (left, MathF.Max(0f, right - left));
    }

    private void UpdateValueFromMouse(int mouseX)
    {
        var (trackLeft, trackWidth) = GetTrackHorizontalExtent();
        float trackStartX = trackLeft + ThumbWidth / 2f;
        float trackEndX = trackLeft + trackWidth - ThumbWidth / 2f;
        float usableWidth = trackEndX - trackStartX;

        if (usableWidth <= 0)
            return;

        float relativeX = mouseX - trackStartX;
        float ratio = Math.Clamp(relativeX / usableWidth, 0f, 1f);

        int range = MaxValue - MinValue;
        Value = MinValue + (int)MathF.Round(ratio * range);   // setter snaps + clamps
    }

    private Rectangle GetThumbRect()
    {
        var (trackLeft, trackWidth) = GetTrackHorizontalExtent();
        float trackStartX = trackLeft + ThumbWidth / 2f;
        float trackEndX = trackLeft + trackWidth - ThumbWidth / 2f;
        float usableWidth = trackEndX - trackStartX;

        int range = MaxValue - MinValue;
        float ratio = range > 0 ? (float)(_value - MinValue) / range : 0f;

        float thumbCenterX = trackStartX + ratio * usableWidth;
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

        // Label (left)
        if (!string.IsNullOrEmpty(Label))
        {
            var labelSize = UITheme.Font.MeasureString(Label);
            float labelY = ScreenPosition.Y + (Size.Y - labelSize.Y) / 2f;
            spriteBatch.DrawString(UITheme.Font, Label, new Vector2(ScreenPosition.X, labelY), LabelColor);
        }

        // Track
        var (trackLeft, trackWidth) = GetTrackHorizontalExtent();
        float trackY = ScreenPosition.Y + (Size.Y - TrackHeight) / 2f;
        var trackRect = new Rectangle((int)trackLeft, (int)trackY, (int)trackWidth, TrackHeight);
        spriteBatch.Draw(pixel, trackRect, TrackColor);

        // Thumb
        var thumbRect = GetThumbRect();
        var thumbColor = _isDragging ? ThumbHoverColor : ThumbColor;
        spriteBatch.Draw(pixel, thumbRect, thumbColor);

        // Value (right)
        if (ValueFormatter != null)
        {
            var text = ValueFormatter(_value);
            var textSize = UITheme.Font.MeasureString(text);
            float textX = ScreenPosition.X + Size.X - textSize.X;
            float textY = ScreenPosition.Y + (Size.Y - textSize.Y) / 2f;
            spriteBatch.DrawString(UITheme.Font, text, new Vector2(textX, textY), ValueColor);
        }
    }
}

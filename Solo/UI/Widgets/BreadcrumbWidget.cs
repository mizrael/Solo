using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace Solo.UI.Widgets;

public class BreadcrumbWidget : Widget
{
    private const string Separator = " > ";
    private const string Ellipsis = "...";
    private const int SegmentPadding = 4;

    private readonly List<BreadcrumbSegment> _segments = new();

    public float FontScale { get; set; } = 1.0f;
    public Color TextColor { get; set; } = UITheme.Text.Secondary;
    public Color HoverColor { get; set; } = UITheme.Text.Title;
    public Color CurrentColor { get; set; } = UITheme.Text.Primary;
    public Color BorderColor { get; set; } = UITheme.Panel.BorderColor;
    public int BorderHeight { get; set; } = 1;
    public int BottomPadding { get; set; } = 8;

    public event Action<int>? OnSegmentClicked;

    protected override Vector2 MeasureCore(float availableWidth, float availableHeight)
    {
        float textHeight = UITheme.Font.LineSpacing * FontScale;
        return new Vector2(availableWidth, textHeight + BottomPadding + BorderHeight);
    }

    public void SetPath(IReadOnlyList<string> path)
    {
        _segments.Clear();
        InvalidateMeasure();

        if (path.Count == 0)
            return;

        var separatorWidth = UITheme.Font.MeasureString(Separator).X * FontScale;
        var ellipsisWidth = UITheme.Font.MeasureString(Ellipsis).X * FontScale;
        float availableWidth = Size.X;

        float totalWidth = CalculateTotalWidth(path, separatorWidth);

        if (totalWidth <= availableWidth || path.Count <= 1)
        {
            BuildFullPath(path, separatorWidth);
        }
        else
        {
            BuildCollapsedPath(path, separatorWidth, ellipsisWidth, availableWidth);
        }
    }

    private float CalculateTotalWidth(IReadOnlyList<string> path, float separatorWidth)
    {
        float width = 0;
        for (int i = 0; i < path.Count; i++)
        {
            width += UITheme.Font.MeasureString(path[i]).X * FontScale;
            if (i < path.Count - 1)
                width += separatorWidth;
        }
        return width;
    }

    private void BuildFullPath(IReadOnlyList<string> path, float separatorWidth)
    {
        float x = 0;
        for (int i = 0; i < path.Count; i++)
        {
            var text = path[i];
            var textWidth = UITheme.Font.MeasureString(text).X * FontScale;

            _segments.Add(new BreadcrumbSegment
            {
                Text = text,
                Index = i,
                X = x,
                Width = textWidth,
                IsLast = i == path.Count - 1,
                IsEllipsis = false
            });

            x += textWidth;
            if (i < path.Count - 1)
                x += separatorWidth;
        }
    }

    private void BuildCollapsedPath(IReadOnlyList<string> path, float separatorWidth, float ellipsisWidth, float availableWidth)
    {
        var lastText = path[^1];
        var lastWidth = UITheme.Font.MeasureString(lastText).X * FontScale;

        float x = 0;

        _segments.Add(new BreadcrumbSegment
        {
            Text = Ellipsis,
            Index = path.Count - 2,
            X = x,
            Width = ellipsisWidth,
            IsLast = false,
            IsEllipsis = true
        });

        x += ellipsisWidth + separatorWidth;

        _segments.Add(new BreadcrumbSegment
        {
            Text = lastText,
            Index = path.Count - 1,
            X = x,
            Width = lastWidth,
            IsLast = true,
            IsEllipsis = false
        });
    }

    protected override void UpdateCore(GameTime gameTime, MouseState mouseState, MouseState previousMouseState)
    {
        var mousePos = new Point(mouseState.X, mouseState.Y);
        var screenPos = ScreenPosition;

        foreach (var segment in _segments)
        {
            var segmentBounds = new Rectangle(
                (int)(screenPos.X + segment.X - SegmentPadding),
                (int)screenPos.Y,
                (int)(segment.Width + SegmentPadding * 2),
                (int)Size.Y
            );

            segment.IsHovered = !segment.IsLast && segmentBounds.Contains(mousePos);
        }

        if (mouseState.LeftButton == ButtonState.Pressed &&
            previousMouseState.LeftButton == ButtonState.Released)
        {
            foreach (var segment in _segments)
            {
                if (segment.IsHovered)
                {
                    OnSegmentClicked?.Invoke(segment.Index);
                    break;
                }
            }
        }
    }

    protected override void RenderCore(SpriteBatch spriteBatch)
    {
        if (_segments.Count == 0)
            return;

        var pixel = UIResources.GetPixelTexture(spriteBatch.GraphicsDevice);
        var screenPos = ScreenPosition;
        float x = screenPos.X;
        var separatorWidth = UITheme.Font.MeasureString(Separator).X * FontScale;

        for (int i = 0; i < _segments.Count; i++)
        {
            var segment = _segments[i];
            Color color;

            if (segment.IsLast)
                color = CurrentColor;
            else if (segment.IsHovered)
                color = HoverColor;
            else
                color = TextColor;

            spriteBatch.DrawString(UITheme.Font, segment.Text, new Vector2(x, screenPos.Y), color, 0f, Vector2.Zero, FontScale, SpriteEffects.None, 0f);
            x += segment.Width;

            if (i < _segments.Count - 1)
            {
                spriteBatch.DrawString(UITheme.Font, Separator, new Vector2(x, screenPos.Y), TextColor, 0f, Vector2.Zero, FontScale, SpriteEffects.None, 0f);
                x += separatorWidth;
            }
        }

        if (BorderHeight > 0)
        {
            int borderY = (int)(screenPos.Y + Size.Y - BorderHeight - BottomPadding);
            spriteBatch.Draw(pixel, new Rectangle((int)screenPos.X, borderY, (int)Size.X, BorderHeight), BorderColor);
        }
    }

    private class BreadcrumbSegment
    {
        public string Text { get; set; } = string.Empty;
        public int Index { get; set; }
        public float X { get; set; }
        public float Width { get; set; }
        public bool IsLast { get; set; }
        public bool IsHovered { get; set; }
        public bool IsEllipsis { get; set; }
    }
}

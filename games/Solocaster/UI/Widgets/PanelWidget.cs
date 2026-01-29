using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace Solocaster.UI.Widgets;

public class PanelWidget : Widget
{
    private const int CloseButtonSize = 20;
    private const int CloseButtonMargin = 6;
    private const int ScrollbarWidth = 6;
    private const int ScrollSpeed = 30;

    private static Texture2D? _pixelTexture;

    private float _scrollOffset;
    private int _previousScrollWheelValue;

    public PanelWidget()
    {
    }

    public Color BackgroundColor { get; set; } = UITheme.Panel.BackgroundColor;
    public Color BorderColor { get; set; } = UITheme.Panel.BorderColor;
    public int BorderWidth { get; set; } = 2;
    public bool ShowCloseButton { get; set; } = true;
    public bool Scrollable { get; set; }
    public int ContentPadding { get; set; } = 16;

    public float ScrollOffset
    {
        get => _scrollOffset;
        set => _scrollOffset = Math.Max(0, Math.Min(value, MaxScrollOffset));
    }

    public event Action? OnCloseClicked;

    protected override Vector2 ChildRenderOffset
    {
        get
        {
            int topOffset = ShowCloseButton ? CloseButtonSize + CloseButtonMargin : ContentPadding;
            float scrollAdjustment = Scrollable ? -_scrollOffset : 0;
            return new Vector2(ContentPadding, topOffset + scrollAdjustment);
        }
    }

    protected Rectangle CloseButtonBounds
    {
        get
        {
            var bounds = Bounds;
            return new Rectangle(
                bounds.Right - CloseButtonSize - CloseButtonMargin,
                bounds.Y + CloseButtonMargin,
                CloseButtonSize,
                CloseButtonSize
            );
        }
    }

    /// <summary>
    /// The area where scrollable content is rendered (excludes close button area and padding).
    /// </summary>
    protected Rectangle ContentBounds
    {
        get
        {
            var bounds = Bounds;
            int topOffset = ShowCloseButton ? CloseButtonSize + CloseButtonMargin : ContentPadding;
            return new Rectangle(
                bounds.X + ContentPadding,
                bounds.Y + topOffset,
                bounds.Width - ContentPadding * 2 - (Scrollable ? ScrollbarWidth : 0),
                bounds.Height - topOffset - ContentPadding
            );
        }
    }

    /// <summary>
    /// Total height of all children content.
    /// </summary>
    protected float ContentHeight
    {
        get
        {
            float maxY = 0;
            foreach (var child in Children)
            {
                float childBottom = child.Position.Y + child.Size.Y;
                if (childBottom > maxY)
                    maxY = childBottom;
            }
            return maxY;
        }
    }

    /// <summary>
    /// Maximum scroll offset based on content height vs visible area.
    /// </summary>
    protected float MaxScrollOffset
    {
        get
        {
            if (!Scrollable)
                return 0;
            float visibleHeight = ContentBounds.Height;
            return Math.Max(0, ContentHeight - visibleHeight);
        }
    }

    protected static Texture2D GetPixelTexture(GraphicsDevice graphicsDevice)
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

        if (ShowCloseButton &&
            mouseState.LeftButton == ButtonState.Pressed &&
            previousMouseState.LeftButton == ButtonState.Released)
        {
            if (CloseButtonBounds.Contains(mouseState.X, mouseState.Y))
            {
                OnCloseClicked?.Invoke();
                Visible = false;
            }
        }

        // Handle mouse wheel scrolling
        if (Scrollable && Bounds.Contains(mouseState.X, mouseState.Y))
        {
            int scrollDelta = mouseState.ScrollWheelValue - _previousScrollWheelValue;
            if (scrollDelta != 0)
            {
                ScrollOffset -= scrollDelta / 120f * ScrollSpeed;
            }
        }
        _previousScrollWheelValue = mouseState.ScrollWheelValue;
    }

    public override void Render(SpriteBatch spriteBatch)
    {
        if (!Visible)
            return;

        RenderCore(spriteBatch);

        if (Scrollable)
        {
            RenderScrollableChildren(spriteBatch);
        }
        else
        {
            foreach (var child in Children)
                child.Render(spriteBatch);
        }
    }

    private void RenderScrollableChildren(SpriteBatch spriteBatch)
    {
        var pixel = GetPixelTexture(spriteBatch.GraphicsDevice);
        var contentBounds = ContentBounds;

        // Save original state
        var originalScissor = spriteBatch.GraphicsDevice.ScissorRectangle;
        var originalRasterizerState = spriteBatch.GraphicsDevice.RasterizerState;

        // End current batch to change rasterizer state
        spriteBatch.End();

        // Set up scissor rectangle for clipping
        var rasterizerState = new RasterizerState { ScissorTestEnable = true };
        spriteBatch.GraphicsDevice.ScissorRectangle = contentBounds;

        spriteBatch.Begin(
            SpriteSortMode.Deferred,
            BlendState.AlphaBlend,
            SamplerState.PointClamp,
            null,
            rasterizerState
        );

        // Render children (they will use ChildRenderOffset for positioning)
        foreach (var child in Children)
            child.Render(spriteBatch);

        spriteBatch.End();

        // Restore original state
        spriteBatch.GraphicsDevice.ScissorRectangle = originalScissor;

        spriteBatch.Begin(
            SpriteSortMode.Deferred,
            BlendState.AlphaBlend,
            SamplerState.PointClamp,
            null,
            originalRasterizerState
        );

        // Render scrollbar
        RenderScrollbar(spriteBatch, pixel, contentBounds);
    }

    private void RenderScrollbar(SpriteBatch spriteBatch, Texture2D pixel, Rectangle contentBounds)
    {
        if (MaxScrollOffset <= 0)
            return;

        int scrollbarX = Bounds.Right - ContentPadding - ScrollbarWidth + 4;
        int scrollbarHeight = contentBounds.Height;

        // Track
        spriteBatch.Draw(pixel, new Rectangle(scrollbarX, contentBounds.Y, ScrollbarWidth, scrollbarHeight), UITheme.Scrollbar.Track);

        // Thumb
        float thumbRatio = contentBounds.Height / (contentBounds.Height + MaxScrollOffset);
        int thumbHeight = Math.Max(20, (int)(scrollbarHeight * thumbRatio));
        float scrollRatio = MaxScrollOffset > 0 ? _scrollOffset / MaxScrollOffset : 0;
        int thumbY = contentBounds.Y + (int)((scrollbarHeight - thumbHeight) * scrollRatio);

        spriteBatch.Draw(pixel, new Rectangle(scrollbarX, thumbY, ScrollbarWidth, thumbHeight), UITheme.Scrollbar.Thumb);
    }

    protected override void RenderCore(SpriteBatch spriteBatch)
    {
        var pixel = GetPixelTexture(spriteBatch.GraphicsDevice);
        var bounds = Bounds;

        // Draw background
        spriteBatch.Draw(pixel, bounds, BackgroundColor);

        // Draw border
        if (BorderWidth > 0)
        {
            // Top
            spriteBatch.Draw(pixel, new Rectangle(bounds.X, bounds.Y, bounds.Width, BorderWidth), BorderColor);
            // Bottom
            spriteBatch.Draw(pixel, new Rectangle(bounds.X, bounds.Bottom - BorderWidth, bounds.Width, BorderWidth), BorderColor);
            // Left
            spriteBatch.Draw(pixel, new Rectangle(bounds.X, bounds.Y, BorderWidth, bounds.Height), BorderColor);
            // Right
            spriteBatch.Draw(pixel, new Rectangle(bounds.Right - BorderWidth, bounds.Y, BorderWidth, bounds.Height), BorderColor);
        }

        // Draw close button
        if (ShowCloseButton)
        {
            RenderCloseButton(spriteBatch, pixel);
        }
    }

    private void RenderCloseButton(SpriteBatch spriteBatch, Texture2D pixel)
    {
        var closeBounds = CloseButtonBounds;
        var closeColor = UITheme.Selection.CloseButton;
        var xColor = UITheme.Text.Primary;

        // Button background
        spriteBatch.Draw(pixel, closeBounds, closeColor);

        // Draw X
        int padding = 4;
        int thickness = 2;

        // Draw X as two diagonal lines using small rectangles
        for (int i = 0; i < closeBounds.Width - padding * 2; i++)
        {
            // Top-left to bottom-right diagonal
            int x1 = closeBounds.X + padding + i;
            int y1 = closeBounds.Y + padding + i;
            spriteBatch.Draw(pixel, new Rectangle(x1, y1, thickness, thickness), xColor);

            // Top-right to bottom-left diagonal
            int x2 = closeBounds.Right - padding - i - thickness;
            int y2 = closeBounds.Y + padding + i;
            spriteBatch.Draw(pixel, new Rectangle(x2, y2, thickness, thickness), xColor);
        }
    }
}

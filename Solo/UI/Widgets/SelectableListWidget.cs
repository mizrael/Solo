using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace Solo.UI.Widgets;

public class SelectableListWidget : PanelWidget
{
    private static Texture2D? _pixelTexture;
    private int _hoveredIndex = -1;

    public SelectableListWidget()
    {
        ShowCloseButton = false;
        Scrollable = true;
        ContentPadding = 0;
    }

    private List<string> _items = new();

    public List<string> Items
    {
        get => _items;
        set
        {
            if (_items != value)
            {
                _items = value;
                InvalidateMeasure();
            }
        }
    }
    public int SelectedIndex { get; set; } = -1;
    public int ItemHeight { get; set; } = UITheme.LineHeight + 8;
    public int ItemPadding { get; set; } = 8;
    public Color SelectedColor { get; set; } = UITheme.Selection.SelectedBackground;
    public Color HoverColor { get; set; } = UITheme.Selection.HoverBackground;
    public Color TextColor { get; set; } = UITheme.Text.Primary;
    public Color SelectedTextColor { get; set; } = UITheme.Text.Title;

    public event Action<int>? OnSelectionChanged;

    protected override float ContentHeight => Items.Count * ItemHeight;

    protected override Vector2 MeasureCore(float availableWidth, float availableHeight)
    {
        return new Vector2(availableWidth, Items.Count * ItemHeight);
    }

    private static Texture2D GetListPixelTexture(GraphicsDevice graphicsDevice)
    {
        if (_pixelTexture == null)
        {
            _pixelTexture = new Texture2D(graphicsDevice, 1, 1);
            _pixelTexture.SetData(new[] { Color.White });
        }
        return _pixelTexture;
    }

    public void SelectNext()
    {
        if (Items.Count == 0) return;
        SelectedIndex = (SelectedIndex + 1) % Items.Count;
        OnSelectionChanged?.Invoke(SelectedIndex);
        EnsureSelectedVisible();
    }

    public void SelectPrevious()
    {
        if (Items.Count == 0) return;
        SelectedIndex = SelectedIndex <= 0 ? Items.Count - 1 : SelectedIndex - 1;
        OnSelectionChanged?.Invoke(SelectedIndex);
        EnsureSelectedVisible();
    }

    private void EnsureSelectedVisible()
    {
        if (SelectedIndex < 0) return;

        float itemY = SelectedIndex * ItemHeight;
        float visibleHeight = Size.Y - BorderWidth * 2;

        if (itemY < ScrollOffset)
            ScrollOffset = itemY;
        else if (itemY + ItemHeight > ScrollOffset + visibleHeight)
            ScrollOffset = itemY + ItemHeight - visibleHeight;
    }

    protected override void UpdateCore(GameTime gameTime, MouseState mouseState, MouseState previousMouseState)
    {
        base.UpdateCore(gameTime, mouseState, previousMouseState);

        _hoveredIndex = -1;
        var mousePoint = new Point(mouseState.X, mouseState.Y);

        if (Bounds.Contains(mousePoint))
        {
            float relativeY = mouseState.Y - ScreenPosition.Y - BorderWidth + ScrollOffset;
            int index = (int)(relativeY / ItemHeight);
            if (index >= 0 && index < Items.Count)
                _hoveredIndex = index;
        }

        if (mouseState.LeftButton == ButtonState.Pressed &&
            previousMouseState.LeftButton == ButtonState.Released &&
            _hoveredIndex >= 0)
        {
            SelectedIndex = _hoveredIndex;
            OnSelectionChanged?.Invoke(SelectedIndex);
        }
    }

    public override void Render(SpriteBatch spriteBatch)
    {
        if (!Visible)
            return;

        // Render panel background/border
        var pixel = GetListPixelTexture(spriteBatch.GraphicsDevice);
        spriteBatch.Draw(pixel, Bounds, BackgroundColor);

        if (BorderWidth > 0)
        {
            var bounds = Bounds;
            spriteBatch.Draw(pixel, new Rectangle(bounds.X, bounds.Y, bounds.Width, BorderWidth), BorderColor);
            spriteBatch.Draw(pixel, new Rectangle(bounds.X, bounds.Bottom - BorderWidth, bounds.Width, BorderWidth), BorderColor);
            spriteBatch.Draw(pixel, new Rectangle(bounds.X, bounds.Y, BorderWidth, bounds.Height), BorderColor);
            spriteBatch.Draw(pixel, new Rectangle(bounds.Right - BorderWidth, bounds.Y, BorderWidth, bounds.Height), BorderColor);
        }

        if (Items.Count == 0)
            return;

        // End current batch to flush state, then save
        spriteBatch.End();

        var originalScissor = spriteBatch.GraphicsDevice.ScissorRectangle;
        var originalRasterizer = spriteBatch.GraphicsDevice.RasterizerState;

        var contentBounds = new Rectangle(
            Bounds.X + BorderWidth,
            Bounds.Y + BorderWidth,
            Bounds.Width - BorderWidth * 2,
            Bounds.Height - BorderWidth * 2
        );

        var scale = UITheme.UIScale;
        var sampler = scale < 1f ? SamplerState.LinearClamp : SamplerState.PointClamp;
        var matrix = scale < 1f ? UITheme.UIScaleMatrix : (Matrix?)null;
        var rasterizerState = new RasterizerState { ScissorTestEnable = true };
        spriteBatch.GraphicsDevice.ScissorRectangle = new Rectangle(
            (int)(contentBounds.X * scale),
            (int)(contentBounds.Y * scale),
            (int)(contentBounds.Width * scale),
            (int)(contentBounds.Height * scale)
        );

        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, sampler, null, rasterizerState, null, matrix);

        float y = ScreenPosition.Y + BorderWidth - ScrollOffset;

        for (int i = 0; i < Items.Count; i++)
        {
            var itemBounds = new Rectangle(
                (int)ScreenPosition.X + BorderWidth,
                (int)y,
                (int)Size.X - BorderWidth * 2,
                ItemHeight
            );

            // Draw selection/hover background
            if (i == SelectedIndex)
                spriteBatch.Draw(pixel, itemBounds, SelectedColor);
            else if (i == _hoveredIndex)
                spriteBatch.Draw(pixel, itemBounds, HoverColor);

            // Draw text
            var textColor = i == SelectedIndex ? SelectedTextColor : TextColor;
            var textPos = new Vector2(itemBounds.X + ItemPadding, itemBounds.Y + (ItemHeight - UITheme.Font.LineSpacing) / 2);
            spriteBatch.DrawString(UITheme.Font, Items[i], textPos, textColor);

            y += ItemHeight;
        }

        spriteBatch.End();

        spriteBatch.GraphicsDevice.ScissorRectangle = originalScissor;
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, sampler, null, originalRasterizer, null, matrix);
    }
}

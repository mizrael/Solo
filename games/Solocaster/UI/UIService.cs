using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Solo.Services;
using Solocaster.UI.Widgets;
using System.Collections.Generic;

namespace Solocaster.UI;

public class UIService : IGameService, IRenderable
{
    private const int DragItemSize = 48;

    private readonly List<Widget> _rootWidgets = new();
    private MouseState _previousMouseState;
    private TooltipWidget? _tooltip;
    private SpriteFont? _tooltipFont;

    public readonly DragDropManager DragDropManager = new();

    public int LayerIndex { get; set; } = RenderLayers.UI;
    public bool Hidden { get; set; } = false;

    public void Initialize()
    {
        _previousMouseState = Mouse.GetState();
        _tooltip = new TooltipWidget();
    }

    public void SetTooltipFont(SpriteFont font)
    {
        _tooltipFont = font;
        if (_tooltip != null)
            _tooltip.Font = font;
    }

    public void AddWidget(Widget widget)
    {
        if (!_rootWidgets.Contains(widget))
            _rootWidgets.Add(widget);
    }

    public void RemoveWidget(Widget widget)
    {
        _rootWidgets.Remove(widget);
    }

    public void ClearWidgets()
    {
        _rootWidgets.Clear();
    }

    public void Update(GameTime gameTime)
    {
        var mouseState = Mouse.GetState();

        if (mouseState.LeftButton == ButtonState.Released &&
            _previousMouseState.LeftButton == ButtonState.Pressed)
        {
            var mousePoint = new Point(mouseState.X, mouseState.Y);
            // Process in reverse order (top-most first)
            for (int i = _rootWidgets.Count - 1; i >= 0; i--)
            {
                if (_rootWidgets[i].HandleMouseClick(mousePoint))
                    break;
            }
        }

        foreach (var widget in _rootWidgets)
        {
            widget.Update(gameTime, mouseState, _previousMouseState);
        }

        UpdateTooltip(mouseState);

        _previousMouseState = mouseState;
    }

    private void UpdateTooltip(MouseState mouseState)
    {
        if (_tooltip == null)
            return;

        string? tooltipText = null;
        var mousePoint = new Point(mouseState.X, mouseState.Y);

        // Find topmost hovered widget with tooltip (reverse order for z-ordering)
        for (int i = _rootWidgets.Count - 1; i >= 0; i--)
        {
            var root = _rootWidgets[i];
            if (root.Visible)
            {
                tooltipText = FindTooltipText(root, mousePoint);
                if (tooltipText != null)
                    break;
            }
        }

        if (tooltipText != null)
        {
            _tooltip.Text = tooltipText;
            _tooltip.Font = _tooltipFont;
            _tooltip.UpdateSize();

            // Position near cursor (offset to not cover it)
            var tooltipX = mouseState.X + 16;
            var tooltipY = mouseState.Y + 16;

            // Keep on screen
            var screenWidth = GraphicsDeviceManagerAccessor.Instance.GraphicsDeviceManager.GraphicsDevice.Viewport.Width;
            var screenHeight = GraphicsDeviceManagerAccessor.Instance.GraphicsDeviceManager.GraphicsDevice.Viewport.Height;

            if (tooltipX + _tooltip.Size.X > screenWidth)
                tooltipX = mouseState.X - (int)_tooltip.Size.X - 8;
            if (tooltipY + _tooltip.Size.Y > screenHeight)
                tooltipY = mouseState.Y - (int)_tooltip.Size.Y - 8;

            _tooltip.Position = new Vector2(tooltipX, tooltipY);
            _tooltip.Visible = true;
        }
        else
        {
            _tooltip.Visible = false;
        }
    }

    private static string? FindTooltipText(Widget widget, Point mousePoint)
    {
        if (!widget.Visible)
            return null;

        // Check children first (reverse order for z-ordering)
        for (int i = widget.Children.Count - 1; i >= 0; i--)
        {
            var result = FindTooltipText(widget.Children[i], mousePoint);
            if (result != null)
                return result;
        }

        // Check self
        if (widget.Bounds.Contains(mousePoint))
        {
            return widget.GetTooltipText();
        }

        return null;
    }

    public bool HasVisibleWidgets()
    {
        foreach (var widget in _rootWidgets)
        {
            if (widget.Visible)
                return true;
        }
        return false;
    }

    public void Render(SpriteBatch spriteBatch)
    {
        foreach (var widget in _rootWidgets)
        {
            widget.Render(spriteBatch);
        }

        DragDropManager?.Render(spriteBatch, DragItemSize);
        _tooltip?.Render(spriteBatch);
    }
}

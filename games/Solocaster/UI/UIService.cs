using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Solo.Services;
using Solocaster.UI.Widgets;

namespace Solocaster.UI;

public class UIService : IGameService
{
    private const int DragItemSize = 48;

    private readonly List<Widget> _rootWidgets = new();
    private readonly GraphicsDeviceManager _graphics;
    private SpriteBatch? _spriteBatch;
    private MouseState _previousMouseState;
    private TooltipWidget? _tooltip;
    private SpriteFont? _tooltipFont;

    public readonly DragDropManager DragDropManager = new();

    public UIService(GraphicsDeviceManager graphics)
    {
        _graphics = graphics;
    }

    public void Initialize()
    {
        _spriteBatch = new SpriteBatch(_graphics.GraphicsDevice);
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

    public void Step(GameTime gameTime)
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
            var screenWidth = _graphics.GraphicsDevice.Viewport.Width;
            var screenHeight = _graphics.GraphicsDevice.Viewport.Height;

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

    public void Render()
    {
        if (_spriteBatch == null)
            return;

        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);

        foreach (var widget in _rootWidgets)
        {
            widget.Render(_spriteBatch);
        }

        // Render dragged item on top of all widgets
        DragDropManager?.Render(_spriteBatch, DragItemSize);

        // Render tooltip last (always on top)
        _tooltip?.Render(_spriteBatch);

        _spriteBatch.End();
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
}

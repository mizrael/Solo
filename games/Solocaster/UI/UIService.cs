using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Solo.Services;
using Solocaster.UI.Widgets;

namespace Solocaster.UI;

public class UIService : IGameService
{
    private readonly List<Widget> _rootWidgets = new();
    private readonly GraphicsDeviceManager _graphics;
    private SpriteBatch? _spriteBatch;
    private MouseState _previousMouseState;

    public UIService(GraphicsDeviceManager graphics)
    {
        _graphics = graphics;
    }

    public void Initialize()
    {
        _spriteBatch = new SpriteBatch(_graphics.GraphicsDevice);
        _previousMouseState = Mouse.GetState();
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

        // Handle mouse clicks
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

        // Update all widgets
        foreach (var widget in _rootWidgets)
        {
            widget.Update(gameTime, mouseState, _previousMouseState);
        }

        _previousMouseState = mouseState;
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

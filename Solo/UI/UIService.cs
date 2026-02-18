using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Solo.Services;
using Solo.UI.Tooltips;
using Solo.UI.Widgets;

namespace Solo.UI;

public class UIService : IGameService, IRenderable
{
    private readonly List<Widget> _rootWidgets = new();
    private MouseState _previousMouseState;
    private TooltipWidget? _tooltip;
    private int _lastViewportHeight;

    public int LayerIndex { get; set; } = int.MaxValue - 1000;
    public bool Hidden { get; set; } = false;

    public void Initialize()
    {
        _previousMouseState = Mouse.GetState();
        _tooltip = new TooltipWidget();

        var viewport = GraphicsDeviceManagerAccessor.Instance.GraphicsDeviceManager.GraphicsDevice.Viewport;
        _lastViewportHeight = viewport.Height;
        UITheme.UpdateUIScale(viewport.Height);
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
        var viewport = GraphicsDeviceManagerAccessor.Instance.GraphicsDeviceManager.GraphicsDevice.Viewport;
        if (viewport.Height != _lastViewportHeight)
        {
            _lastViewportHeight = viewport.Height;
            UITheme.UpdateUIScale(viewport.Height);
        }

        var rawMouseState = Mouse.GetState();
        var mouseState = ScaleMouseState(rawMouseState);
        var previousMouseState = _previousMouseState;

        if (rawMouseState.LeftButton == ButtonState.Released &&
            _previousMouseState.LeftButton == ButtonState.Pressed)
        {
            var mousePoint = new Point(mouseState.X, mouseState.Y);
            for (int i = _rootWidgets.Count - 1; i >= 0; i--)
            {
                if (_rootWidgets[i].HandleMouseClick(mousePoint))
                    break;
            }
        }

        foreach (var widget in _rootWidgets)
        {
            widget.Update(gameTime, mouseState, previousMouseState);
        }

        UpdateTooltip(mouseState);

        _previousMouseState = mouseState;
    }

    private static MouseState ScaleMouseState(MouseState mouseState)
    {
        float scale = UITheme.UIScale;
        if (scale >= 1f)
            return mouseState;

        return new MouseState(
            (int)(mouseState.X / scale),
            (int)(mouseState.Y / scale),
            mouseState.ScrollWheelValue,
            mouseState.LeftButton,
            mouseState.MiddleButton,
            mouseState.RightButton,
            mouseState.XButton1,
            mouseState.XButton2
        );
    }

    private void UpdateTooltip(MouseState mouseState)
    {
        if (_tooltip == null)
            return;

        var mousePoint = new Point(mouseState.X, mouseState.Y);

        TooltipTableData? tableData = null;
        TooltipContent? tooltipContent = null;
        string? tooltipText = null;

        for (int i = _rootWidgets.Count - 1; i >= 0; i--)
        {
            var root = _rootWidgets[i];
            if (root.Visible)
            {
                tableData = FindTooltipTableData(root, mousePoint);
                if (tableData != null)
                    break;

                tooltipContent = FindTooltipContent(root, mousePoint);
                if (tooltipContent != null)
                    break;

                tooltipText = FindTooltipText(root, mousePoint);
                if (tooltipText != null)
                    break;
            }
        }

        if (tableData != null)
        {
            _tooltip.SetTableContent(tableData);
            PositionTooltip(mouseState);
            _tooltip.Visible = true;
        }
        else if (tooltipContent != null)
        {
            _tooltip.SetContent(tooltipContent);
            PositionTooltip(mouseState);
            _tooltip.Visible = true;
        }
        else if (tooltipText != null)
        {
            _tooltip.SetText(tooltipText);
            PositionTooltip(mouseState);
            _tooltip.Visible = true;
        }
        else
        {
            _tooltip.Visible = false;
        }
    }

    private void PositionTooltip(MouseState mouseState)
    {
        if (_tooltip == null)
            return;

        var tooltipX = mouseState.X + 16;
        var tooltipY = mouseState.Y + 16;

        var viewport = GraphicsDeviceManagerAccessor.Instance.GraphicsDeviceManager.GraphicsDevice.Viewport;
        var screenWidth = (int)(viewport.Width / UITheme.UIScale);
        var screenHeight = (int)(viewport.Height / UITheme.UIScale);

        if (tooltipX + _tooltip.Size.X > screenWidth)
            tooltipX = mouseState.X - (int)_tooltip.Size.X - 8;
        if (tooltipY + _tooltip.Size.Y > screenHeight)
            tooltipY = mouseState.Y - (int)_tooltip.Size.Y - 8;

        _tooltip.Position = new Vector2(tooltipX, tooltipY);
    }

    private static TooltipTableData? FindTooltipTableData(Widget widget, Point mousePoint)
    {
        if (!widget.Visible)
            return null;

        for (int i = widget.Children.Count - 1; i >= 0; i--)
        {
            var result = FindTooltipTableData(widget.Children[i], mousePoint);
            if (result != null)
                return result;
        }

        if (widget.Bounds.Contains(mousePoint))
        {
            return widget.GetTooltipTableData();
        }

        return null;
    }

    private static TooltipContent? FindTooltipContent(Widget widget, Point mousePoint)
    {
        if (!widget.Visible)
            return null;

        for (int i = widget.Children.Count - 1; i >= 0; i--)
        {
            var result = FindTooltipContent(widget.Children[i], mousePoint);
            if (result != null)
                return result;
        }

        if (widget.Bounds.Contains(mousePoint))
        {
            return widget.GetTooltipContent();
        }

        return null;
    }

    private static string? FindTooltipText(Widget widget, Point mousePoint)
    {
        if (!widget.Visible)
            return null;

        for (int i = widget.Children.Count - 1; i >= 0; i--)
        {
            var result = FindTooltipText(widget.Children[i], mousePoint);
            if (result != null)
                return result;
        }

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
        var sampler = UITheme.UIScale < 1f ? SamplerState.LinearClamp : SamplerState.PointClamp;
        var matrix = UITheme.UIScale < 1f ? UITheme.UIScaleMatrix : (Matrix?)null;

        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, sampler, null, null, null, matrix);

        foreach (var widget in _rootWidgets)
        {
            widget.Render(spriteBatch);
        }

        _tooltip?.Render(spriteBatch);

        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
    }
}

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
        _tooltip = new TooltipWidget();

        var viewport = GraphicsDeviceManagerAccessor.Instance.GraphicsDeviceManager.GraphicsDevice.Viewport;
        _lastViewportHeight = viewport.Height;
        UITheme.UpdateUIScale(viewport.Height);

        // Captured after the scale is known, and scaled on the way in, because every other
        // write to this field stores a scaled state. Capturing raw here would hand widgets a
        // previous position in a different coordinate space to the current one on frame one.
        _previousMouseState = ScaleMouseState(PointerSource.GetState());
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

        var rawMouseState = PointerSource.GetState();
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

    internal static MouseState ScaleMouseState(MouseState mouseState)
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

        IReadOnlyList<TooltipBlock>? blocks = null;
        string? tooltipText = null;

        for (int i = _rootWidgets.Count - 1; i >= 0; i--)
        {
            var root = _rootWidgets[i];
            if (root.Visible)
            {
                blocks = FindTooltipBlocks(root, mousePoint);
                if (blocks != null)
                    break;

                tooltipText = FindTooltipText(root, mousePoint);
                if (tooltipText != null)
                    break;
            }
        }

        if (blocks != null)
        {
            _tooltip.SetBlocks(blocks);
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

        var viewport = GraphicsDeviceManagerAccessor.Instance.GraphicsDeviceManager.GraphicsDevice.Viewport;
        var screenWidth = (int)(viewport.Width / UITheme.UIScale);
        var screenHeight = (int)(viewport.Height / UITheme.UIScale);

        var (x, y) = ComputeTooltipPosition(
            mouseState.X, mouseState.Y,
            (int)_tooltip.Size.X, (int)_tooltip.Size.Y,
            screenWidth, screenHeight);

        _tooltip.Position = new Vector2(x, y);
    }

    internal static (int x, int y) ComputeTooltipPosition(
        int mouseX, int mouseY,
        int tooltipWidth, int tooltipHeight,
        int screenWidth, int screenHeight)
    {
        var x = mouseX + 16;
        var y = mouseY + 16;

        if (x + tooltipWidth > screenWidth)
            x = mouseX - tooltipWidth - 8;
        if (y + tooltipHeight > screenHeight)
            y = mouseY - tooltipHeight - 8;

        x = Math.Clamp(x, 0, Math.Max(0, screenWidth - tooltipWidth));
        y = Math.Clamp(y, 0, Math.Max(0, screenHeight - tooltipHeight));

        return (x, y);
    }

    private static IReadOnlyList<TooltipBlock>? FindTooltipBlocks(Widget widget, Point mousePoint)
    {
        if (!widget.Visible || widget.IsInteractionClipped(mousePoint))
            return null;

        for (int i = widget.Children.Count - 1; i >= 0; i--)
        {
            var result = FindTooltipBlocks(widget.Children[i], mousePoint);
            if (result != null)
                return result;
        }

        if (widget.Bounds.Contains(mousePoint))
        {
            return widget.GetTooltipBlocks();
        }

        return null;
    }

    private static string? FindTooltipText(Widget widget, Point mousePoint)
    {
        if (!widget.Visible || widget.IsInteractionClipped(mousePoint))
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

    public bool IsPointOverWidget(Point point)
    {
        for (int i = _rootWidgets.Count - 1; i >= 0; i--)
        {
            var widget = _rootWidgets[i];
            if (widget.Visible && widget.ContainsPoint(point))
                return true;
        }
        return false;
    }

    public bool IsScreenPointOverUI(Vector2 screenPos)
    {
        var scaledPoint = new Point(
            (int)(screenPos.X / UITheme.UIScale),
            (int)(screenPos.Y / UITheme.UIScale));
        return IsPointOverWidget(scaledPoint);
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

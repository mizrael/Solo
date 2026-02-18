using Solo.UI.Tooltips;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace Solo.UI.Widgets;

public abstract class Widget
{
    private readonly List<Widget> _children = new();
    private Vector2 _position;
    private Vector2 _size;
    private Vector2 _desiredSize;
    private bool _isMeasureDirty = true;
    private float _lastMeasureWidth;
    private float _lastMeasureHeight;

    protected Widget()
    {
        Visible = true;
        Enabled = true;
    }

    public Vector2 Position
    {
        get => _position;
        set => _position = value;
    }

    public Vector2 Size
    {
        get => _size;
        set => _size = value;
    }

    public bool Visible { get; set; }
    public bool Enabled { get; set; }
    public Widget? Parent { get; private set; }
    public IReadOnlyList<Widget> Children => _children;
    public Vector2 DesiredSize => _desiredSize;

    public Vector2 ScreenPosition
    {
        get
        {
            if (Parent == null)
                return Position;
            return Parent.ScreenPosition + Parent.ChildRenderOffset + Position;
        }
    }

    /// <summary>
    /// Offset applied to children's positions (used for scrolling).
    /// </summary>
    protected virtual Vector2 ChildRenderOffset => Vector2.Zero;

    public Rectangle Bounds => new(
        (int)ScreenPosition.X,
        (int)ScreenPosition.Y,
        (int)Size.X,
        (int)Size.Y
    );

    public void AddChild(Widget child)
    {
        if (child.Parent != null)
            child.Parent.RemoveChild(child);

        child.Parent = this;
        _children.Add(child);
        InvalidateMeasure();
    }

    public void RemoveChild(Widget child)
    {
        if (_children.Remove(child))
        {
            child.Parent = null;
            InvalidateMeasure();
        }
    }

    public void ClearChildren()
    {
        foreach (var child in _children)
            child.Parent = null;
        _children.Clear();
        InvalidateMeasure();
    }

    public Vector2 Measure(float availableWidth, float availableHeight)
    {
        bool constraintsChanged = availableWidth != _lastMeasureWidth || availableHeight != _lastMeasureHeight;
        if (!_isMeasureDirty && !constraintsChanged)
            return _desiredSize;

        _lastMeasureWidth = availableWidth;
        _lastMeasureHeight = availableHeight;
        _desiredSize = MeasureCore(availableWidth, availableHeight);
        _isMeasureDirty = false;
        return _desiredSize;
    }

    protected virtual Vector2 MeasureCore(float availableWidth, float availableHeight)
    {
        return Size;
    }

    public void Arrange(Vector2 finalSize)
    {
        var oldSize = Size;
        Size = finalSize;
        ArrangeCore(finalSize);
        if (oldSize != Size)
            OnSizeChanged();
    }

    protected virtual void ArrangeCore(Vector2 finalSize)
    {
    }

    protected virtual void OnSizeChanged()
    {
    }

    public void InvalidateMeasure()
    {
        if (_isMeasureDirty)
            return;
        _isMeasureDirty = true;
        Parent?.InvalidateMeasure();
    }

    public void Update(GameTime gameTime, MouseState mouseState, MouseState previousMouseState)
    {
        if (!Visible || !Enabled)
            return;

        if (_isMeasureDirty && Parent == null)
        {
            var prevSize = Size;
            Measure(Size.X, Size.Y);
            Arrange(DesiredSize);

            if (prevSize != Size)
            {
                _isMeasureDirty = true;
                Measure(Size.X, Size.Y);
                Arrange(DesiredSize);
            }
        }

        UpdateCore(gameTime, mouseState, previousMouseState);

        foreach (var child in _children)
            child.Update(gameTime, mouseState, previousMouseState);
    }

    protected virtual void UpdateCore(GameTime gameTime, MouseState mouseState, MouseState previousMouseState)
    {
    }

    public virtual void Render(SpriteBatch spriteBatch)
    {
        if (!Visible)
            return;

        RenderCore(spriteBatch);

        foreach (var child in _children)
            child.Render(spriteBatch);
    }

    protected virtual void RenderCore(SpriteBatch spriteBatch)
    {
    }

    public bool HandleMouseClick(Point mousePosition)
    {
        if (!Visible || !Enabled)
            return false;

        // Check children first (in reverse order for proper z-ordering)
        for (int i = _children.Count - 1; i >= 0; i--)
        {
            if (_children[i].HandleMouseClick(mousePosition))
                return true;
        }

        // Then check self
        if (Bounds.Contains(mousePosition))
        {
            OnMouseClick(mousePosition);
            return true;
        }

        return false;
    }

    protected virtual void OnMouseClick(Point mousePosition)
    {
    }

    public bool ContainsPoint(Point point)
    {
        return Bounds.Contains(point);
    }

    public virtual string? GetTooltipText() => null;

    public virtual TooltipContent? GetTooltipContent() => null;

    public virtual TooltipTableData? GetTooltipTableData() => null;
}

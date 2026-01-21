using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Solocaster.UI.Widgets;

public abstract class Widget
{
    private readonly List<Widget> _children = new();
    private Vector2 _position;
    private Vector2 _size;

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
    }

    public void RemoveChild(Widget child)
    {
        if (_children.Remove(child))
            child.Parent = null;
    }

    public void ClearChildren()
    {
        foreach (var child in _children)
            child.Parent = null;
        _children.Clear();
    }

    public void Update(GameTime gameTime, MouseState mouseState, MouseState previousMouseState)
    {
        if (!Visible || !Enabled)
            return;

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
}

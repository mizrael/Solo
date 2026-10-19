using Microsoft.Xna.Framework;

namespace Solo.UI.Widgets;

/// <summary>
/// Lays out visible children in a row or column with <see cref="Spacing"/> between them and
/// sizes itself to fit them. A vertical stack offers its full available width to each child,
/// so wrapping labels wrap to the stack's width.
/// </summary>
public class StackPanelWidget : Widget
{
    private StackOrientation _orientation = StackOrientation.Vertical;
    private StackAlignment _alignment = StackAlignment.Start;
    private float _spacing;

    public StackOrientation Orientation
    {
        get => _orientation;
        set
        {
            if (_orientation == value)
                return;
            _orientation = value;
            InvalidateMeasure();
        }
    }

    public StackAlignment Alignment
    {
        get => _alignment;
        set
        {
            if (_alignment == value)
                return;
            _alignment = value;
            InvalidateMeasure();
        }
    }

    public float Spacing
    {
        get => _spacing;
        set
        {
            if (_spacing == value)
                return;
            _spacing = value;
            InvalidateMeasure();
        }
    }

    private bool IsVertical => _orientation == StackOrientation.Vertical;

    protected override Vector2 MeasureCore(float availableWidth, float availableHeight)
    {
        float main = 0;
        float cross = 0;
        int visibleCount = 0;
        var children = Children;
        for (int i = 0; i < children.Count; i++)
        {
            var child = children[i];
            if (!child.Visible)
                continue;

            var size = IsVertical
                ? child.Measure(availableWidth, 0)
                : child.Measure(0, availableHeight);
            main += IsVertical ? size.Y : size.X;
            cross = Math.Max(cross, IsVertical ? size.X : size.Y);
            visibleCount++;
        }

        if (visibleCount > 1)
            main += _spacing * (visibleCount - 1);

        return IsVertical ? new Vector2(cross, main) : new Vector2(main, cross);
    }

    protected override void ArrangeCore(Vector2 finalSize)
    {
        float crossExtent = IsVertical ? finalSize.X : finalSize.Y;
        float offset = 0;
        var children = Children;
        for (int i = 0; i < children.Count; i++)
        {
            var child = children[i];
            if (!child.Visible)
                continue;

            var size = child.DesiredSize;
            float childMain = IsVertical ? size.Y : size.X;
            float crossOffset = AlignCross(crossExtent, IsVertical ? size.X : size.Y);
            child.Position = IsVertical ? new Vector2(crossOffset, offset) : new Vector2(offset, crossOffset);
            child.Arrange(size);
            offset += childMain + _spacing;
        }
    }

    private float AlignCross(float extent, float childExtent) => _alignment switch
    {
        StackAlignment.Center => (extent - childExtent) / 2,
        StackAlignment.End => extent - childExtent,
        _ => 0
    };
}

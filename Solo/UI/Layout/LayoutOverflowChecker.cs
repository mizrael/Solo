using Microsoft.Xna.Framework;
using Solo.UI.Widgets;

namespace Solo.UI.Layout;

/// <summary>
/// Finds visible widgets that spill out of their parent's content area, or whose text is
/// larger than the widget. Intended for tests and debug checks after layout has run.
/// Vertical overflow inside a scrollable panel is expected and not reported.
/// </summary>
public static class LayoutOverflowChecker
{
    private const float Tolerance = 0.5f;

    public static IReadOnlyList<LayoutOverflow> FindOverflows(Widget root)
    {
        ArgumentNullException.ThrowIfNull(root);
        var overflows = new List<LayoutOverflow>();
        Visit(root, overflows);
        return overflows;
    }

    private static void Visit(Widget widget, List<LayoutOverflow> overflows)
    {
        if (!widget.Visible)
            return;

        CheckText(widget, overflows);

        var content = ContentBoundsOf(widget);
        bool allowVerticalOverflow = widget is PanelWidget { Scrollable: true };
        foreach (var child in widget.Children)
        {
            if (!child.Visible)
                continue;
            CheckBounds(child, content, allowVerticalOverflow, overflows);
            Visit(child, overflows);
        }
    }

    private static void CheckBounds(Widget child, Rectangle content, bool allowVerticalOverflow, List<LayoutOverflow> overflows)
    {
        var position = child.ScreenPosition;
        var size = child.Size;
        bool horizontal = position.X < content.Left - Tolerance || position.X + size.X > content.Right + Tolerance;
        bool vertical = !allowVerticalOverflow &&
            (position.Y < content.Top - Tolerance || position.Y + size.Y > content.Bottom + Tolerance);
        if (!horizontal && !vertical)
            return;

        overflows.Add(new LayoutOverflow(child, LayoutOverflowKind.Bounds,
            $"{Describe(child)} at ({position.X}, {position.Y}) size ({size.X}, {size.Y}) exceeds {Describe(child.Parent!)} content {content}"));
    }

    private static void CheckText(Widget widget, List<LayoutOverflow> overflows)
    {
        var textSize = widget switch
        {
            LabelWidget label => label.MeasureRenderedText(),
            WrappingLabelWidget label => label.TextSize,
            _ => Vector2.Zero
        };
        var size = widget.Size;
        if (textSize.X <= size.X + Tolerance && textSize.Y <= size.Y + Tolerance)
            return;

        overflows.Add(new LayoutOverflow(widget, LayoutOverflowKind.Text,
            $"{Describe(widget)} text ({textSize.X}, {textSize.Y}) exceeds its size ({size.X}, {size.Y})"));
    }

    private static Rectangle ContentBoundsOf(Widget widget) =>
        widget is PanelWidget panel ? panel.ContentBounds : widget.Bounds;

    private static string Describe(Widget widget) => widget switch
    {
        LabelWidget label => $"{nameof(LabelWidget)} \"{label.Text}\"",
        WrappingLabelWidget label => $"{nameof(WrappingLabelWidget)} \"{label.Text}\"",
        _ => widget.GetType().Name
    };
}

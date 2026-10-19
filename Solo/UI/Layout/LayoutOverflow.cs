using Solo.UI.Widgets;

namespace Solo.UI.Layout;

public enum LayoutOverflowKind
{
    /// <summary>The widget's bounds extend past its parent's content area.</summary>
    Bounds,

    /// <summary>The widget's text is larger than the widget's own bounds.</summary>
    Text
}

public readonly record struct LayoutOverflow(Widget Widget, LayoutOverflowKind Kind, string Description)
{
    public override string ToString() => Description;
}

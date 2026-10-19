using Microsoft.Xna.Framework;
using Solo.Tests.UI.Layout;
using Solo.UI.Widgets;
using Xunit;

namespace Solo.Tests.UI.Widgets;

public sealed class StackPanelWidgetTests
{
    [Fact]
    public void Vertical_StacksChildrenWithSpacing()
    {
        var stack = new StackPanelWidget { Spacing = 5 };
        var first = new FixedSizeWidget(100, 20);
        var second = new FixedSizeWidget(60, 30);
        stack.AddChild(first);
        stack.AddChild(second);

        stack.UpdateLayout();

        Assert.Equal(Vector2.Zero, first.Position);
        Assert.Equal(new Vector2(0, 25), second.Position);
        Assert.Equal(new Vector2(100, 55), stack.Size);
    }

    [Fact]
    public void Horizontal_StacksChildrenWithSpacing()
    {
        var stack = new StackPanelWidget { Orientation = StackOrientation.Horizontal, Spacing = 5 };
        var first = new FixedSizeWidget(100, 20);
        var second = new FixedSizeWidget(60, 30);
        stack.AddChild(first);
        stack.AddChild(second);

        stack.UpdateLayout();

        Assert.Equal(new Vector2(105, 0), second.Position);
        Assert.Equal(new Vector2(165, 30), stack.Size);
    }

    [Fact]
    public void Center_CentresChildrenAcrossTheStack()
    {
        var stack = new StackPanelWidget { Alignment = StackAlignment.Center };
        var wide = new FixedSizeWidget(100, 20);
        var narrow = new FixedSizeWidget(60, 20);
        stack.AddChild(wide);
        stack.AddChild(narrow);

        stack.UpdateLayout();

        Assert.Equal(new Vector2(20, 20), narrow.Position);
    }

    [Fact]
    public void End_AlignsChildrenToTheFarEdge()
    {
        var stack = new StackPanelWidget { Orientation = StackOrientation.Horizontal, Alignment = StackAlignment.End };
        var tall = new FixedSizeWidget(10, 40);
        var short_ = new FixedSizeWidget(10, 15);
        stack.AddChild(tall);
        stack.AddChild(short_);

        stack.UpdateLayout();

        Assert.Equal(new Vector2(10, 25), short_.Position);
    }

    [Fact]
    public void Vertical_SkipsHiddenChildren()
    {
        var stack = new StackPanelWidget { Spacing = 5 };
        stack.AddChild(new FixedSizeWidget(10, 20));
        stack.AddChild(new FixedSizeWidget(10, 20) { Visible = false });
        var last = new FixedSizeWidget(10, 20);
        stack.AddChild(last);

        stack.UpdateLayout();

        Assert.Equal(new Vector2(0, 25), last.Position);
        Assert.Equal(new Vector2(10, 45), stack.Size);
    }

    [Fact]
    public void Vertical_InsidePanel_WrapsLabelsToTheContentWidth()
    {
        var measurer = new FakeTextMeasurer();
        var panel = new PanelWidget { ShowCloseButton = false, ContentPadding = 10, BorderWidth = 0, MaxSize = new Vector2(90, 0) };
        var stack = new StackPanelWidget { Spacing = 4, Alignment = StackAlignment.Center };
        var title = new WrappingLabelWidget(measurer) { Text = "T" };
        var body = new WrappingLabelWidget(measurer) { Text = "aaa bbb ccc" };
        stack.AddChild(title);
        stack.AddChild(body);
        panel.AddChild(stack);

        panel.UpdateLayout();

        Assert.Equal(["aaa bbb", "ccc"], body.Lines);
        Assert.Equal(new Vector2(30, 0), title.Position);
        Assert.Equal(new Vector2(0, 24), body.Position);
        Assert.Equal(new Vector2(90, 84), panel.Size);
    }
}

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Solo.UI.Layout;
using Solo.UI.Widgets;
using Xunit;

namespace Solo.Tests.UI.Layout;

public sealed class LayoutOverflowCheckerTests
{
    private const int Padding = 10;
    private const float Chrome = Padding * 2;

    private static PanelWidget CreatePanel(Vector2 maxSize, bool scrollable = false) =>
        new() { ShowCloseButton = false, ContentPadding = Padding, BorderWidth = 0, MaxSize = maxSize, Scrollable = scrollable };

    [Fact]
    public void FindOverflows_WhenContentFits_ReturnsNothing()
    {
        var measurer = new FakeTextMeasurer();
        var panel = CreatePanel(new Vector2(90, 0));
        var stack = new StackPanelWidget { Spacing = 4 };
        stack.AddChild(new WrappingLabelWidget(measurer) { Text = "Title" });
        stack.AddChild(new WrappingLabelWidget(measurer) { Text = "a long body that has to wrap" });
        panel.AddChild(stack);
        panel.UpdateLayout();

        Assert.Empty(LayoutOverflowChecker.FindOverflows(panel));
    }

    [Fact]
    public void FindOverflows_ChildLargerThanParentContent_ReportsBounds()
    {
        var panel = CreatePanel(new Vector2(80, 60));
        var child = new FixedSizeWidget(100, 50);
        panel.AddChild(child);
        panel.UpdateLayout();

        var overflow = Assert.Single(LayoutOverflowChecker.FindOverflows(panel));

        Assert.Same(child, overflow.Widget);
        Assert.Equal(LayoutOverflowKind.Bounds, overflow.Kind);
    }

    [Fact]
    public void FindOverflows_LabelTextWiderThanLabel_ReportsText()
    {
        var panel = CreatePanel(new Vector2(50 + Chrome, 0));
        var label = new LabelWidget { Text = "aaaaaaaaaa", Font = CreateFont() };
        panel.AddChild(label);
        panel.UpdateLayout();

        var overflow = Assert.Single(LayoutOverflowChecker.FindOverflows(panel));

        Assert.Same(label, overflow.Widget);
        Assert.Equal(LayoutOverflowKind.Text, overflow.Kind);
        Assert.Contains("aaaaaaaaaa", overflow.Description);
    }

    [Fact]
    public void FindOverflows_LabelWrappedToFullWidthButOffset_ReportsBounds()
    {
        var panel = CreatePanel(Vector2.Zero);
        panel.Size = new Vector2(70 + Chrome, 200);
        var label = new WrappingLabelWidget(new FakeTextMeasurer()) { Text = "aaa bbb ccc", Position = new Vector2(16, 0) };
        panel.AddChild(label);
        // Simulates a parent that offers the full inner width and ignores the child's offset.
        label.Measure(70, 0);
        label.Arrange(label.DesiredSize);

        var overflow = Assert.Single(LayoutOverflowChecker.FindOverflows(panel));

        Assert.Same(label, overflow.Widget);
        Assert.Equal(LayoutOverflowKind.Bounds, overflow.Kind);
    }

    [Fact]
    public void FindOverflows_OffsetLabelLaidOutByPanel_ReturnsNothing()
    {
        var panel = CreatePanel(new Vector2(70 + Chrome, 0));
        panel.AddChild(new WrappingLabelWidget(new FakeTextMeasurer()) { Text = "aaa bbb ccc", Position = new Vector2(16, 0) });
        panel.UpdateLayout();

        Assert.Empty(LayoutOverflowChecker.FindOverflows(panel));
    }

    [Fact]
    public void FindOverflows_ScrollablePanel_IgnoresVerticalOverflow()
    {
        var panel = CreatePanel(new Vector2(0, 60), scrollable: true);
        panel.AddChild(new FixedSizeWidget(50, 200));
        panel.UpdateLayout();

        Assert.Empty(LayoutOverflowChecker.FindOverflows(panel));
    }

    [Fact]
    public void FindOverflows_ScrollablePanel_ReportsHorizontalOverflow()
    {
        var panel = CreatePanel(new Vector2(60, 60), scrollable: true);
        panel.AddChild(new FixedSizeWidget(100, 200));
        panel.UpdateLayout();

        Assert.Single(LayoutOverflowChecker.FindOverflows(panel));
    }

    [Fact]
    public void FindOverflows_SkipsHiddenWidgets()
    {
        var panel = CreatePanel(new Vector2(60, 60));
        panel.AddChild(new FixedSizeWidget(500, 500) { Visible = false });
        panel.UpdateLayout();

        Assert.Empty(LayoutOverflowChecker.FindOverflows(panel));
    }

    private static SpriteFont CreateFont() =>
        new(null, [new Rectangle(0, 0, 10, 20)], [new Rectangle(0, 0, 10, 20)], ['a'], 20, 0, [new Vector3(0, 10, 0)], null);
}

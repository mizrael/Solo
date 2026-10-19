using Microsoft.Xna.Framework;
using Solo.Tests.UI.Layout;
using Solo.UI.Widgets;
using Xunit;

namespace Solo.Tests.UI.Widgets;

public sealed class PanelWidgetSizingTests
{
    private const int Padding = 10;
    private const int Border = 2;
    private const float Chrome = Padding * 2 + Border * 2;

    private static PanelWidget CreatePanel() =>
        new() { ShowCloseButton = false, ContentPadding = Padding, BorderWidth = Border };

    [Fact]
    public void UpdateLayout_FitsChildrenPlusPadding()
    {
        var panel = CreatePanel();
        panel.AddChild(new FixedSizeWidget(100, 50) { Position = new Vector2(0, 10) });

        panel.UpdateLayout();

        Assert.Equal(new Vector2(100 + Chrome, 60 + Chrome), panel.Size);
    }

    [Fact]
    public void UpdateLayout_MinSize_EnlargesSmallContent()
    {
        var panel = CreatePanel();
        panel.MinSize = new Vector2(300, 200);
        panel.AddChild(new FixedSizeWidget(100, 50));

        panel.UpdateLayout();

        Assert.Equal(new Vector2(300, 200), panel.Size);
    }

    [Fact]
    public void UpdateLayout_MaxSize_CapsLargeContent()
    {
        var panel = CreatePanel();
        panel.MaxSize = new Vector2(80, 60);
        panel.AddChild(new FixedSizeWidget(100, 50));

        panel.UpdateLayout();

        Assert.Equal(new Vector2(80, 60), panel.Size);
    }

    [Fact]
    public void UpdateLayout_MaxSize_WrapsTextInsideIt()
    {
        var panel = CreatePanel();
        panel.MaxSize = new Vector2(70 + Chrome, 0);
        var label = new WrappingLabelWidget(new FakeTextMeasurer()) { Text = "aaa bbb ccc" };
        panel.AddChild(label);

        panel.UpdateLayout();

        Assert.Equal(["aaa bbb", "ccc"], label.Lines);
        Assert.Equal(new Vector2(70 + Chrome, 40 + Chrome), panel.Size);
    }

    [Fact]
    public void UpdateLayout_RootWithMaxSize_GrowsBackAfterShrinking()
    {
        var panel = CreatePanel();
        panel.MaxSize = new Vector2(70 + Chrome, 0);
        var label = new WrappingLabelWidget(new FakeTextMeasurer()) { Text = "a" };
        panel.AddChild(label);
        panel.UpdateLayout();

        label.Text = "aaa bbb ccc";
        panel.UpdateLayout();

        Assert.Equal(["aaa bbb", "ccc"], label.Lines);
        Assert.Equal(70 + Chrome, panel.Size.X);
    }

    [Fact]
    public void UpdateLayout_OffsetChild_WrapsToRemainingWidth()
    {
        var panel = CreatePanel();
        panel.MaxSize = new Vector2(70 + Chrome, 0);
        var label = new WrappingLabelWidget(new FakeTextMeasurer()) { Text = "aaa bbb ccc", Position = new Vector2(16, 0) };
        panel.AddChild(label);

        panel.UpdateLayout();

        Assert.Equal(["aaa", "bbb", "ccc"], label.Lines);
        Assert.True(label.Position.X + label.Size.X <= 70);
    }

    [Fact]
    public void UpdateLayout_NestedMaxSize_RespectsNarrowerParent()
    {
        var root = CreatePanel();
        root.MaxSize = new Vector2(40 + Chrome, 0);
        var inner = new PanelWidget { ShowCloseButton = false, ContentPadding = 0, BorderWidth = 0, MaxSize = new Vector2(500, 0) };
        var label = new WrappingLabelWidget(new FakeTextMeasurer()) { Text = "aaa bbb" };
        inner.AddChild(label);
        root.AddChild(inner);

        root.UpdateLayout();

        Assert.Equal(["aaa", "bbb"], label.Lines);
    }
}

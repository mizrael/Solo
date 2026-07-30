using Microsoft.Xna.Framework;
using Solo.UI.Widgets;
using Xunit;

namespace Solo.Tests.UI;

public class TabbedPanelWidgetHeaderBoundsTests
{
    private static TabbedPanelWidget CreatePanel(int tabCount, float width, Vector2? position = null)
    {
        var tabs = new List<TabPage>();
        for (int i = 0; i < tabCount; i++)
            tabs.Add(new TabPage($"Tab{i}", new PanelWidget()));

        return new TabbedPanelWidget(tabs)
        {
            Position = position ?? Vector2.Zero,
            Size = new Vector2(width, 400),
        };
    }

    [Fact]
    public void GetTabHeaderBounds_ReturnsNonOverlappingSlicesAcrossFullWidth()
    {
        var panel = CreatePanel(tabCount: 6, width: 600f);

        var first = panel.GetTabHeaderBounds(0);
        var second = panel.GetTabHeaderBounds(1);
        var last = panel.GetTabHeaderBounds(5);

        Assert.Equal(0, first.X);
        Assert.Equal(first.Right, second.X);
        Assert.Equal(600, last.Right);
        Assert.True(first.Height > 0);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(3)]
    [InlineData(5)]
    public void GetTabHeaderBounds_CenterHitTestsToTheSameIndex(int index)
    {
        var panel = CreatePanel(tabCount: 6, width: 600f);
        var bounds = panel.GetTabHeaderBounds(index);

        panel.SetActiveTabFromPoint(bounds.Center.X, bounds.Center.Y);

        Assert.Equal(index, panel.ActiveIndex);
    }

    [Fact]
    public void GetTabHeaderBounds_WithIndexOutOfRange_Throws()
    {
        var panel = CreatePanel(tabCount: 3, width: 300f);
        Assert.Throws<ArgumentOutOfRangeException>(() => panel.GetTabHeaderBounds(3));
    }

    [Fact]
    public void GetTabHeaderBounds_WithNonDivisibleWidth_LastTabAbsorbsRemainder()
    {
        // 602 / 6 = 100 remainder 2. Tabs 0-4 are 100px wide. Tab 5 is 102px wide.
        var panel = CreatePanel(tabCount: 6, width: 602f);

        var last = panel.GetTabHeaderBounds(5);

        Assert.Equal(500, last.X);
        Assert.Equal(602, last.Right);  // absorbs the 2 remainder pixels

        // A point in the remainder region (x=601) must still select the last tab.
        panel.SetActiveTabFromPoint(601, last.Y + last.Height / 2);
        Assert.Equal(5, panel.ActiveIndex);
    }

    [Fact]
    public void GetTabHeaderBounds_WhenStripIsTooNarrowToLayOut_ReturnsEmptyForEveryTab()
    {
        // 4px across 6 tabs truncates to 0px per tab. Rendering and hit-testing both bail
        // out at exactly this point, so bounds must report "nothing to aim at" rather than a
        // degenerate slice that a scripted pointer would silently click straight through.
        // The panel sits at a non-zero origin so a degenerate slice is distinguishable from
        // Rectangle.Empty rather than coincidentally equal to it.
        var panel = CreatePanel(tabCount: 6, width: 4f, position: new Vector2(50f, 30f));

        for (int i = 0; i < 6; i++)
            Assert.Equal(Rectangle.Empty, panel.GetTabHeaderBounds(i));
    }

    [Fact]
    public void GetTabHeaderBounds_WhenStripIsTooNarrowToLayOut_AgreesWithHitTesting()
    {
        var panel = CreatePanel(tabCount: 6, width: 4f, position: new Vector2(50f, 30f));

        // Inside the strip rectangle horizontally and vertically, yet still un-hittable,
        // which is the behaviour the empty bounds are promising the caller.
        Assert.False(panel.SetActiveTabFromPoint(52, 36));
    }
}

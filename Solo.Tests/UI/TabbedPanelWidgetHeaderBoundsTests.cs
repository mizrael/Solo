using Microsoft.Xna.Framework;
using Solo.UI.Widgets;
using Xunit;

namespace Solo.Tests.UI;

public class TabbedPanelWidgetHeaderBoundsTests
{
    private static TabbedPanelWidget CreatePanel(int tabCount, float width)
    {
        var tabs = new List<TabPage>();
        for (int i = 0; i < tabCount; i++)
            tabs.Add(new TabPage($"Tab{i}", new PanelWidget()));

        return new TabbedPanelWidget(tabs)
        {
            Position = Vector2.Zero,
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
}

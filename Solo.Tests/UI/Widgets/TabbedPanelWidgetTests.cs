using System;
using System.Linq;
using Solo.UI.Widgets;
using Xunit;

namespace Solo.Tests.UI.Widgets;

public class TabbedPanelWidgetTests
{
    [Fact]
    public void ComputeTabWidths_SharesRoomInProportionToTitleLength()
    {
        var widths = TabbedPanelWidget.ComputeTabWidths(new float[] { 40, 100 }, horizontalPadding: 10, totalWidth: 300);

        // Natural widths are 60 and 120, so the long title gets twice the room.
        Assert.Equal(100, widths[0]);
        Assert.Equal(200, widths[1]);
    }

    [Fact]
    public void ComputeTabWidths_AlwaysFillsTheStripExactly()
    {
        var widths = TabbedPanelWidget.ComputeTabWidths(new float[] { 37, 91, 55, 12 }, horizontalPadding: 12, totalWidth: 401);

        Assert.Equal(401, widths.Sum());
    }

    [Fact]
    public void ComputeTabWidths_KeepsPaddingAroundEveryTitleWhenThereIsRoom()
    {
        float[] titles = { 60, 80, 100 };
        var widths = TabbedPanelWidget.ComputeTabWidths(titles, horizontalPadding: 12, totalWidth: 500);

        // 500 is wider than the natural 312, so no title should be squeezed.
        for (int i = 0; i < titles.Length; i++)
            Assert.True(widths[i] >= titles[i] + 24, $"tab {i} lost its padding: {widths[i]}");
    }

    [Fact]
    public void ComputeTabWidths_ScalesDownTogetherWhenTheStripIsTooNarrow()
    {
        var widths = TabbedPanelWidget.ComputeTabWidths(new float[] { 50, 150 }, horizontalPadding: 10, totalWidth: 100);

        // Natural is 70 + 170 = 240, so everything shrinks but the longer title keeps the larger share.
        Assert.Equal(100, widths.Sum());
        Assert.True(widths[1] > widths[0]);
    }

    [Fact]
    public void ComputeTabWidths_ReturnsNothingForAnEmptyOrCollapsedStrip()
    {
        Assert.Empty(TabbedPanelWidget.ComputeTabWidths(Array.Empty<float>(), 12, 300));
        Assert.All(TabbedPanelWidget.ComputeTabWidths(new float[] { 40, 40 }, 12, 0), w => Assert.Equal(0, w));
    }

    [Fact]
    public void ComputeTabWidths_GivesEveryTabAtLeastOnePixelSoHitTestingStaysUnambiguous()
    {
        // A 6px strip cannot honour any padding, but each tab must still own a
        // distinct pixel or clicks land on a zero-width tab.
        var widths = TabbedPanelWidget.ComputeTabWidths(new float[] { 40, 40, 40, 40 }, horizontalPadding: 12, totalWidth: 6);

        Assert.All(widths, w => Assert.True(w >= 1, $"a tab collapsed to {w}px"));
        Assert.Equal(6, widths.Sum());
    }

    [Fact]
    public void ComputeTabWidths_GivesEveryTabAtLeastOnePixelEvenWhenOneTitleDominates()
    {
        // The long title would otherwise claim the whole strip and starve the rest.
        var widths = TabbedPanelWidget.ComputeTabWidths(new float[] { 1, 1, 1, 400 }, horizontalPadding: 0, totalWidth: 5);

        Assert.All(widths, w => Assert.True(w >= 1, $"a tab collapsed to {w}px"));
        Assert.Equal(5, widths.Sum());
    }

    [Fact]
    public void ComputeTabWidths_ReportsNothingWhenTheStripCannotAffordAPixelPerTab()
    {
        // Callers treat an all-zero result as "too narrow to draw", matching the
        // behaviour of the guard this replaced.
        var widths = TabbedPanelWidget.ComputeTabWidths(new float[] { 40, 40, 40, 40 }, horizontalPadding: 12, totalWidth: 3);

        Assert.All(widths, w => Assert.Equal(0, w));
    }

    [Fact]
    public void ComputeTabOffsets_StayDistinctWhenTheStripIsNarrow()
    {
        var widths = TabbedPanelWidget.ComputeTabWidths(new float[] { 40, 40, 40, 40 }, horizontalPadding: 12, totalWidth: 6);
        var offsets = TabbedPanelWidget.ComputeTabOffsets(widths);

        // Duplicate offsets make hit-testing ambiguous: a click would resolve to a
        // tab that occupies no pixels.
        Assert.Equal(offsets.Length, offsets.Distinct().Count());
    }

    [Fact]
    public void ComputeTabOffsets_PlacesEachTabAfterThePreviousOne()
    {
        var offsets = TabbedPanelWidget.ComputeTabOffsets(new[] { 100, 200, 50 });

        Assert.Equal(new[] { 0, 100, 300 }, offsets);
    }
}

using Microsoft.Xna.Framework;
using Solo.UI.Widgets;
using Xunit;

namespace Solo.Tests.UI.Widgets;

public sealed class StatProgressRowWidgetTests
{
    [Fact]
    public void CalculateProgressBarBounds_WhenLabelColumnWidthProvided_UsesColumnWidthForBarStart()
    {
        var bounds = StatProgressRowWidget.CalculateProgressBarBounds(
            rowPosition: Vector2.Zero,
            rowSize: new Vector2(300, 20),
            labelWidth: 60,
            labelColumnWidth: 140);

        Assert.Equal(152, bounds.X);
        Assert.Equal(148, bounds.Width);
    }

    [Fact]
    public void CalculateProgressBarBounds_WhenMeasuredLabelExceedsColumnWidth_UsesMeasuredWidthForBarStart()
    {
        var bounds = StatProgressRowWidget.CalculateProgressBarBounds(
            rowPosition: Vector2.Zero,
            rowSize: new Vector2(300, 20),
            labelWidth: 160,
            labelColumnWidth: 140);

        Assert.Equal(172, bounds.X);
        Assert.Equal(128, bounds.Width);
    }

    [Fact]
    public void CalculateProgressBarBounds_WhenLabelColumnWidthMissing_UsesMeasuredLabelWidthForBarStart()
    {
        var bounds = StatProgressRowWidget.CalculateProgressBarBounds(
            rowPosition: Vector2.Zero,
            rowSize: new Vector2(300, 20),
            labelWidth: 60);

        Assert.Equal(72, bounds.X);
        Assert.Equal(228, bounds.Width);
    }

    [Fact]
    public void CalculateProgressBarBounds_WhenRowWidthExpands_TracksNewRightEdge()
    {
        var narrow = StatProgressRowWidget.CalculateProgressBarBounds(
            rowPosition: Vector2.Zero,
            rowSize: new Vector2(300, 20),
            labelWidth: 90);

        var wide = StatProgressRowWidget.CalculateProgressBarBounds(
            rowPosition: Vector2.Zero,
            rowSize: new Vector2(620, 20),
            labelWidth: 90);

        Assert.Equal(300, narrow.Right);
        Assert.Equal(620, wide.Right);
        Assert.True(wide.Width > narrow.Width);
    }

    [Fact]
    public void CalculateProgressBarBounds_WhenLabelConsumesRow_DoesNotCreateNegativeWidth()
    {
        var bounds = StatProgressRowWidget.CalculateProgressBarBounds(
            rowPosition: new Vector2(5, 7),
            rowSize: new Vector2(80, 20),
            labelWidth: 120);

        Assert.Equal(85, bounds.X);
        Assert.Equal(0, bounds.Width);
        Assert.Equal(8, bounds.Y);
        Assert.Equal(18, bounds.Height);
    }

    [Theory]
    [InlineData(20, 8, 18)]
    [InlineData(22, 8, 20)]
    public void CalculateProgressBarBounds_WhenRowHeightAllows_UsesOnePixelVerticalInset(float rowHeight, int expectedY, int expectedHeight)
    {
        var bounds = StatProgressRowWidget.CalculateProgressBarBounds(
            rowPosition: new Vector2(5, 7),
            rowSize: new Vector2(300, rowHeight),
            labelWidth: 90);

        Assert.Equal(expectedY, bounds.Y);
        Assert.Equal(expectedHeight, bounds.Height);
    }

    [Fact]
    public void CalculatePercentTextPosition_WhenTextFits_CentersTextInsideProgressBar()
    {
        var position = StatProgressRowWidget.CalculatePercentTextPosition(
            barBounds: new Rectangle(110, 8, 190, 18),
            percentSize: new Vector2(24, 12));

        Assert.Equal(193, position.X);
        Assert.Equal(11, position.Y);
    }

    [Theory]
    [InlineData(100, 18, 99f, 18f, true)]
    [InlineData(100, 18, 100.1f, 18f, false)]
    [InlineData(100, 18, 99f, 18.1f, false)]
    public void ShouldRenderPercentText_WhenTextDoesNotFitWidthOrHeight_ReturnsFalse(
        int barWidth,
        int barHeight,
        float percentTextWidth,
        float percentTextHeight,
        bool expected)
    {
        bool shouldRender = StatProgressRowWidget.ShouldRenderPercentText(
            new Rectangle(0, 0, barWidth, barHeight),
            new Vector2(percentTextWidth, percentTextHeight));

        Assert.Equal(expected, shouldRender);
    }

    [Theory]
    [InlineData(-25, 0)]
    [InlineData(0, 0)]
    [InlineData(50, 100)]
    [InlineData(100, 200)]
    [InlineData(150, 200)]
    public void CalculateFillWidth_ClampsProgressToBarWidth(float progress, int expectedFillWidth)
    {
        int fillWidth = StatProgressRowWidget.CalculateFillWidth(barWidth: 200, progress);

        Assert.Equal(expectedFillWidth, fillWidth);
    }

    [Theory]
    [InlineData(99, 99f, true)]
    [InlineData(100, 100f, true)]
    [InlineData(100, 100.1f, false)]
    public void ShouldRenderPercentText_ReturnsFalseWhenTextIsWiderThanBar(int barWidth, float percentTextWidth, bool expected)
    {
        bool shouldRender = StatProgressRowWidget.ShouldRenderPercentText(barWidth, percentTextWidth);

        Assert.Equal(expected, shouldRender);
    }
}

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Solo.UI.Widgets;
using Xunit;

namespace Solo.Tests.UI.Widgets;

public sealed class LabelWidgetTests
{
    [Fact]
    public void ComputeLabelSize_WhenTextFits_ReturnsTextWidth()
    {
        var size = LabelWidget.ComputeLabelSize(
            textSize: new Vector2(120, 18),
            availableWidth: 244,
            centerHorizontally: false);

        Assert.Equal(120, size.X);
        Assert.Equal(18, size.Y);
    }

    [Fact]
    public void ComputeLabelSize_WhenTextExceedsAvailableWidth_ClampsToAvailable()
    {
        var size = LabelWidget.ComputeLabelSize(
            textSize: new Vector2(400, 18),
            availableWidth: 244,
            centerHorizontally: false);

        Assert.Equal(244, size.X);
    }

    [Fact]
    public void ComputeLabelSize_WhenCentering_FillsAvailableWidth()
    {
        var size = LabelWidget.ComputeLabelSize(
            textSize: new Vector2(80, 18),
            availableWidth: 244,
            centerHorizontally: true);

        Assert.Equal(244, size.X);
    }

    [Fact]
    public void ComputeLabelSize_WhenAvailableWidthUnconstrained_KeepsTextWidth()
    {
        var size = LabelWidget.ComputeLabelSize(
            textSize: new Vector2(400, 18),
            availableWidth: 0,
            centerHorizontally: false);

        Assert.Equal(400, size.X);
    }

    [Fact]
    public void ComputeLabelSize_WhenCenteringWithUnconstrainedWidth_KeepsTextWidth()
    {
        var size = LabelWidget.ComputeLabelSize(
            textSize: new Vector2(400, 18),
            availableWidth: 0,
            centerHorizontally: true);

        Assert.Equal(400, size.X);
    }

    [Fact]
    public void Measure_WithOwnFont_UsesThatFont()
    {
        var font = new SpriteFont(null, [new Rectangle(0, 0, 10, 30)], [new Rectangle(0, 0, 10, 30)], ['a'], 30, 0, [new Vector3(0, 10, 0)], null);
        var label = new LabelWidget { Text = "aaa", Font = font };

        label.Measure(500, 500);

        Assert.Equal(new Vector2(30, 30), label.DesiredSize);
    }
}

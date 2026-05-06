using Solo.UI.Widgets;
using Xunit;

namespace Solo.Tests.UI.Widgets;

public sealed class MetricRowWidgetTests
{
    [Fact]
    public void CalculateValueX_WhenRowWidthExpands_AnchorsValueToNewRightEdge()
    {
        float narrowX = MetricRowWidget.CalculateValueX(rowX: 10, rowWidth: 300, valueWidth: 42);
        float wideX = MetricRowWidget.CalculateValueX(rowX: 10, rowWidth: 620, valueWidth: 42);

        Assert.Equal(268, narrowX);
        Assert.Equal(588, wideX);
    }

    [Fact]
    public void CalculateValueX_WhenValueIsWiderThanRow_DoesNotMoveLeftOfRow()
    {
        float valueX = MetricRowWidget.CalculateValueX(rowX: 25, rowWidth: 40, valueWidth: 120);

        Assert.Equal(25, valueX);
    }
}

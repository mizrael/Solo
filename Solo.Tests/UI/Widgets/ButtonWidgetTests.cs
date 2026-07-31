using Microsoft.Xna.Framework;
using Solo.UI.Widgets;
using Xunit;

namespace Solo.Tests.UI.Widgets;

public sealed class ButtonWidgetTests
{
    [Fact]
    public void ComputeButtonSize_WhenAutoSize_FitsTextPlusChrome()
    {
        var size = ButtonWidget.ComputeButtonSize(
            textSize: new Vector2(52, 18),
            authoredSize: new Vector2(80, 30),
            autoSize: true,
            contentPadding: 8,
            borderWidth: 2);

        Assert.Equal(72, size.X);
        Assert.Equal(38, size.Y);
    }

    [Fact]
    public void ComputeButtonSize_WhenFixedAndTextFits_KeepsAuthoredSize()
    {
        var size = ButtonWidget.ComputeButtonSize(
            textSize: new Vector2(40, 18),
            authoredSize: new Vector2(80, 30),
            autoSize: false,
            contentPadding: 8,
            borderWidth: 2);

        Assert.Equal(80, size.X);
        Assert.Equal(30, size.Y);
    }

    [Fact]
    public void ComputeButtonSize_WhenFixedAndTextTooWide_GrowsToFitText()
    {
        // "Cancel" at 68px cannot fit an 80px button once 8px padding and
        // 2px borders are accounted for: it needs 68 + 16 + 4 = 88.
        var size = ButtonWidget.ComputeButtonSize(
            textSize: new Vector2(68, 18),
            authoredSize: new Vector2(80, 30),
            autoSize: false,
            contentPadding: 8,
            borderWidth: 2);

        Assert.Equal(88, size.X);
        Assert.Equal(30, size.Y);
    }

    [Fact]
    public void ComputeButtonSize_WhenFixedWithNoAuthoredHeight_FallsBackToFittedHeight()
    {
        var size = ButtonWidget.ComputeButtonSize(
            textSize: new Vector2(40, 18),
            authoredSize: new Vector2(80, 0),
            autoSize: false,
            contentPadding: 8,
            borderWidth: 2);

        Assert.Equal(38, size.Y);
    }
}

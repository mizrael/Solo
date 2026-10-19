using Microsoft.Xna.Framework;
using Solo.UI.Widgets;
using Solo.Tests.UI.Layout;
using Xunit;

namespace Solo.Tests.UI.Widgets;

public sealed class WrappingLabelWidgetTests
{
    private readonly FakeTextMeasurer _measurer = new();

    [Fact]
    public void Measure_WrapsToAvailableWidth()
    {
        var label = new WrappingLabelWidget(_measurer) { Text = "aaa bbb ccc" };

        var size = label.Measure(70, 0);

        Assert.Equal(["aaa bbb", "ccc"], label.Lines);
        Assert.Equal(new Vector2(70, 40), size);
    }

    [Fact]
    public void Measure_MaxWidthNarrowerThanAvailable_WrapsToMaxWidth()
    {
        var label = new WrappingLabelWidget(_measurer) { Text = "aaa bbb ccc", MaxWidth = 30 };

        var size = label.Measure(500, 0);

        Assert.Equal(new Vector2(30, 60), size);
    }

    [Fact]
    public void Measure_Unconstrained_UsesMaxWidth()
    {
        var label = new WrappingLabelWidget(_measurer) { Text = "aaa bbb ccc", MaxWidth = 70 };

        var size = label.Measure(0, 0);

        Assert.Equal(new Vector2(70, 40), size);
    }

    [Fact]
    public void Measure_MaxLines_Truncates()
    {
        var label = new WrappingLabelWidget(_measurer) { Text = "aaa bbb ccc ddd", MaxLines = 1 };

        var size = label.Measure(70, 0);

        Assert.Equal(["aaa..."], label.Lines);
        Assert.Equal(new Vector2(60, 20), size);
    }

    [Fact]
    public void Measure_Repeated_DoesNotRemeasureText()
    {
        var label = new WrappingLabelWidget(_measurer) { Text = "aaa bbb ccc" };
        label.Measure(70, 0);
        int measured = _measurer.MeasureCount;

        label.Measure(70, 0);
        label.Measure(70, 0);

        Assert.Equal(measured, _measurer.MeasureCount);
    }

    [Fact]
    public void TextChange_RelayoutsTheRoot()
    {
        var label = new WrappingLabelWidget(_measurer) { Text = "aa" };
        var root = new PanelWidget { ShowCloseButton = false, ContentPadding = 0, BorderWidth = 0, MaxSize = new Vector2(50, 0) };
        root.AddChild(label);
        root.UpdateLayout();

        label.Text = "aaaa bbbb";
        root.UpdateLayout();

        Assert.Equal(["aaaa", "bbbb"], label.Lines);
        Assert.Equal(new Vector2(40, 40), root.Size);
    }
}

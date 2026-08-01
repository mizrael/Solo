using Solo.UI.Widgets;
using Xunit;

namespace Solo.Tests.UI.Widgets;

public class TooltipWidgetTests
{
    [Fact]
    public void ComputeContentInset_KeepsThePaddingClearOfTheBorder()
    {
        // The border is drawn on the outer edge, so a 2px border plus 12px of padding
        // has to push content 14px in for the visible gap to actually be 12px.
        Assert.Equal(14, TooltipWidget.ComputeContentInset(contentPadding: 12, borderWidth: 2));
    }

    [Fact]
    public void ComputeContentInset_WithoutABorderIsJustThePadding()
    {
        Assert.Equal(12, TooltipWidget.ComputeContentInset(contentPadding: 12, borderWidth: 0));
    }

    [Fact]
    public void ComputeContentInset_GrowsWithAThickerBorder()
    {
        int thin = TooltipWidget.ComputeContentInset(8, 2);
        int thick = TooltipWidget.ComputeContentInset(8, 6);

        Assert.True(thick > thin, "a thicker border must not eat into the padding");
    }
}

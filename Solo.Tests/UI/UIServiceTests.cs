using Microsoft.Xna.Framework;
using Solo.Tests.UI.Widgets;
using Solo.UI;
using Xunit;

namespace Solo.Tests.UI;

public sealed class UIServiceTests
{
    // ── Normal placement ───────────────────────────────────────────────────────

    [Fact]
    public void ComputeTooltipPosition_WhenNoOverflow_PlacesTooltipBelowRightOfCursor()
    {
        var (x, y) = UIService.ComputeTooltipPosition(
            mouseX: 100, mouseY: 200,
            tooltipWidth: 120, tooltipHeight: 60,
            screenWidth: 1280, screenHeight: 720);

        Assert.Equal(116, x);
        Assert.Equal(216, y);
    }

    // ── Right-overflow flip ────────────────────────────────────────────────────

    [Fact]
    public void ComputeTooltipPosition_WhenRightOverflow_FlipsToLeftOfCursor()
    {
        // preferred x = 700+16 = 716; 716+200 = 916 > 800 → flip to 700-200-8 = 492
        var (x, _) = UIService.ComputeTooltipPosition(
            mouseX: 700, mouseY: 100,
            tooltipWidth: 200, tooltipHeight: 40,
            screenWidth: 800, screenHeight: 600);

        Assert.Equal(492, x);
    }

    [Fact]
    public void ComputeTooltipPosition_WhenRightOverflipProducesNegativeX_ClampsToZero()
    {
        // mouseX=5, tooltipWidth=300, screenWidth=320
        // preferred x = 21; 21+300 = 321 > 320 → flip to 5-300-8 = -303 → clamp to 0
        var (x, _) = UIService.ComputeTooltipPosition(
            mouseX: 5, mouseY: 100,
            tooltipWidth: 300, tooltipHeight: 40,
            screenWidth: 320, screenHeight: 600);

        Assert.Equal(0, x);
    }

    // ── Bottom-overflow flip ───────────────────────────────────────────────────

    [Fact]
    public void ComputeTooltipPosition_WhenBottomOverflow_FlipsAboveCursor()
    {
        // mouseY=500, tooltipHeight=120, screenHeight=600
        // preferred y = 516; 516+120 = 636 > 600 → flip to 500-120-8 = 372
        var (_, y) = UIService.ComputeTooltipPosition(
            mouseX: 100, mouseY: 500,
            tooltipWidth: 60, tooltipHeight: 120,
            screenWidth: 800, screenHeight: 600);

        Assert.Equal(372, y);
    }

    [Fact]
    public void ComputeTooltipPosition_WhenBottomOverflipProducesNegativeY_ClampsToZero()
    {
        // mouseY=5, tooltipHeight=300, screenHeight=320
        // preferred y = 21; 21+300 = 321 > 320 → flip to 5-300-8 = -303 → clamp to 0
        var (_, y) = UIService.ComputeTooltipPosition(
            mouseX: 100, mouseY: 5,
            tooltipWidth: 60, tooltipHeight: 300,
            screenWidth: 800, screenHeight: 320);

        Assert.Equal(0, y);
    }

    // ── Tooltip larger than viewport ───────────────────────────────────────────

    [Fact]
    public void ComputeTooltipPosition_WhenTooltipWiderThanViewport_PinsToZeroX()
    {
        var (x, _) = UIService.ComputeTooltipPosition(
            mouseX: 400, mouseY: 100,
            tooltipWidth: 900, tooltipHeight: 40,
            screenWidth: 800, screenHeight: 600);

        Assert.Equal(0, x);
    }

    [Fact]
    public void ComputeTooltipPosition_WhenTooltipTallerThanViewport_PinsToZeroY()
    {
        var (_, y) = UIService.ComputeTooltipPosition(
            mouseX: 100, mouseY: 300,
            tooltipWidth: 60, tooltipHeight: 700,
            screenWidth: 800, screenHeight: 600);

        Assert.Equal(0, y);
    }

    // ── Root widget updates ────────────────────────────────────────────────────

    [Fact]
    public void UpdateWidgets_WhenRootRemovesItself_LaterRootsStillUpdate()
    {
        var service = new UIService();
        var self = new ProbeWidget();
        self.OnUpdate = () => service.RemoveWidget(self);
        var later = new ProbeWidget();
        service.AddWidget(self);
        service.AddWidget(later);

        service.UpdateWidgets(new GameTime(), default, default);

        Assert.Equal(1, self.UpdateCount);
        Assert.Equal(1, later.UpdateCount);
    }

    [Fact]
    public void UpdateWidgets_WhenRootRemovesLaterRoot_RemovedRootIsSkipped()
    {
        var service = new UIService();
        var later = new ProbeWidget();
        service.AddWidget(new ProbeWidget { OnUpdate = () => service.RemoveWidget(later) });
        service.AddWidget(later);

        service.UpdateWidgets(new GameTime(), default, default);

        Assert.Equal(0, later.UpdateCount);
    }

    [Fact]
    public void UpdateWidgets_WhenRootAddsRoot_NewRootWaitsUntilNextFrame()
    {
        var service = new UIService();
        var added = new ProbeWidget();
        service.AddWidget(new ProbeWidget { OnUpdate = () => service.AddWidget(added) });

        service.UpdateWidgets(new GameTime(), default, default);
        Assert.Equal(0, added.UpdateCount);

        service.UpdateWidgets(new GameTime(), default, default);
        Assert.Equal(1, added.UpdateCount);
    }

    [Fact]
    public void UpdateWidgets_DoesNotAllocate()
    {
        var service = new UIService();
        service.AddWidget(new ProbeWidget());
        service.AddWidget(new ProbeWidget());
        var gameTime = new GameTime();

        long allocated = AllocationMeter.MeasureFrames(() => service.UpdateWidgets(gameTime, default, default));

        Assert.Equal(0, allocated);
    }
}

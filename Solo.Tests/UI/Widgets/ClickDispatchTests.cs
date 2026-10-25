using Microsoft.Xna.Framework;
using Solo.UI.Widgets;
using Xunit;

namespace Solo.Tests.UI.Widgets;

public sealed class ClickDispatchTests
{
    private static readonly Point InsideChild = new(20, 15);

    public static TheoryData<string> DisplayOnlyWidgets => new() { nameof(LabelWidget), nameof(WrappingLabelWidget), nameof(ImageWidget) };

    [Theory]
    [MemberData(nameof(DisplayOnlyWidgets))]
    public void HandleMouseClick_OnDisplayOnlyChild_ReachesClickableParent(string childType)
    {
        var parent = new ClickableWidget();
        parent.AddChild(PlaceChild(CreateDisplayOnly(childType)));

        bool handled = parent.HandleMouseClick(InsideChild);

        Assert.True(handled);
        Assert.Equal(1, parent.Clicks);
    }

    [Fact]
    public void HandleMouseClick_OnButtonChild_GoesToButtonOnly()
    {
        var parent = new ClickableWidget();
        var button = PlaceChild(new ButtonWidget { AutoSize = false });
        int buttonClicks = 0;
        button.OnClick += () => buttonClicks++;
        parent.AddChild(button);

        bool handled = parent.HandleMouseClick(InsideChild);

        Assert.True(handled);
        Assert.Equal(1, buttonClicks);
        Assert.Equal(0, parent.Clicks);
    }

    [Fact]
    public void HandleMouseClick_OnPlainChild_StopsAtChild()
    {
        var parent = new ClickableWidget();
        var child = PlaceChild(new ClickableWidget());
        parent.AddChild(child);

        parent.HandleMouseClick(InsideChild);

        Assert.Equal(1, child.Clicks);
        Assert.Equal(0, parent.Clicks);
    }

    [Fact]
    public void HandleMouseClick_OnLabelRoot_IsNotHandled()
    {
        var label = PlaceChild(new LabelWidget());

        Assert.False(label.HandleMouseClick(InsideChild));
    }

    [Fact]
    public void HandleMouseClick_OnButtonInsideLabel_StillReachesButton()
    {
        var label = new LabelWidget { Size = new Vector2(200, 40) };
        var button = PlaceChild(new ButtonWidget { AutoSize = false });
        int buttonClicks = 0;
        button.OnClick += () => buttonClicks++;
        label.AddChild(button);

        Assert.True(label.HandleMouseClick(InsideChild));
        Assert.Equal(1, buttonClicks);
    }

    private static Widget CreateDisplayOnly(string type) => type switch
    {
        nameof(LabelWidget) => new LabelWidget { Text = "Reply" },
        nameof(WrappingLabelWidget) => new WrappingLabelWidget { Text = "Reply" },
        nameof(ImageWidget) => new ImageWidget(),
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, null),
    };

    private static T PlaceChild<T>(T child) where T : Widget
    {
        child.Position = new Vector2(10, 10);
        child.Size = new Vector2(50, 20);
        return child;
    }

    private sealed class ClickableWidget : Widget
    {
        public ClickableWidget() => Size = new Vector2(200, 40);

        public int Clicks { get; private set; }

        protected override void OnMouseClick(Point mousePosition) => Clicks++;
    }
}

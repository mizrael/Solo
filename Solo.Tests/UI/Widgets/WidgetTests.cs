using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Solo.UI.Widgets;
using Xunit;

namespace Solo.Tests.UI.Widgets;

public sealed class WidgetTests
{
    [Fact]
    public void Update_WhenChildRebuildsParent_DoesNotThrowAndSkipsRemovedSiblings()
    {
        var root = new PanelWidget();
        var later = new ProbeWidget();
        var rebuilder = new ProbeWidget { OnUpdate = () => { root.ClearChildren(); root.AddChild(new ProbeWidget()); } };
        root.AddChild(rebuilder);
        root.AddChild(later);

        root.Update(new GameTime(), default, default);

        Assert.Equal(0, later.UpdateCount);
    }

    private sealed class ProbeWidget : Widget
    {
        public Action? OnUpdate { get; init; }
        public int UpdateCount { get; private set; }

        protected override void UpdateCore(GameTime gameTime, MouseState mouseState, MouseState previousMouseState)
        {
            UpdateCount++;
            OnUpdate?.Invoke();
        }
    }
}

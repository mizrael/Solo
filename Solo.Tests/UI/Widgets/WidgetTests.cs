using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Solo.UI.Widgets;
using Xunit;

namespace Solo.Tests.UI.Widgets;

public sealed class WidgetTests
{
    private const int WarmupFrames = 10;
    private const int MeasuredFrames = 100;

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

    [Fact]
    public void Update_NestedTree_DoesNotAllocate()
    {
        var root = BuildNestedTree();
        var gameTime = new GameTime();

        long allocated = MeasureAllocatedBytes(() => root.Update(gameTime, default, default));

        Assert.Equal(0, allocated);
    }

    [Fact]
    public void Render_NestedTree_DoesNotAllocate()
    {
        var root = BuildNestedTree();
        root.Update(new GameTime(), default, default);

        long allocated = MeasureAllocatedBytes(() => root.Render(null!));

        Assert.Equal(0, allocated);
    }

    [Fact]
    public void Update_VisitsParentThenChildrenInInsertionOrder()
    {
        var log = new List<string>();
        var root = new ProbeWidget { Name = "root", Log = log };
        var first = new ProbeWidget { Name = "first", Log = log };
        first.AddChild(new ProbeWidget { Name = "first.child", Log = log });
        root.AddChild(first);
        root.AddChild(new ProbeWidget { Name = "second", Log = log });

        root.Update(new GameTime(), default, default);

        Assert.Equal(["root", "first", "first.child", "second"], log);
    }

    [Fact]
    public void Update_WhenChildRemovesItself_LaterSiblingsStillUpdate()
    {
        var root = new ProbeWidget();
        var self = new ProbeWidget();
        self.OnUpdate = () => root.RemoveChild(self);
        var later = new ProbeWidget();
        root.AddChild(self);
        root.AddChild(later);

        root.Update(new GameTime(), default, default);

        Assert.Equal(1, self.UpdateCount);
        Assert.Equal(1, later.UpdateCount);
        Assert.Equal([later], root.Children);
    }

    [Fact]
    public void Update_WhenChildRemovesLaterSibling_SiblingIsSkipped()
    {
        var root = new ProbeWidget();
        var later = new ProbeWidget();
        var remover = new ProbeWidget { OnUpdate = () => root.RemoveChild(later) };
        root.AddChild(remover);
        root.AddChild(later);

        root.Update(new GameTime(), default, default);

        Assert.Equal(0, later.UpdateCount);
    }

    [Fact]
    public void Update_WhenChildRemovesEarlierSibling_LaterSiblingsStillUpdateOnce()
    {
        var root = new ProbeWidget();
        var earlier = new ProbeWidget();
        var remover = new ProbeWidget { OnUpdate = () => root.RemoveChild(earlier) };
        var later = new ProbeWidget();
        root.AddChild(earlier);
        root.AddChild(remover);
        root.AddChild(later);

        root.Update(new GameTime(), default, default);

        Assert.Equal(1, earlier.UpdateCount);
        Assert.Equal(1, later.UpdateCount);
    }

    [Fact]
    public void Update_WhenChildAddsSibling_NewSiblingWaitsUntilNextFrame()
    {
        var root = new ProbeWidget();
        var added = new ProbeWidget();
        var adder = new ProbeWidget();
        adder.OnUpdate = () =>
        {
            if (added.Parent == null)
                root.AddChild(added);
        };
        root.AddChild(adder);

        root.Update(new GameTime(), default, default);
        Assert.Equal(0, added.UpdateCount);

        root.Update(new GameTime(), default, default);
        Assert.Equal(1, added.UpdateCount);
    }

    [Fact]
    public void Update_WhenParentAddsChildInItsOwnUpdate_ChildUpdatesSameFrame()
    {
        var added = new ProbeWidget();
        var root = new ProbeWidget();
        root.OnUpdate = () =>
        {
            if (added.Parent == null)
                root.AddChild(added);
        };

        root.Update(new GameTime(), default, default);

        Assert.Equal(1, added.UpdateCount);
    }

    [Fact]
    public void Update_WhenChildReentersParentUpdate_EachChildUpdatesOncePerCall()
    {
        var root = new ProbeWidget();
        var reentered = false;
        var reenter = new ProbeWidget();
        reenter.OnUpdate = () =>
        {
            if (reentered)
                return;
            reentered = true;
            root.Update(new GameTime(), default, default);
        };
        var later = new ProbeWidget();
        root.AddChild(reenter);
        root.AddChild(later);

        root.Update(new GameTime(), default, default);

        Assert.Equal(2, reenter.UpdateCount);
        Assert.Equal(2, later.UpdateCount);
    }

    private static Widget BuildNestedTree()
    {
        var root = new ChromelessPanel();
        var hud = new ChromelessPanel();
        for (int i = 0; i < 4; i++)
            hud.AddChild(new ProbeWidget());
        var group = new ProbeWidget();
        group.AddChild(new ProbeWidget());
        group.AddChild(new ProbeWidget());
        hud.AddChild(group);
        root.AddChild(hud);

        var dialog = new ChromelessPanel();
        dialog.AddChild(new ProbeWidget());
        dialog.AddChild(new ProbeWidget());
        root.AddChild(dialog);
        return root;
    }

    private static long MeasureAllocatedBytes(Action frame)
    {
        for (int i = 0; i < WarmupFrames; i++)
            frame();

        long before = GC.GetAllocatedBytesForCurrentThread();
        for (int i = 0; i < MeasuredFrames; i++)
            frame();
        return GC.GetAllocatedBytesForCurrentThread() - before;
    }

    private sealed class ProbeWidget : Widget
    {
        public string Name { get; init; } = string.Empty;
        public List<string>? Log { get; init; }
        public Action? OnUpdate { get; set; }
        public int UpdateCount { get; private set; }

        protected override void UpdateCore(GameTime gameTime, MouseState mouseState, MouseState previousMouseState)
        {
            UpdateCount++;
            Log?.Add(Name);
            OnUpdate?.Invoke();
        }
    }

    private sealed class ChromelessPanel : PanelWidget
    {
        public ChromelessPanel()
        {
            ShowCloseButton = false;
        }

        protected override void RenderCore(SpriteBatch spriteBatch)
        {
        }
    }
}

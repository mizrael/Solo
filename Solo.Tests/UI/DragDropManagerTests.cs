using Microsoft.Xna.Framework;
using Solo.UI;
using Xunit;

namespace Solo.Tests.UI;

public sealed class DragDropManagerTests
{
    [Fact]
    public void Move_WhenBelowThreshold_DoesNotStartDrag()
    {
        var manager = new DragDropManager<string>(dragThreshold: 6f);
        var source = new object();

        manager.BeginPress("item", source, new Vector2(10, 10));
        manager.Move(new Vector2(13, 14));

        Assert.False(manager.IsDragging);
        Assert.Equal("item", manager.Payload);
        Assert.Same(source, manager.Source);
        Assert.Equal(new Vector2(13, 14), manager.Position);
    }

    [Fact]
    public void Move_WhenPastThreshold_StartsDrag()
    {
        var manager = new DragDropManager<string>(dragThreshold: 6f);

        manager.BeginPress("item", new object(), new Vector2(10, 10));
        manager.Move(new Vector2(16, 10));

        Assert.True(manager.IsDragging);
        Assert.Equal(new Vector2(16, 10), manager.Position);
    }

    [Fact]
    public void Release_WhenNotDragging_ReturnsNullAndResets()
    {
        var manager = new DragDropManager<string>();

        manager.BeginPress("item", new object(), new Vector2(10, 10));
        var result = manager.Release(new object());

        Assert.Null(result);
        Assert.False(manager.IsDragging);
        Assert.Null(manager.Payload);
        Assert.Null(manager.Source);
        Assert.Equal(Vector2.Zero, manager.Position);
    }

    [Fact]
    public void Release_WhenDragging_ReturnsDropResultAndResets()
    {
        var manager = new DragDropManager<string>(dragThreshold: 6f);
        var source = new object();
        var target = new object();

        manager.BeginPress("item", source, new Vector2(10, 10));
        manager.Move(new Vector2(20, 10));
        var result = manager.Release(target);

        Assert.Equal(new DropResult<string>("item", source, target), result);
        Assert.False(manager.IsDragging);
        Assert.Null(manager.Payload);
        Assert.Null(manager.Source);
        Assert.Equal(Vector2.Zero, manager.Position);
    }

    [Fact]
    public void Cancel_ResetsPendingDrag()
    {
        var manager = new DragDropManager<string>();

        manager.BeginPress("item", new object(), new Vector2(10, 10));
        manager.Cancel();

        Assert.False(manager.IsDragging);
        Assert.Null(manager.Payload);
        Assert.Null(manager.Source);
        Assert.Equal(Vector2.Zero, manager.Position);
    }
}

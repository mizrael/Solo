using Microsoft.Xna.Framework;
using Solo.Components;
using Xunit;

namespace Solo.Tests;

public sealed class GameObjectTests
{
    [Fact]
    public void Update_WithComponentsAndChildren_DoesNotAllocate()
    {
        var root = new GameObject();
        root.Components.Add(new CountingComponent(root));
        for (var i = 0; i < 3; i++)
        {
            var child = new GameObject();
            child.Components.Add(new CountingComponent(child));
            root.AddChild(child);
        }
        var gameTime = new GameTime();

        long allocated = AllocationMeter.MeasureFrames(() => root.Update(gameTime));

        Assert.Equal(0, allocated);
    }

    [Fact]
    public void Components_Foreach_DoesNotAllocate()
    {
        var owner = new GameObject();
        owner.Components.Add(new CountingComponent(owner));
        var count = 0;

        long allocated = AllocationMeter.MeasureFrames(() =>
        {
            foreach (var _ in owner.Components)
                count++;
        });

        Assert.Equal(0, allocated);
        Assert.True(count > 0);
    }

    private sealed class CountingComponent(GameObject owner) : Component(owner)
    {
        public int Updates { get; private set; }

        protected override void UpdateCore(GameTime gameTime) => Updates++;
    }
}

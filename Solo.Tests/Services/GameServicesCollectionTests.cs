using Microsoft.Xna.Framework;
using Solo.Services;
using Xunit;

namespace Solo.Tests.Services;

public sealed class GameServicesCollectionTests
{
    [Fact]
    public void Foreach_DoesNotAllocate()
    {
        var services = new GameServicesCollection();
        services.Add(new SceneObjectsGraph());
        var count = 0;

        long allocated = AllocationMeter.MeasureFrames(() =>
        {
            foreach (var _ in services)
                count++;
        });

        Assert.Equal(0, allocated);
        Assert.True(count > 0);
    }

    [Fact]
    public void Step_DoesNotAllocate()
    {
        var services = new GameServicesCollection();
        services.Add(new SceneObjectsGraph());
        var gameTime = new GameTime();

        long allocated = AllocationMeter.MeasureFrames(() => services.Step(gameTime));

        Assert.Equal(0, allocated);
    }
}

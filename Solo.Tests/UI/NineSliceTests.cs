using Microsoft.Xna.Framework;
using Solo.UI;
using Xunit;

namespace Solo.Tests.UI;

public sealed class NineSliceTests
{
    [Fact]
    public void Compute_KeepsCornerSizesWhenDestinationHasRoom()
    {
        var slices = NineSlice.Compute(
            new Rectangle(0, 0, 100, 80),
            new NineSliceMargins(10, 20, 30, 40),
            new Rectangle(5, 7, 200, 160));

        Assert.Equal(9, slices.Length);
        Assert.Equal(new Rectangle(5, 7, 10, 20), slices[0].Dest);
        Assert.Equal(new Rectangle(175, 7, 30, 20), slices[2].Dest);
        Assert.Equal(new Rectangle(5, 127, 10, 40), slices[6].Dest);
        Assert.Equal(new Rectangle(175, 127, 30, 40), slices[8].Dest);
    }

    [Fact]
    public void Compute_StretchesEdgesOnOneAxis()
    {
        var slices = NineSlice.Compute(
            new Rectangle(0, 0, 100, 80),
            new NineSliceMargins(10, 20, 30, 40),
            new Rectangle(5, 7, 200, 160));

        Assert.Equal(new Rectangle(15, 7, 160, 20), slices[1].Dest);
        Assert.Equal(new Rectangle(5, 27, 10, 100), slices[3].Dest);
        Assert.Equal(new Rectangle(175, 27, 30, 100), slices[5].Dest);
        Assert.Equal(new Rectangle(15, 127, 160, 40), slices[7].Dest);
    }

    [Fact]
    public void Compute_StretchesCenterOnBothAxes()
    {
        var slices = NineSlice.Compute(
            new Rectangle(0, 0, 100, 80),
            new NineSliceMargins(10, 20, 30, 40),
            new Rectangle(5, 7, 200, 160));

        Assert.Equal(new Rectangle(15, 27, 160, 100), slices[4].Dest);
        Assert.Equal(new Rectangle(10, 20, 60, 20), slices[4].Source);
    }

    [Fact]
    public void Compute_WhenDestinationIsSmallerThanMargins_ClampsWithoutNegativeSizes()
    {
        var dest = new Rectangle(5, 7, 30, 25);

        var slices = NineSlice.Compute(
            new Rectangle(0, 0, 100, 80),
            new NineSliceMargins(20, 20, 20, 20),
            dest);

        Assert.All(slices, slice =>
        {
            Assert.True(slice.Dest.Width >= 0);
            Assert.True(slice.Dest.Height >= 0);
            Assert.True(slice.Dest.Left >= dest.Left);
            Assert.True(slice.Dest.Top >= dest.Top);
            Assert.True(slice.Dest.Right <= dest.Right);
            Assert.True(slice.Dest.Bottom <= dest.Bottom);
        });
        Assert.Equal(0, slices[4].Dest.Width);
        Assert.Equal(0, slices[4].Dest.Height);
    }
}

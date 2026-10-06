using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Solo.UI;

public readonly record struct NineSliceMargins(int Left, int Top, int Right, int Bottom);

public static class NineSlice
{
    public static (Rectangle Source, Rectangle Dest)[] Compute(Rectangle source, NineSliceMargins m, Rectangle dest)
    {
        var sourceWidths = Split(source.X, source.Width, m.Left, m.Right);
        var sourceHeights = Split(source.Y, source.Height, m.Top, m.Bottom);
        var destWidths = Split(dest.X, dest.Width, m.Left, m.Right);
        var destHeights = Split(dest.Y, dest.Height, m.Top, m.Bottom);

        var slices = new (Rectangle Source, Rectangle Dest)[9];
        var index = 0;
        for (var row = 0; row < 3; row++)
        {
            for (var column = 0; column < 3; column++)
            {
                slices[index++] = (
                    new Rectangle(sourceWidths[column].Start, sourceHeights[row].Start, sourceWidths[column].Size, sourceHeights[row].Size),
                    new Rectangle(destWidths[column].Start, destHeights[row].Start, destWidths[column].Size, destHeights[row].Size));
            }
        }

        return slices;
    }

    public static void Draw(SpriteBatch sb, Texture2D tex, Rectangle source, NineSliceMargins m, Rectangle dest, Color tint)
    {
        ArgumentNullException.ThrowIfNull(sb);
        ArgumentNullException.ThrowIfNull(tex);

        foreach (var (sliceSource, sliceDest) in Compute(source, m, dest))
        {
            if (sliceSource.Width > 0 && sliceSource.Height > 0 && sliceDest.Width > 0 && sliceDest.Height > 0)
                sb.Draw(tex, sliceDest, sliceSource, tint);
        }
    }

    private static (int Start, int Size)[] Split(int start, int total, int leading, int trailing)
    {
        total = Math.Max(0, total);
        var first = Math.Clamp(leading, 0, total);
        var third = Math.Clamp(trailing, 0, total - first);
        var second = total - first - third;

        return new[]
        {
            (start, first),
            (start + first, second),
            (start + first + second, third)
        };
    }
}

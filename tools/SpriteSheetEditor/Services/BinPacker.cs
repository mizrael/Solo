using SkiaSharp;

namespace SpriteSheetEditor.Services;

public record PackingItem(string Name, int Width, int Height, SKBitmap Image);

public record PackedItem(string Name, int X, int Y, int Width, int Height, SKBitmap Image);

public record PackedResult(IReadOnlyList<PackedItem> Items, int CanvasWidth, int CanvasHeight);

public static class BinPacker
{
    public static PackedResult Pack(IEnumerable<PackingItem> items, int padding = 0)
    {
        var itemList = items.ToList();
        if (itemList.Count == 0)
        {
            return new PackedResult([], 0, 0);
        }

        var sortedItems = itemList
            .OrderByDescending(i => i.Height)
            .ThenByDescending(i => i.Width)
            .ToList();

        var totalArea = sortedItems.Sum(i => (i.Width + padding) * (i.Height + padding));
        var initialSize = NextPowerOfTwo((int)Math.Ceiling(Math.Sqrt(totalArea)));

        var canvasWidth = initialSize;
        var canvasHeight = initialSize;

        while (true)
        {
            var result = TryPack(sortedItems, canvasWidth, canvasHeight, padding);
            if (result is not null)
            {
                var (packedItems, usedWidth, usedHeight) = result.Value;
                var finalWidth = NextPowerOfTwo(usedWidth);
                var finalHeight = NextPowerOfTwo(usedHeight);
                return new PackedResult(packedItems, finalWidth, finalHeight);
            }

            if (canvasWidth <= canvasHeight)
            {
                canvasWidth *= 2;
            }
            else
            {
                canvasHeight *= 2;
            }
        }
    }

    private static (IReadOnlyList<PackedItem> Items, int UsedWidth, int UsedHeight)? TryPack(
        IReadOnlyList<PackingItem> items,
        int canvasWidth,
        int canvasHeight,
        int padding)
    {
        var packedItems = new List<PackedItem>();
        var shelves = new List<Shelf>();
        var maxUsedWidth = 0;
        var maxUsedHeight = 0;

        foreach (var item in items)
        {
            var paddedWidth = item.Width + padding;
            var paddedHeight = item.Height + padding;
            var placed = false;

            foreach (var shelf in shelves)
            {
                if (shelf.RemainingWidth >= paddedWidth && shelf.Height >= paddedHeight)
                {
                    var x = shelf.CurrentX;
                    var y = shelf.Y;
                    packedItems.Add(new PackedItem(item.Name, x, y, item.Width, item.Height, item.Image));
                    shelf.CurrentX += paddedWidth;
                    shelf.RemainingWidth -= paddedWidth;
                    maxUsedWidth = Math.Max(maxUsedWidth, x + item.Width);
                    maxUsedHeight = Math.Max(maxUsedHeight, y + item.Height);
                    placed = true;
                    break;
                }
            }

            if (!placed)
            {
                var newShelfY = shelves.Count == 0 ? 0 : shelves[^1].Y + shelves[^1].Height;
                if (newShelfY + paddedHeight > canvasHeight || paddedWidth > canvasWidth)
                {
                    return null;
                }

                var newShelf = new Shelf
                {
                    Y = newShelfY,
                    Height = paddedHeight,
                    CurrentX = paddedWidth,
                    RemainingWidth = canvasWidth - paddedWidth
                };
                shelves.Add(newShelf);
                packedItems.Add(new PackedItem(item.Name, 0, newShelfY, item.Width, item.Height, item.Image));
                maxUsedWidth = Math.Max(maxUsedWidth, item.Width);
                maxUsedHeight = Math.Max(maxUsedHeight, newShelfY + item.Height);
            }
        }

        return (packedItems, maxUsedWidth, maxUsedHeight);
    }

    private static int NextPowerOfTwo(int value)
    {
        if (value <= 0) return 1;
        value--;
        value |= value >> 1;
        value |= value >> 2;
        value |= value >> 4;
        value |= value >> 8;
        value |= value >> 16;
        return value + 1;
    }

    private class Shelf
    {
        public int Y { get; init; }
        public int Height { get; init; }
        public int CurrentX { get; set; }
        public int RemainingWidth { get; set; }
    }
}

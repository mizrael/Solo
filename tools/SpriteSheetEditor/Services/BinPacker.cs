using SkiaSharp;

namespace SpriteSheetEditor.Services;

public enum PackingLayout
{
    Grid,
    SingleColumn,
    SingleRow
}

public record PackingItem(string Name, int Width, int Height, SKBitmap Image);

public record PackedItem(string Name, int X, int Y, int Width, int Height, SKBitmap Image);

public record PackedResult(IReadOnlyList<PackedItem> Items, int CanvasWidth, int CanvasHeight);

public static class BinPacker
{
    public static PackedResult Pack(IEnumerable<PackingItem> items, int padding = 0, PackingLayout layout = PackingLayout.Grid)
    {
        var itemList = items.ToList();
        if (itemList.Count == 0)
        {
            return new PackedResult([], 0, 0);
        }

        return layout switch
        {
            PackingLayout.SingleColumn => PackSingleColumn(itemList, padding),
            PackingLayout.SingleRow => PackSingleRow(itemList, padding),
            _ => PackGrid(itemList, padding)
        };
    }

    private static PackedResult PackGrid(IReadOnlyList<PackingItem> items, int padding)
    {
        var totalArea = items.Sum(i => (i.Width + padding) * (i.Height + padding));
        var targetWidth = (int)Math.Ceiling(Math.Sqrt(totalArea));

        var packedItems = new List<PackedItem>();
        var currentX = 0;
        var currentY = 0;
        var rowHeight = 0;

        foreach (var item in items)
        {
            var paddedWidth = item.Width + padding;
            var paddedHeight = item.Height + padding;

            if (currentX + paddedWidth > targetWidth && currentX > 0)
            {
                currentY += rowHeight;
                currentX = 0;
                rowHeight = 0;
            }

            packedItems.Add(new PackedItem(item.Name, currentX, currentY, item.Width, item.Height, item.Image));
            rowHeight = Math.Max(rowHeight, paddedHeight);
            currentX += paddedWidth;
        }

        return CreateResult(packedItems);
    }

    private static PackedResult PackSingleColumn(IReadOnlyList<PackingItem> items, int padding)
    {
        var packedItems = new List<PackedItem>();
        var currentY = 0;

        foreach (var item in items)
        {
            packedItems.Add(new PackedItem(item.Name, 0, currentY, item.Width, item.Height, item.Image));
            currentY += item.Height + padding;
        }

        return CreateResult(packedItems);
    }

    private static PackedResult PackSingleRow(IReadOnlyList<PackingItem> items, int padding)
    {
        var packedItems = new List<PackedItem>();
        var currentX = 0;

        foreach (var item in items)
        {
            packedItems.Add(new PackedItem(item.Name, currentX, 0, item.Width, item.Height, item.Image));
            currentX += item.Width + padding;
        }

        return CreateResult(packedItems);
    }

    private static PackedResult CreateResult(IReadOnlyList<PackedItem> packedItems)
    {
        if (packedItems.Count == 0)
        {
            return new PackedResult(packedItems, 0, 0);
        }

        var width = packedItems.Max(p => p.X + p.Width);
        var height = packedItems.Max(p => p.Y + p.Height);
        return new PackedResult(packedItems, width, height);
    }
}

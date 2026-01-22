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
    public static PackedResult Pack(IEnumerable<PackingItem> items, PackingLayout layout = PackingLayout.Grid)
    {
        var itemList = items.ToList();
        if (itemList.Count == 0)
        {
            return new PackedResult([], 0, 0);
        }

        return layout switch
        {
            PackingLayout.SingleColumn => PackSingleColumn(itemList),
            PackingLayout.SingleRow => PackSingleRow(itemList),
            _ => PackGrid(itemList)
        };
    }

    private static PackedResult PackGrid(IReadOnlyList<PackingItem> items)
    {
        var totalArea = items.Sum(i => i.Width * i.Height);
        var targetWidth = (int)Math.Ceiling(Math.Sqrt(totalArea));

        var packedItems = new List<PackedItem>();
        var currentX = 0;
        var currentY = 0;
        var rowHeight = 0;

        foreach (var item in items)
        {
            if (currentX + item.Width > targetWidth && currentX > 0)
            {
                currentY += rowHeight;
                currentX = 0;
                rowHeight = 0;
            }

            packedItems.Add(new PackedItem(item.Name, currentX, currentY, item.Width, item.Height, item.Image));
            rowHeight = Math.Max(rowHeight, item.Height);
            currentX += item.Width;
        }

        return CreateResult(packedItems);
    }

    private static PackedResult PackSingleColumn(IReadOnlyList<PackingItem> items)
    {
        var packedItems = new List<PackedItem>();
        var currentY = 0;

        foreach (var item in items)
        {
            packedItems.Add(new PackedItem(item.Name, 0, currentY, item.Width, item.Height, item.Image));
            currentY += item.Height;
        }

        return CreateResult(packedItems);
    }

    private static PackedResult PackSingleRow(IReadOnlyList<PackingItem> items)
    {
        var packedItems = new List<PackedItem>();
        var currentX = 0;

        foreach (var item in items)
        {
            packedItems.Add(new PackedItem(item.Name, currentX, 0, item.Width, item.Height, item.Image));
            currentX += item.Width;
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

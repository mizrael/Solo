using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Solo.UI.Tooltips;

public record TooltipLine(string Text, Color Color)
{
    /// <summary>
    /// Optional icon rendered inline before <see cref="Text"/>. Scaled to the
    /// tooltip line height while preserving the source aspect ratio. When set,
    /// a small gap separates it from the text.
    /// </summary>
    public Texture2D? LeadingIcon { get; init; }

    /// <summary>
    /// Source rectangle within <see cref="LeadingIcon"/>. If null, the full
    /// texture is used.
    /// </summary>
    public Rectangle? LeadingIconSource { get; init; }
}

public class TooltipContent
{
    private readonly List<TooltipLine> _lines = new();

    public IReadOnlyList<TooltipLine> Lines => _lines;

    public TooltipContent AddLine(string text, Color color)
    {
        _lines.Add(new TooltipLine(text, color));
        return this;
    }

    public TooltipContent AddLine(string text)
    {
        _lines.Add(new TooltipLine(text, UITheme.Text.Primary));
        return this;
    }

    public TooltipContent AddIconLine(Texture2D icon, Rectangle? iconSource, string text, Color color)
    {
        _lines.Add(new TooltipLine(text, color)
        {
            LeadingIcon = icon,
            LeadingIconSource = iconSource
        });
        return this;
    }

    public TooltipContent AddEmptyLine()
    {
        _lines.Add(new TooltipLine("", UITheme.Text.Primary));
        return this;
    }

    public static TooltipContent FromPlainText(string text)
    {
        var content = new TooltipContent();
        var lines = text.Split('\n');
        foreach (var line in lines)
        {
            content.AddLine(line.TrimEnd('\r'));
        }
        return content;
    }

    /// <summary>
    /// Snapshots the accumulated lines into an immutable
    /// <see cref="TooltipContentBlock"/> for use in
    /// <see cref="Widgets.TooltipWidget.SetBlocks"/>.
    /// </summary>
    public TooltipContentBlock ToBlock() => new(_lines.ToArray());
}

public record TooltipColumnHeader(string ItemName, string? SlotLabel);

public record TooltipTableCell(string Value, Color Color);

public record TooltipTableRow(string StatName, IReadOnlyList<TooltipTableCell> Cells);

public class TooltipTableData
{
    private readonly List<TooltipColumnHeader> _headers = new();
    private readonly List<TooltipTableRow> _rows = new();

    public IReadOnlyList<TooltipColumnHeader> Headers => _headers;
    public IReadOnlyList<TooltipTableRow> Rows => _rows;

    public TooltipTableData AddHeader(string itemName, string? slotLabel = null)
    {
        _headers.Add(new TooltipColumnHeader(itemName, slotLabel));
        return this;
    }

    public TooltipTableData AddRow(string statName, IReadOnlyList<TooltipTableCell> cells)
    {
        _rows.Add(new TooltipTableRow(statName, cells));
        return this;
    }

    /// <summary>
    /// Snapshots the accumulated headers and rows into an immutable
    /// <see cref="TooltipTableBlock"/> for use in
    /// <see cref="Widgets.TooltipWidget.SetBlocks"/>.
    /// </summary>
    public TooltipTableBlock ToBlock() => new(_headers.ToArray(), _rows.ToArray());
}

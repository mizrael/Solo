using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Solo.UI.Tooltips;

public record TooltipLine(string Text, Color Color);

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
}

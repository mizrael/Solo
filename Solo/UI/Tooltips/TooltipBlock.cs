using System.Collections.Generic;

namespace Solo.UI.Tooltips;

/// <summary>
/// One vertically-stacked section of a tooltip. A tooltip is rendered as an
/// ordered list of blocks, top-to-bottom, with no extra spacing between them
/// (use an empty <see cref="TooltipLine"/> in a <see cref="TooltipContentBlock"/>
/// for a visual gap).
/// </summary>
/// <remarks>
/// Blocks replace the older <c>TooltipTableData.Footer</c> coupling, where a
/// table could optionally carry one trailing content section. With blocks,
/// any consumer can mix tables and content freely (e.g. table + effects list +
/// flavor text) without the renderer needing to know about the combination.
/// </remarks>
public abstract record TooltipBlock;

/// <summary>A block of colored text lines.</summary>
public sealed record TooltipContentBlock(IReadOnlyList<TooltipLine> Lines) : TooltipBlock;

/// <summary>A comparison table: a stat-name column followed by N item columns.</summary>
public sealed record TooltipTableBlock(
    IReadOnlyList<TooltipColumnHeader> Headers,
    IReadOnlyList<TooltipTableRow> Rows) : TooltipBlock;

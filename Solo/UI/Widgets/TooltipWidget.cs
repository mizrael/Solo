using Solo.UI.Tooltips;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Solo.UI.Widgets;

public class TooltipWidget : PanelWidget
{
    private const int Padding = 8;
    private const int ColumnGap = 16;
    private const int RowGap = 2;
    private const int IconTextGap = 4;

    private IReadOnlyList<TooltipBlock>? _blocks;

    public TooltipWidget()
    {
        ShowCloseButton = false;
        BackgroundColor = UITheme.Tooltip.BackgroundColor;
        BorderColor = UITheme.Tooltip.BorderColor;
        BorderWidth = UITheme.Tooltip.BorderWidth;
        Visible = false;
    }

    public string Text { get; set; } = string.Empty;
    public Color TextColor { get; set; } = UITheme.Text.Primary;

    /// <summary>
    /// Sets the tooltip to render an ordered list of blocks (tables and/or
    /// content sections), top-to-bottom. Replaces any previous block list or
    /// plain-text content.
    /// </summary>
    public void SetBlocks(IReadOnlyList<TooltipBlock> blocks)
    {
        _blocks = blocks;
        Text = string.Empty;
        UpdateSize();
    }

    public void SetText(string text)
    {
        Text = text;
        _blocks = null;
        UpdateSize();
    }

    public void UpdateSize()
    {
        InvalidateMeasure();
        var desired = Measure(float.MaxValue, float.MaxValue);
        Arrange(desired);
    }

    protected override Vector2 MeasureCore(float availableWidth, float availableHeight)
    {
        if (_blocks != null && _blocks.Count > 0)
        {
            float maxWidth = 0;
            float totalHeight = 0;

            foreach (var block in _blocks)
            {
                var size = MeasureBlock(block);
                if (size.X > maxWidth)
                    maxWidth = size.X;
                totalHeight += size.Y;
            }

            return new Vector2(maxWidth + Padding * 2, totalHeight + Padding * 2);
        }

        if (!string.IsNullOrEmpty(Text))
        {
            var textSize = UITheme.TooltipFont.MeasureString(Text);
            return new Vector2(textSize.X + Padding * 2, textSize.Y + Padding * 2);
        }

        return Vector2.Zero;
    }

    private static Vector2 MeasureBlock(TooltipBlock block) => block switch
    {
        TooltipContentBlock content => MeasureContentBlock(content),
        TooltipTableBlock table => MeasureTableBlock(table),
        _ => Vector2.Zero,
    };

    private static Vector2 MeasureContentBlock(TooltipContentBlock content)
    {
        if (content.Lines.Count == 0)
            return Vector2.Zero;

        float maxWidth = 0;
        float totalHeight = 0;
        float lineHeight = UITheme.TooltipFont.LineSpacing;

        foreach (var line in content.Lines)
        {
            float lineWidth = 0;
            if (line.LeadingIcon != null)
                lineWidth += MeasureLineIconWidth(line, lineHeight) + IconTextGap;
            if (!string.IsNullOrEmpty(line.Text))
                lineWidth += UITheme.TooltipFont.MeasureString(line.Text).X;
            if (lineWidth > maxWidth)
                maxWidth = lineWidth;
            totalHeight += lineHeight;
        }

        return new Vector2(maxWidth, totalHeight);
    }

    private static float MeasureLineIconWidth(TooltipLine line, float lineHeight)
    {
        if (line.LeadingIcon == null)
            return 0;
        var src = line.LeadingIconSource ?? new Rectangle(0, 0, line.LeadingIcon.Width, line.LeadingIcon.Height);
        if (src.Height <= 0)
            return lineHeight;
        return lineHeight * (src.Width / (float)src.Height);
    }

    private static Vector2 MeasureTableBlock(TooltipTableBlock table)
    {
        var columnWidths = CalculateColumnWidths(table);
        float totalWidth = 0;
        for (int i = 0; i < columnWidths.Length; i++)
        {
            totalWidth += columnWidths[i];
            if (i < columnWidths.Length - 1)
                totalWidth += ColumnGap;
        }

        float lineHeight = UITheme.TooltipFont.LineSpacing + RowGap;
        int headerRows = HasSlotLabels(table) ? 2 : 1;
        int totalRows = headerRows + table.Rows.Count;
        float totalHeight = totalRows * lineHeight;

        return new Vector2(totalWidth, totalHeight);
    }

    private static float[] CalculateColumnWidths(TooltipTableBlock table)
    {
        int columnCount = table.Headers.Count + 1;
        var widths = new float[columnCount];

        float statColumnWidth = 0;
        foreach (var row in table.Rows)
        {
            var width = UITheme.TooltipFont.MeasureString(row.StatName).X;
            if (width > statColumnWidth)
                statColumnWidth = width;
        }
        widths[0] = statColumnWidth;

        for (int i = 0; i < table.Headers.Count; i++)
        {
            var header = table.Headers[i];
            float maxWidth = UITheme.TooltipFont.MeasureString(header.ItemName).X;

            if (!string.IsNullOrEmpty(header.SlotLabel))
            {
                var labelWidth = UITheme.TooltipFont.MeasureString(header.SlotLabel).X;
                if (labelWidth > maxWidth)
                    maxWidth = labelWidth;
            }

            foreach (var row in table.Rows)
            {
                if (i < row.Cells.Count)
                {
                    var cellWidth = UITheme.TooltipFont.MeasureString(row.Cells[i].Value).X;
                    if (cellWidth > maxWidth)
                        maxWidth = cellWidth;
                }
            }

            widths[i + 1] = maxWidth;
        }

        return widths;
    }

    private static bool HasSlotLabels(TooltipTableBlock table)
    {
        foreach (var header in table.Headers)
        {
            if (!string.IsNullOrEmpty(header.SlotLabel))
                return true;
        }
        return false;
    }

    protected override void RenderCore(SpriteBatch spriteBatch)
    {
        bool hasContent = (_blocks != null && _blocks.Count > 0) ||
                          !string.IsNullOrEmpty(Text);
        if (!hasContent)
            return;

        base.RenderCore(spriteBatch);

        if (_blocks != null && _blocks.Count > 0)
        {
            var pos = ScreenPosition + new Vector2(Padding, Padding);
            foreach (var block in _blocks)
            {
                var size = MeasureBlock(block);
                RenderBlock(spriteBatch, block, pos);
                pos.Y += size.Y;
            }
        }
        else if (!string.IsNullOrEmpty(Text))
        {
            var textPos = ScreenPosition + new Vector2(Padding, Padding);
            spriteBatch.DrawString(UITheme.TooltipFont, Text, textPos, TextColor);
        }
    }

    private static void RenderBlock(SpriteBatch spriteBatch, TooltipBlock block, Vector2 origin)
    {
        switch (block)
        {
            case TooltipContentBlock content:
                RenderContentBlock(spriteBatch, content, origin);
                break;
            case TooltipTableBlock table:
                RenderTableBlock(spriteBatch, table, origin);
                break;
        }
    }

    private static void RenderContentBlock(SpriteBatch spriteBatch, TooltipContentBlock content, Vector2 origin)
    {
        var pos = origin;
        float lineHeight = UITheme.TooltipFont.LineSpacing;

        foreach (var line in content.Lines)
        {
            float textOffset = 0;

            if (line.LeadingIcon != null)
            {
                float iconWidth = MeasureLineIconWidth(line, lineHeight);
                var iconRect = new Rectangle((int)pos.X, (int)pos.Y, (int)iconWidth, (int)lineHeight);
                spriteBatch.Draw(line.LeadingIcon, iconRect, line.LeadingIconSource, Color.White);
                textOffset = iconWidth + IconTextGap;
            }

            if (!string.IsNullOrEmpty(line.Text))
            {
                spriteBatch.DrawString(UITheme.TooltipFont, line.Text, pos + new Vector2(textOffset, 0), line.Color);
            }
            pos.Y += lineHeight;
        }
    }

    private static void RenderTableBlock(SpriteBatch spriteBatch, TooltipTableBlock table, Vector2 origin)
    {
        var columnWidths = CalculateColumnWidths(table);
        float lineHeight = UITheme.TooltipFont.LineSpacing + RowGap;

        float[] columnX = new float[columnWidths.Length];
        columnX[0] = origin.X;
        for (int i = 1; i < columnWidths.Length; i++)
        {
            columnX[i] = columnX[i - 1] + columnWidths[i - 1] + ColumnGap;
        }

        float y = origin.Y;

        for (int i = 0; i < table.Headers.Count; i++)
        {
            var header = table.Headers[i];
            var x = columnX[i + 1];
            var nameWidth = UITheme.TooltipFont.MeasureString(header.ItemName).X;
            var centeredX = x + (columnWidths[i + 1] - nameWidth) / 2;
            spriteBatch.DrawString(UITheme.TooltipFont, header.ItemName, new Vector2(centeredX, y), UITheme.Text.Title);
        }
        y += lineHeight;

        if (HasSlotLabels(table))
        {
            for (int i = 0; i < table.Headers.Count; i++)
            {
                var header = table.Headers[i];
                if (!string.IsNullOrEmpty(header.SlotLabel))
                {
                    var x = columnX[i + 1];
                    var labelWidth = UITheme.TooltipFont.MeasureString(header.SlotLabel).X;
                    var centeredX = x + (columnWidths[i + 1] - labelWidth) / 2;
                    spriteBatch.DrawString(UITheme.TooltipFont, header.SlotLabel, new Vector2(centeredX, y), UITheme.Text.Muted);
                }
            }
            y += lineHeight;
        }

        foreach (var row in table.Rows)
        {
            spriteBatch.DrawString(UITheme.TooltipFont, row.StatName, new Vector2(columnX[0], y), UITheme.Text.Secondary);

            for (int i = 0; i < row.Cells.Count && i < table.Headers.Count; i++)
            {
                var cell = row.Cells[i];
                var x = columnX[i + 1];
                var cellWidth = UITheme.TooltipFont.MeasureString(cell.Value).X;
                var centeredX = x + (columnWidths[i + 1] - cellWidth) / 2;
                spriteBatch.DrawString(UITheme.TooltipFont, cell.Value, new Vector2(centeredX, y), cell.Color);
            }

            y += lineHeight;
        }
    }
}

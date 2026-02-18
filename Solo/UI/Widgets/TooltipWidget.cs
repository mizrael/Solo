using Solo.UI.Tooltips;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Solo.UI.Widgets;

public class TooltipWidget : PanelWidget
{
    private const int Padding = 8;
    private const int ColumnGap = 16;
    private const int RowGap = 2;

    private TooltipContent? _content;
    private TooltipTableData? _tableData;

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

    public void SetContent(TooltipContent content)
    {
        _content = content;
        _tableData = null;
        Text = string.Empty;
        UpdateSize();
    }

    public void SetTableContent(TooltipTableData table)
    {
        _tableData = table;
        _content = null;
        Text = string.Empty;
        UpdateSize();
    }

    public void SetText(string text)
    {
        Text = text;
        _content = null;
        _tableData = null;
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
        if (_tableData != null)
            return CalculateTableSize();

        if (_content != null && _content.Lines.Count > 0)
            return CalculateContentSize();

        if (!string.IsNullOrEmpty(Text))
        {
            var textSize = UITheme.TooltipFont.MeasureString(Text);
            return new Vector2(textSize.X + Padding * 2, textSize.Y + Padding * 2);
        }

        return Vector2.Zero;
    }

    private Vector2 CalculateContentSize()
    {
        if (_content == null)
            return Vector2.Zero;

        float maxWidth = 0;
        float totalHeight = 0;
        float lineHeight = UITheme.TooltipFont.LineSpacing;

        foreach (var line in _content.Lines)
        {
            if (!string.IsNullOrEmpty(line.Text))
            {
                var lineSize = UITheme.TooltipFont.MeasureString(line.Text);
                if (lineSize.X > maxWidth)
                    maxWidth = lineSize.X;
            }
            totalHeight += lineHeight;
        }

        return new Vector2(maxWidth + Padding * 2, totalHeight + Padding * 2);
    }

    private Vector2 CalculateTableSize()
    {
        if (_tableData == null)
            return Vector2.Zero;

        var columnWidths = CalculateColumnWidths();
        float totalWidth = Padding * 2;
        for (int i = 0; i < columnWidths.Length; i++)
        {
            totalWidth += columnWidths[i];
            if (i < columnWidths.Length - 1)
                totalWidth += ColumnGap;
        }

        float lineHeight = UITheme.TooltipFont.LineSpacing + RowGap;
        int headerRows = HasSlotLabels() ? 2 : 1;
        int totalRows = headerRows + _tableData.Rows.Count;
        float totalHeight = Padding * 2 + totalRows * lineHeight;

        return new Vector2(totalWidth, totalHeight);
    }

    private float[] CalculateColumnWidths()
    {
        if (_tableData == null)
            return [];

        int columnCount = _tableData.Headers.Count + 1;
        var widths = new float[columnCount];

        float statColumnWidth = 0;
        foreach (var row in _tableData.Rows)
        {
            var width = UITheme.TooltipFont.MeasureString(row.StatName).X;
            if (width > statColumnWidth)
                statColumnWidth = width;
        }
        widths[0] = statColumnWidth;

        for (int i = 0; i < _tableData.Headers.Count; i++)
        {
            var header = _tableData.Headers[i];
            float maxWidth = UITheme.TooltipFont.MeasureString(header.ItemName).X;

            if (!string.IsNullOrEmpty(header.SlotLabel))
            {
                var labelWidth = UITheme.TooltipFont.MeasureString(header.SlotLabel).X;
                if (labelWidth > maxWidth)
                    maxWidth = labelWidth;
            }

            foreach (var row in _tableData.Rows)
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

    private bool HasSlotLabels()
    {
        if (_tableData == null)
            return false;

        foreach (var header in _tableData.Headers)
        {
            if (!string.IsNullOrEmpty(header.SlotLabel))
                return true;
        }
        return false;
    }

    protected override void RenderCore(SpriteBatch spriteBatch)
    {
        bool hasContent = _tableData != null ||
                          (_content != null && _content.Lines.Count > 0) ||
                          !string.IsNullOrEmpty(Text);
        if (!hasContent)
            return;

        base.RenderCore(spriteBatch);

        if (_tableData != null)
        {
            RenderTable(spriteBatch);
        }
        else if (_content != null && _content.Lines.Count > 0)
        {
            RenderColoredContent(spriteBatch);
        }
        else if (!string.IsNullOrEmpty(Text))
        {
            var textPos = ScreenPosition + new Vector2(Padding, Padding);
            spriteBatch.DrawString(UITheme.TooltipFont, Text, textPos, TextColor);
        }
    }

    private void RenderColoredContent(SpriteBatch spriteBatch)
    {
        if (_content == null)
            return;

        var pos = ScreenPosition + new Vector2(Padding, Padding);
        float lineHeight = UITheme.TooltipFont.LineSpacing;

        foreach (var line in _content.Lines)
        {
            if (!string.IsNullOrEmpty(line.Text))
            {
                spriteBatch.DrawString(UITheme.TooltipFont, line.Text, pos, line.Color);
            }
            pos.Y += lineHeight;
        }
    }

    private void RenderTable(SpriteBatch spriteBatch)
    {
        if (_tableData == null)
            return;

        var columnWidths = CalculateColumnWidths();
        float lineHeight = UITheme.TooltipFont.LineSpacing + RowGap;
        var basePos = ScreenPosition + new Vector2(Padding, Padding);

        float[] columnX = new float[columnWidths.Length];
        columnX[0] = basePos.X;
        for (int i = 1; i < columnWidths.Length; i++)
        {
            columnX[i] = columnX[i - 1] + columnWidths[i - 1] + ColumnGap;
        }

        float y = basePos.Y;

        for (int i = 0; i < _tableData.Headers.Count; i++)
        {
            var header = _tableData.Headers[i];
            var x = columnX[i + 1];
            var nameWidth = UITheme.TooltipFont.MeasureString(header.ItemName).X;
            var centeredX = x + (columnWidths[i + 1] - nameWidth) / 2;
            spriteBatch.DrawString(UITheme.TooltipFont, header.ItemName, new Vector2(centeredX, y), UITheme.Text.Title);
        }
        y += lineHeight;

        if (HasSlotLabels())
        {
            for (int i = 0; i < _tableData.Headers.Count; i++)
            {
                var header = _tableData.Headers[i];
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

        foreach (var row in _tableData.Rows)
        {
            spriteBatch.DrawString(UITheme.TooltipFont, row.StatName, new Vector2(columnX[0], y), UITheme.Text.Secondary);

            for (int i = 0; i < row.Cells.Count && i < _tableData.Headers.Count; i++)
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

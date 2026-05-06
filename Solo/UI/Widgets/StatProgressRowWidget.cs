using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Solo.UI.Widgets;

public class StatProgressRowWidget : Widget
{
    private const int ProgressBarVerticalInset = 1;
    private const int LabelBarGap = 12;

    public StatProgressRowWidget()
    {
    }

    public string StatName { get; set; } = string.Empty;
    public float StatValue { get; set; }
    public float Progress { get; set; }
    public Color BarColor { get; set; } = UITheme.StatusBar.ProgressFill;
    public Color LabelColor { get; set; } = UITheme.Text.Secondary;
    public float? LabelColumnWidth { get; set; }

    internal static float ResolveLabelColumnWidth(float measuredLabelWidth, float? labelColumnWidth)
    {
        return labelColumnWidth.HasValue
            ? Math.Max(measuredLabelWidth, Math.Max(0, labelColumnWidth.Value))
            : measuredLabelWidth;
    }

    internal static Rectangle CalculateProgressBarBounds(Vector2 rowPosition, Vector2 rowSize, float labelWidth, float? labelColumnWidth = null)
    {
        int rowLeft = (int)MathF.Round(rowPosition.X);
        int rowTop = (int)MathF.Round(rowPosition.Y);
        int rowWidth = Math.Max(0, (int)MathF.Round(rowSize.X));
        int rowHeight = Math.Max(0, (int)MathF.Round(rowSize.Y));
        int rowRight = rowLeft + rowWidth;
        float resolvedLabelWidth = ResolveLabelColumnWidth(labelWidth, labelColumnWidth);
        int labelRight = rowLeft + Math.Max(0, (int)MathF.Ceiling(resolvedLabelWidth));
        int barLeft = Math.Min(rowRight, labelRight + LabelBarGap);
        int barWidth = Math.Max(0, rowRight - barLeft);

        int verticalInset = Math.Min(ProgressBarVerticalInset, rowHeight / 2);
        int barHeight = Math.Max(0, rowHeight - verticalInset * 2);

        return new Rectangle(barLeft, rowTop + verticalInset, barWidth, barHeight);
    }

    internal static int CalculateFillWidth(int barWidth, float progress)
    {
        if (barWidth <= 0)
        {
            return 0;
        }

        float ratio = Math.Clamp(progress / 100f, 0f, 1f);
        return (int)(barWidth * ratio);
    }

    internal static bool ShouldRenderPercentText(int barWidth, float percentTextWidth)
    {
        return barWidth > 0 && percentTextWidth <= barWidth;
    }

    internal static bool ShouldRenderPercentText(Rectangle barBounds, Vector2 percentSize)
    {
        return barBounds.Width > 0
            && barBounds.Height > 0
            && percentSize.X <= barBounds.Width
            && percentSize.Y <= barBounds.Height;
    }

    internal static Vector2 CalculatePercentTextPosition(Rectangle barBounds, Vector2 percentSize)
    {
        float x = barBounds.X + (barBounds.Width - percentSize.X) / 2f;
        float y = barBounds.Y + (barBounds.Height - percentSize.Y) / 2f;

        return new Vector2(x, y);
    }

    protected override Vector2 MeasureCore(float availableWidth, float availableHeight)
    {
        return Size;
    }

    protected override void RenderCore(SpriteBatch spriteBatch)
    {
        var pixel = UIResources.GetPixelTexture(spriteBatch.GraphicsDevice);
        var pos = ScreenPosition;

        string label = $"{StatName}: {StatValue:F0}";
        var labelSize = UITheme.Font.MeasureString(label);
        spriteBatch.DrawString(UITheme.Font, label, pos, LabelColor);

        var barBounds = CalculateProgressBarBounds(pos, Size, labelSize.X, LabelColumnWidth);
        if (barBounds.Width <= 0 || barBounds.Height <= 0)
        {
            return;
        }

        spriteBatch.Draw(pixel, barBounds, UITheme.StatusBar.ProgressBackground);

        int fillWidth = CalculateFillWidth(barBounds.Width, Progress);
        if (fillWidth > 0)
        {
            spriteBatch.Draw(pixel, new Rectangle(barBounds.X, barBounds.Y, fillWidth, barBounds.Height), BarColor);
        }

        var borderColor = UITheme.Panel.BorderColor;
        spriteBatch.Draw(pixel, new Rectangle(barBounds.X, barBounds.Y, barBounds.Width, 1), borderColor);
        spriteBatch.Draw(pixel, new Rectangle(barBounds.X, barBounds.Bottom - 1, barBounds.Width, 1), borderColor);
        spriteBatch.Draw(pixel, new Rectangle(barBounds.X, barBounds.Y, 1, barBounds.Height), borderColor);
        spriteBatch.Draw(pixel, new Rectangle(barBounds.Right - 1, barBounds.Y, 1, barBounds.Height), borderColor);

        string percentText = $"{Progress:F0}%";
        var percentSize = UITheme.Font.MeasureString(percentText);
        if (!ShouldRenderPercentText(barBounds, percentSize))
        {
            return;
        }

        var percentPosition = CalculatePercentTextPosition(barBounds, percentSize);
        spriteBatch.DrawString(UITheme.Font, percentText, percentPosition + Vector2.One, UITheme.Text.Shadow * 0.5f);
        spriteBatch.DrawString(UITheme.Font, percentText, percentPosition, UITheme.Text.Primary);
    }
}

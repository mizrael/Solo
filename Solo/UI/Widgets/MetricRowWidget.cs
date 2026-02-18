using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Solo.UI.Widgets;

public class MetricRowWidget : Widget
{
    public MetricRowWidget()
    {
    }

    public string Label { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public Color LabelColor { get; set; } = UITheme.Text.Secondary;
    public Color ValueColor { get; set; } = UITheme.Text.Primary;

    protected override Vector2 MeasureCore(float availableWidth, float availableHeight)
    {
        return Size;
    }

    protected override void RenderCore(SpriteBatch spriteBatch)
    {
        var pos = ScreenPosition;

        // Label on left
        spriteBatch.DrawString(UITheme.Font, Label, pos, LabelColor);

        // Value on right
        if (!string.IsNullOrEmpty(Value))
        {
            var valueSize = UITheme.Font.MeasureString(Value);
            float valueX = pos.X + Size.X - valueSize.X;
            spriteBatch.DrawString(UITheme.Font, Value, new Vector2(valueX, pos.Y), ValueColor);
        }
    }
}

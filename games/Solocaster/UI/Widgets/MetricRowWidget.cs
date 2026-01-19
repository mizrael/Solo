using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Solocaster.UI.Widgets;

public class MetricRowWidget : Widget
{
    public MetricRowWidget()
    {
    }

    public string Label { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public SpriteFont? Font { get; set; }
    public Color LabelColor { get; set; } = Color.LightGray;
    public Color ValueColor { get; set; } = Color.White;

    protected override void RenderCore(SpriteBatch spriteBatch)
    {
        if (Font == null)
            return;

        var pos = ScreenPosition;

        // Label on left
        spriteBatch.DrawString(Font, Label, pos, LabelColor);

        // Value on right
        if (!string.IsNullOrEmpty(Value))
        {
            var valueSize = Font.MeasureString(Value);
            float valueX = pos.X + Size.X - valueSize.X;
            spriteBatch.DrawString(Font, Value, new Vector2(valueX, pos.Y), ValueColor);
        }
    }
}

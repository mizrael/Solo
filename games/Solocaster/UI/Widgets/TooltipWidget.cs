using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Solocaster.UI.Widgets;

public class TooltipWidget : PanelWidget
{
    private const int Padding = 8;

    public TooltipWidget()
    {
        BackgroundColor = new Color(20, 20, 25, 250);
        BorderColor = new Color(100, 80, 60);
        BorderWidth = 2;
        Visible = false;
    }

    public string Text { get; set; } = string.Empty;
    public SpriteFont? Font { get; set; }
    public Color TextColor { get; set; } = Color.White;

    public void UpdateSize()
    {
        if (Font == null || string.IsNullOrEmpty(Text))
        {
            Size = Vector2.Zero;
            return;
        }

        var textSize = Font.MeasureString(Text);
        Size = new Vector2(textSize.X + Padding * 2, textSize.Y + Padding * 2);
    }

    protected override void RenderCore(SpriteBatch spriteBatch)
    {
        if (string.IsNullOrEmpty(Text))
            return;

        base.RenderCore(spriteBatch);

        if (Font != null)
        {
            var textPos = ScreenPosition + new Vector2(Padding, Padding);
            spriteBatch.DrawString(Font, Text, textPos, TextColor);
        }
    }
}

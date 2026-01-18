using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Solocaster.UI.Widgets;

public class LabelWidget : Widget
{
    public LabelWidget()
    {
    }

    public string Text { get; set; } = string.Empty;
    public SpriteFont? Font { get; set; }
    public Color TextColor { get; set; } = Color.White;
    public bool CenterHorizontally { get; set; } = false;
    public bool CenterVertically { get; set; } = false;

    protected override void RenderCore(SpriteBatch spriteBatch)
    {
        if (Font == null || string.IsNullOrEmpty(Text))
            return;

        var textSize = Font.MeasureString(Text);
        var position = ScreenPosition;

        if (CenterHorizontally)
            position.X += (Size.X - textSize.X) / 2;

        if (CenterVertically)
            position.Y += (Size.Y - textSize.Y) / 2;

        spriteBatch.DrawString(Font, Text, position, TextColor);
    }
}

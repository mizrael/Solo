using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Solocaster.Character;
using Solocaster.UI;

namespace Solocaster.UI.Widgets;

public class StatProgressRowWidget : Widget
{
    private static Texture2D? _pixelTexture;
    private const int ProgressBarWidth = 100;
    private const int ProgressBarHeight = 10;

    public StatProgressRowWidget()
    {
    }

    public string StatName { get; set; } = string.Empty;
    public float StatValue { get; set; }
    public float Progress { get; set; }
    public Stats StatType { get; set; }
    public SpriteFont? Font { get; set; }
    public Color LabelColor { get; set; } = UITheme.Text.Secondary;

    private static Texture2D GetPixelTexture(GraphicsDevice graphicsDevice)
    {
        if (_pixelTexture == null)
        {
            _pixelTexture = new Texture2D(graphicsDevice, 1, 1);
            _pixelTexture.SetData(new[] { Color.White });
        }
        return _pixelTexture;
    }

    protected override void RenderCore(SpriteBatch spriteBatch)
    {
        if (Font == null)
            return;

        var pixel = GetPixelTexture(spriteBatch.GraphicsDevice);
        var pos = ScreenPosition;

        // Draw stat name and value
        string label = $"{StatName}: {StatValue:F0}";
        spriteBatch.DrawString(Font, label, pos, LabelColor);

        // Draw progress bar
        int barX = (int)(pos.X + Size.X - ProgressBarWidth);
        int barY = (int)pos.Y + 4;

        // Background
        spriteBatch.Draw(pixel, new Rectangle(barX, barY, ProgressBarWidth, ProgressBarHeight), UITheme.StatusBar.ProgressBackground);

        // Fill
        int fillWidth = (int)(ProgressBarWidth * (Progress / 100f));
        if (fillWidth > 0)
        {
            var fillColor = UITheme.Stats.GetColorForStat(StatType);
            spriteBatch.Draw(pixel, new Rectangle(barX, barY, fillWidth, ProgressBarHeight), fillColor);
        }

        // Border
        var borderColor = UITheme.Panel.BorderColor;
        spriteBatch.Draw(pixel, new Rectangle(barX, barY, ProgressBarWidth, 1), borderColor);
        spriteBatch.Draw(pixel, new Rectangle(barX, barY + ProgressBarHeight - 1, ProgressBarWidth, 1), borderColor);
        spriteBatch.Draw(pixel, new Rectangle(barX, barY, 1, ProgressBarHeight), borderColor);
        spriteBatch.Draw(pixel, new Rectangle(barX + ProgressBarWidth - 1, barY, 1, ProgressBarHeight), borderColor);

        // Progress percentage
        string percentText = $"{Progress:F0}%";
        var percentSize = Font.MeasureString(percentText);
        float percentX = barX + (ProgressBarWidth - percentSize.X) / 2;
        // Draw with shadow for readability
        spriteBatch.DrawString(Font, percentText, new Vector2(percentX + 1, pos.Y + 1), UITheme.Text.Shadow * 0.5f);
        spriteBatch.DrawString(Font, percentText, new Vector2(percentX, pos.Y), UITheme.Text.Primary);
    }
}

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Solo.UI.Widgets;

public class ProgressBarWidget : Widget
{
    public ProgressBarWidget()
    {
    }

    public float Progress { get; set; }
    public float MaxProgress { get; set; } = 100f;
    public Color FillColor { get; set; } = UITheme.StatusBar.ProgressFill;
    public Color BackgroundColor { get; set; } = UITheme.StatusBar.ProgressBackground;
    public Color BorderColor { get; set; } = UITheme.Panel.BorderColor;
    public string? OverlayText { get; set; }
    public Color TextColor { get; set; } = UITheme.Text.Primary;
    public Color TextShadowColor { get; set; } = UITheme.Text.Shadow;

    protected override Vector2 MeasureCore(float availableWidth, float availableHeight)
    {
        return Size;
    }

    protected override void RenderCore(SpriteBatch spriteBatch)
    {
        var pixel = UIResources.GetPixelTexture(spriteBatch.GraphicsDevice);
        var bounds = Bounds;

        // Background
        spriteBatch.Draw(pixel, bounds, BackgroundColor);

        // Fill
        float ratio = MaxProgress > 0 ? Progress / MaxProgress : 0;
        int fillWidth = (int)(bounds.Width * Math.Clamp(ratio, 0, 1));
        if (fillWidth > 0)
        {
            spriteBatch.Draw(pixel, new Rectangle(bounds.X, bounds.Y, fillWidth, bounds.Height), FillColor);
        }

        // Border
        spriteBatch.Draw(pixel, new Rectangle(bounds.X, bounds.Y, bounds.Width, 1), BorderColor);
        spriteBatch.Draw(pixel, new Rectangle(bounds.X, bounds.Bottom - 1, bounds.Width, 1), BorderColor);
        spriteBatch.Draw(pixel, new Rectangle(bounds.X, bounds.Y, 1, bounds.Height), BorderColor);
        spriteBatch.Draw(pixel, new Rectangle(bounds.Right - 1, bounds.Y, 1, bounds.Height), BorderColor);

        // Overlay text
        if (!string.IsNullOrEmpty(OverlayText))
        {
            var textSize = UITheme.Font.MeasureString(OverlayText);
            float textX = bounds.X + (bounds.Width - textSize.X) / 2;
            float textY = bounds.Y + (bounds.Height - textSize.Y) / 2;

            // Shadow
            spriteBatch.DrawString(UITheme.Font, OverlayText, new Vector2(textX + 1, textY + 1), TextShadowColor);
            // Text
            spriteBatch.DrawString(UITheme.Font, OverlayText, new Vector2(textX, textY), TextColor);
        }
    }
}

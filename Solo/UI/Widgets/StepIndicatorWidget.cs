using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Solo.UI.Widgets;

public class StepIndicatorWidget : Widget
{
    public StepIndicatorWidget()
    {
    }

    public List<string> Steps { get; set; } = new();
    public int CurrentStep { get; set; }
    public HashSet<int> CompletedSteps { get; set; } = new();
    public Color ActiveColor { get; set; } = UITheme.Text.Highlight;
    public Color CompletedColor { get; set; } = UITheme.Text.SectionHeader;
    public Color InactiveColor { get; set; } = UITheme.Text.Muted;
    public Color SeparatorColor { get; set; } = UITheme.Text.Placeholder;

    public event Action<int>? OnStepClicked;

    protected override Vector2 MeasureCore(float availableWidth, float availableHeight)
    {
        return new Vector2(availableWidth, UITheme.LineHeight + 8);
    }

    protected override void RenderCore(SpriteBatch spriteBatch)
    {
        if (Steps.Count == 0)
            return;

        float totalWidth = 0;
        var stepWidths = new List<float>();

        foreach (var step in Steps)
        {
            var width = UITheme.Font.MeasureString(step).X;
            stepWidths.Add(width);
            totalWidth += width;
        }

        float separatorWidth = 30;
        totalWidth += (Steps.Count - 1) * separatorWidth;

        float startX = ScreenPosition.X + (Size.X - totalWidth) / 2;
        float y = ScreenPosition.Y + (Size.Y - UITheme.Font.LineSpacing) / 2;

        for (int i = 0; i < Steps.Count; i++)
        {
            Color color;
            if (i == CurrentStep)
                color = ActiveColor;
            else if (CompletedSteps.Contains(i))
                color = CompletedColor;
            else
                color = InactiveColor;

            spriteBatch.DrawString(UITheme.Font, Steps[i], new Vector2(startX, y), color);
            startX += stepWidths[i];

            if (i < Steps.Count - 1)
            {
                var dotX = startX + separatorWidth / 2 - 2;
                var dotY = y + UITheme.Font.LineSpacing / 2 - 2;
                var pixel = UIResources.GetPixelTexture(spriteBatch.GraphicsDevice);
                spriteBatch.Draw(pixel, new Rectangle((int)dotX, (int)dotY, 4, 4), SeparatorColor);
                startX += separatorWidth;
            }
        }
    }

    protected override void OnMouseClick(Point mousePosition)
    {
        if (Steps.Count == 0)
            return;

        float totalWidth = 0;
        var stepWidths = new List<float>();

        foreach (var step in Steps)
        {
            var width = UITheme.Font.MeasureString(step).X;
            stepWidths.Add(width);
            totalWidth += width;
        }

        float separatorWidth = 30;
        totalWidth += (Steps.Count - 1) * separatorWidth;

        float startX = ScreenPosition.X + (Size.X - totalWidth) / 2;
        float y = ScreenPosition.Y;

        for (int i = 0; i < Steps.Count; i++)
        {
            var stepBounds = new Rectangle((int)startX, (int)y, (int)stepWidths[i], (int)Size.Y);
            if (stepBounds.Contains(mousePosition) && CompletedSteps.Contains(i))
            {
                OnStepClicked?.Invoke(i);
                return;
            }
            startX += stepWidths[i] + separatorWidth;
        }
    }
}

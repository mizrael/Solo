using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Solocaster.UI.Widgets;

public class StepIndicatorWidget : Widget
{
    private static Texture2D? _pixelTexture;

    public StepIndicatorWidget()
    {
    }

    public List<string> Steps { get; set; } = new();
    public int CurrentStep { get; set; }
    public HashSet<int> CompletedSteps { get; set; } = new();
    public SpriteFont? Font { get; set; }
    public Color ActiveColor { get; set; } = new Color(200, 180, 140);
    public Color CompletedColor { get; set; } = new Color(150, 150, 150);
    public Color InactiveColor { get; set; } = new Color(80, 80, 80);
    public Color SeparatorColor { get; set; } = new Color(100, 100, 100);

    public event Action<int>? OnStepClicked;

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
        if (Font == null || Steps.Count == 0)
            return;

        float totalWidth = 0;
        var stepWidths = new List<float>();

        foreach (var step in Steps)
        {
            var width = Font.MeasureString(step).X;
            stepWidths.Add(width);
            totalWidth += width;
        }

        float separatorWidth = 30;
        totalWidth += (Steps.Count - 1) * separatorWidth;

        float startX = ScreenPosition.X + (Size.X - totalWidth) / 2;
        float y = ScreenPosition.Y + (Size.Y - Font.LineSpacing) / 2;

        for (int i = 0; i < Steps.Count; i++)
        {
            Color color;
            if (i == CurrentStep)
                color = ActiveColor;
            else if (CompletedSteps.Contains(i))
                color = CompletedColor;
            else
                color = InactiveColor;

            spriteBatch.DrawString(Font, Steps[i], new Vector2(startX, y), color);
            startX += stepWidths[i];

            if (i < Steps.Count - 1)
            {
                var dotX = startX + separatorWidth / 2 - 2;
                var dotY = y + Font.LineSpacing / 2 - 2;
                var pixel = GetPixelTexture(spriteBatch.GraphicsDevice);
                spriteBatch.Draw(pixel, new Rectangle((int)dotX, (int)dotY, 4, 4), SeparatorColor);
                startX += separatorWidth;
            }
        }
    }

    protected override void OnMouseClick(Point mousePosition)
    {
        if (Font == null || Steps.Count == 0)
            return;

        float totalWidth = 0;
        var stepWidths = new List<float>();

        foreach (var step in Steps)
        {
            var width = Font.MeasureString(step).X;
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

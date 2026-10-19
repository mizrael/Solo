using Microsoft.Xna.Framework;
using Solo.UI.Layout;
using Solo.UI.Widgets;
using System.Text;

namespace Solo.Tests.UI.Layout;

/// <summary>Every character is <see cref="CharWidth"/> wide.</summary>
internal sealed class FakeTextMeasurer : ITextMeasurer
{
    public const float CharWidth = 10;

    public float LineHeight { get; init; } = 20;
    public int MeasureCount { get; private set; }

    public float MeasureWidth(StringBuilder text)
    {
        MeasureCount++;
        return text.Length * CharWidth;
    }
}

internal sealed class FixedSizeWidget : Widget
{
    public FixedSizeWidget(float width, float height)
    {
        Size = new Vector2(width, height);
    }
}

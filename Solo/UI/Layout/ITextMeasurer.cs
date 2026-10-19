using System.Text;

namespace Solo.UI.Layout;

/// <summary>
/// Measures text without needing a GraphicsDevice, so layout can be unit tested with a fake.
/// </summary>
public interface ITextMeasurer
{
    float LineHeight { get; }

    float MeasureWidth(StringBuilder text);
}

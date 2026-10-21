using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Solo.UI.Widgets;

namespace Solo.Tests.UI.Widgets;

internal sealed class ProbeWidget : Widget
{
    public string Name { get; init; } = string.Empty;
    public List<string>? Log { get; init; }
    public Action? OnUpdate { get; set; }
    public int UpdateCount { get; private set; }

    protected override void UpdateCore(GameTime gameTime, MouseState mouseState, MouseState previousMouseState)
    {
        UpdateCount++;
        Log?.Add(Name);
        OnUpdate?.Invoke();
    }
}

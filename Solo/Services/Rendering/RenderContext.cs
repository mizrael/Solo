using Microsoft.Xna.Framework.Graphics;

namespace Solo.Services.Rendering;

public struct RenderContext
{
    public GraphicsDevice Graphics { get; init; }
    public SpriteBatch SpriteBatch { get; init; }
    public SortedList<int, IList<IRenderable>> Layers { get; init; }
    public Dictionary<int, RenderLayerConfig> LayerConfigs { get; init; }
    public int CurrentLayerIndex { get; set; }
}

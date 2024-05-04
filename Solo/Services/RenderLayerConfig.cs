using Microsoft.Xna.Framework.Graphics;

namespace Solo.Services;

public record struct RenderLayerConfig
{
    public RenderLayerConfig()
    {
    }

    public SamplerState? SamplerState { get; set; } = null;
}
using Microsoft.Xna.Framework.Graphics;

namespace Monoroids.Core.Services;

public record struct RenderLayerConfig
{
    public RenderLayerConfig()
    {
    }

    public SamplerState? SamplerState { get; set; } = null;
}
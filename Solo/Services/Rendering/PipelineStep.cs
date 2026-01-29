using Microsoft.Xna.Framework.Graphics;

namespace Solo.Services.Rendering;

public abstract class PipelineStep
{
    public RenderTarget2D? Output { get; init; }

    public abstract RenderTarget2D? Execute(ref RenderContext context, RenderTarget2D? input);
}

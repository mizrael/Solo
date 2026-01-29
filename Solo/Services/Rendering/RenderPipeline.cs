using Microsoft.Xna.Framework.Graphics;

namespace Solo.Services.Rendering;

public class RenderPipeline
{
    private readonly List<PipelineStep> _steps = new();

    public RenderPipeline Add(PipelineStep step)
    {
        _steps.Add(step);
        return this;
    }

    public void Execute(ref RenderContext context)
    {
        RenderTarget2D? currentOutput = null;

        foreach (var step in _steps)
        {
            currentOutput = step.Execute(ref context, currentOutput);
        }
    }
}

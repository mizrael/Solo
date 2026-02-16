using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Solo.Services.Rendering;

public class RenderLayersStep : PipelineStep
{
    public int? LayerEnd { get; init; }
    public bool ClearTarget { get; init; } = true;

    public override RenderTarget2D? Execute(ref RenderContext ctx, RenderTarget2D? input)
    {
        ctx.Graphics.SetRenderTarget(Output);

        if (ClearTarget)
            ctx.Graphics.Clear(Color.Black);

        while (ctx.CurrentLayerIndex < ctx.Layers.Count)
        {
            var layerIndex = ctx.Layers.Keys[ctx.CurrentLayerIndex];
            if (LayerEnd.HasValue && layerIndex >= LayerEnd.Value)
                break;

            RenderLayer(ref ctx, layerIndex);
            ctx.CurrentLayerIndex++;
        }

        return Output;
    }

    private static void RenderLayer(ref RenderContext ctx, int layerIndex)
    {
        var layer = ctx.Layers[layerIndex];

        if (!ctx.LayerConfigs.TryGetValue(layerIndex, out var config))
            ctx.SpriteBatch.Begin();
        else
            ctx.SpriteBatch.Begin(
                samplerState: config.SamplerState,
                transformMatrix: config.TransformMatrix);

        foreach (var renderable in layer)
            renderable.Render(ctx.SpriteBatch);

        ctx.SpriteBatch.End();
    }
}

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Solo.Services.Rendering;

public class ApplyEffectStep : PipelineStep
{
    public Effect? Effect { get; init; }

    public override RenderTarget2D? Execute(ref RenderContext ctx, RenderTarget2D? input)
    {
        if (input == null)
            return Output;

        ctx.Graphics.SetRenderTarget(Output);

        var destRect = Output != null
            ? new Rectangle(0, 0, Output.Width, Output.Height)
            : ctx.Graphics.Viewport.Bounds;

        ctx.SpriteBatch.Begin(effect: Effect);
        ctx.SpriteBatch.Draw(input, destRect, Color.White);
        ctx.SpriteBatch.End();

        return Output;
    }
}

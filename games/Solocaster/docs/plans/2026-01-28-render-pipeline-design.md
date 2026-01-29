# Design: Render Pipeline with Effects

## Problem

Modal overlay scenes (character panel, metrics) show a black background instead of the gameplay. We want to capture the gameplay, apply blur/darken effects, and render the UI on top.

The current RenderService renders layers directly to screen with no support for:
- Render targets (textures)
- Post-processing effects
- Multi-stage rendering

## Solution

Refactor RenderService to support a configurable render pipeline. The pipeline is a sequence of steps, where each step either renders layers or applies an effect.

## Architecture

### Pipeline Steps

```csharp
public abstract class PipelineStep
{
    public RenderTarget2D? Output { get; init; }  // null = screen

    public abstract RenderTarget2D? Execute(PipelineContext context, RenderTarget2D? input);
}
```

Two concrete step types:

**RenderLayersStep** - Renders a range of layers to the output target:

```csharp
public class RenderLayersStep : PipelineStep
{
    public int? LayerEnd { get; init; }       // null = all remaining
    public bool ClearTarget { get; init; } = true;

    public override RenderTarget2D? Execute(PipelineContext ctx, RenderTarget2D? input)
    {
        ctx.Graphics.SetRenderTarget(Output);

        if (ClearTarget)
            ctx.Graphics.Clear(Color.Black);

        while (ctx.CurrentLayerIndex < ctx.Layers.Count)
        {
            var layerIndex = ctx.Layers.Keys[ctx.CurrentLayerIndex];
            if (LayerEnd.HasValue && layerIndex >= LayerEnd.Value)
                break;

            RenderLayer(ctx, layerIndex);
            ctx.CurrentLayerIndex++;
        }

        return Output;
    }
}
```

**ApplyEffectStep** - Applies a shader effect to the input texture:

```csharp
public class ApplyEffectStep : PipelineStep
{
    public Effect Effect { get; init; }

    public override RenderTarget2D? Execute(PipelineContext ctx, RenderTarget2D? input)
    {
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
```

### Pipeline Context

Shared state passed to each step:

```csharp
public class PipelineContext
{
    public GraphicsDevice Graphics { get; init; }
    public SpriteBatch SpriteBatch { get; init; }
    public SortedList<int, IList<IRenderable>> Layers { get; init; }
    public Dictionary<int, RenderLayerConfig> LayerConfigs { get; init; }
    public int CurrentLayerIndex { get; set; }
}
```

### RenderPipeline

Holds and executes the sequence of steps:

```csharp
public class RenderPipeline
{
    private readonly List<PipelineStep> _steps = new();

    public RenderPipeline Add(PipelineStep step)
    {
        _steps.Add(step);
        return this;
    }

    public void Execute(PipelineContext context)
    {
        RenderTarget2D? currentOutput = null;

        foreach (var step in _steps)
        {
            currentOutput = step.Execute(context, currentOutput);
        }
    }
}
```

### Updated RenderService

```csharp
public sealed class RenderService : IGameService
{
    private RenderPipeline? _pipeline;

    public void SetPipeline(RenderPipeline? pipeline)
    {
        _pipeline = pipeline;
    }

    public void Render()
    {
        if (_pipeline == null)
        {
            RenderDirect();  // Current behavior - no pipeline
            return;
        }

        var context = new PipelineContext
        {
            Graphics = _graphicsDevice,
            SpriteBatch = _spriteBatch,
            Layers = _layers,
            LayerConfigs = _layerConfigs,
            CurrentLayerIndex = 0
        };

        _pipeline.Execute(context);
    }

    private void RenderDirect()
    {
        // Existing implementation - renders all layers to screen
    }
}
```

## Usage Examples

### Default (no pipeline)

```csharp
// No pipeline set - renders all layers directly to screen
renderService.SetPipeline(null);
```

### Blur Overlay Background

```csharp
var gameplayTarget = new RenderTarget2D(graphics, width, height);

var pipeline = new RenderPipeline()
    .Add(new RenderLayersStep { LayerEnd = 1000, Output = gameplayTarget })
    .Add(new ApplyEffectStep { Effect = blurEffect, Output = null })
    .Add(new RenderLayersStep { Output = null, ClearTarget = false });

renderService.SetPipeline(pipeline);
```

Flow:
1. Render layers 0-999 (gameplay) → gameplayTarget
2. Apply blur → screen
3. Render layers 1000+ (UI) → screen (no clear, on top)

### Chained Effects (blur + darken)

```csharp
var textureA = new RenderTarget2D(graphics, width, height);
var textureB = new RenderTarget2D(graphics, width, height);

var pipeline = new RenderPipeline()
    .Add(new RenderLayersStep { LayerEnd = 1000, Output = textureA })
    .Add(new ApplyEffectStep { Effect = blurEffect, Output = textureB })
    .Add(new ApplyEffectStep { Effect = darkenEffect, Output = null })
    .Add(new RenderLayersStep { Output = null, ClearTarget = false });
```

Flow:
1. Gameplay → TextureA
2. Blur: TextureA → TextureB
3. Darken: TextureB → screen
4. UI on top (no clear)

### Half-Resolution Blur (performance)

```csharp
var fullRes = new RenderTarget2D(graphics, width, height);
var halfRes = new RenderTarget2D(graphics, width / 2, height / 2);

var pipeline = new RenderPipeline()
    .Add(new RenderLayersStep { LayerEnd = 1000, Output = fullRes })
    .Add(new ApplyEffectStep { Effect = downscaleEffect, Output = halfRes })
    .Add(new ApplyEffectStep { Effect = blurEffect, Output = halfRes })  // in-place at half res
    .Add(new ApplyEffectStep { Effect = upscaleEffect, Output = null })
    .Add(new RenderLayersStep { Output = null, ClearTarget = false });
```

## Design Decisions

### Why ClearTarget flag?

`RenderLayersStep` doesn't use the input parameter because:
- It renders "new content" (layers), not a transformation
- After an effect outputs to screen, we can't read from screen
- `ClearTarget = false` lets UI render on top without clearing

### Why not Option B (all textures)?

We considered always rendering to textures and copying to screen at the end. Rejected because:
- Extra texture allocation
- Redundant final copy to screen
- No benefit for common use cases

### Chaining Model

- **ApplyEffectStep**: Truly chains (input → effect → output)
- **RenderLayersStep**: Doesn't chain - either clears or renders on top

This matches semantics: effects transform content, layers add new content.

## Files

### Solo Engine (new/modified)

| File | Action |
|------|--------|
| `Services/RenderPipeline.cs` | Create |
| `Services/PipelineStep.cs` | Create |
| `Services/PipelineContext.cs` | Create |
| `Services/RenderLayersStep.cs` | Create |
| `Services/ApplyEffectStep.cs` | Create |
| `Services/RenderService.cs` | Modify - add pipeline support |

### Solocaster (for blur overlay feature)

| File | Action |
|------|--------|
| `Content/Effects/Blur.fx` | Create - blur shader |
| `Scenes/CharacterPanelScene.cs` | Modify - set up blur pipeline |
| `Scenes/MetricsPanelScene.cs` | Modify - set up blur pipeline |

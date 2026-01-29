# Render Pipeline Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Add render pipeline support to RenderService for post-processing effects like blur overlays.

**Architecture:** Pipeline is a sequence of steps. Each step renders layers or applies effects. Steps chain via input/output textures.

**Tech Stack:** MonoGame, C#, HLSL shaders

---

## Task 1: Create PipelineContext

**Files:**
- Create: `Solo/Services/Rendering/PipelineContext.cs`

**Step 1: Create the file**

```csharp
using Microsoft.Xna.Framework.Graphics;

namespace Solo.Services.Rendering;

public class PipelineContext
{
    public required GraphicsDevice Graphics { get; init; }
    public required SpriteBatch SpriteBatch { get; init; }
    public required SortedList<int, IList<IRenderable>> Layers { get; init; }
    public required Dictionary<int, RenderLayerConfig> LayerConfigs { get; init; }
    public int CurrentLayerIndex { get; set; }
}
```

**Step 2: Build**

Run: `dotnet build Solo.sln`
Expected: 0 errors

---

## Task 2: Create PipelineStep Base Class

**Files:**
- Create: `Solo/Services/Rendering/PipelineStep.cs`

**Step 1: Create the file**

```csharp
using Microsoft.Xna.Framework.Graphics;

namespace Solo.Services.Rendering;

public abstract class PipelineStep
{
    public RenderTarget2D? Output { get; init; }

    public abstract RenderTarget2D? Execute(PipelineContext context, RenderTarget2D? input);
}
```

**Step 2: Build**

Run: `dotnet build Solo.sln`
Expected: 0 errors

---

## Task 3: Create RenderLayersStep

**Files:**
- Create: `Solo/Services/Rendering/RenderLayersStep.cs`

**Step 1: Create the file**

```csharp
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Solo.Services.Rendering;

public class RenderLayersStep : PipelineStep
{
    public int? LayerEnd { get; init; }
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

    private static void RenderLayer(PipelineContext ctx, int layerIndex)
    {
        var layer = ctx.Layers[layerIndex];

        if (!ctx.LayerConfigs.TryGetValue(layerIndex, out var config))
            ctx.SpriteBatch.Begin();
        else
            ctx.SpriteBatch.Begin(samplerState: config.SamplerState);

        foreach (var renderable in layer)
            renderable.Render(ctx.SpriteBatch);

        ctx.SpriteBatch.End();
    }
}
```

**Step 2: Build**

Run: `dotnet build Solo.sln`
Expected: 0 errors

---

## Task 4: Create ApplyEffectStep

**Files:**
- Create: `Solo/Services/Rendering/ApplyEffectStep.cs`

**Step 1: Create the file**

```csharp
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Solo.Services.Rendering;

public class ApplyEffectStep : PipelineStep
{
    public Effect? Effect { get; init; }

    public override RenderTarget2D? Execute(PipelineContext ctx, RenderTarget2D? input)
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
```

**Step 2: Build**

Run: `dotnet build Solo.sln`
Expected: 0 errors

---

## Task 5: Create RenderPipeline

**Files:**
- Create: `Solo/Services/Rendering/RenderPipeline.cs`

**Step 1: Create the file**

```csharp
namespace Solo.Services.Rendering;

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

**Step 2: Build**

Run: `dotnet build Solo.sln`
Expected: 0 errors

---

## Task 6: Update RenderService with Pipeline Support

**Files:**
- Modify: `Solo/Services/RenderService.cs`

**Step 1: Add pipeline field and setter**

Add after existing fields:

```csharp
private RenderPipeline? _pipeline;

public void SetPipeline(RenderPipeline? pipeline)
{
    _pipeline = pipeline;
}
```

**Step 2: Add using directive**

Add at top:

```csharp
using Solo.Services.Rendering;
```

**Step 3: Update Render method**

Replace existing `Render()` method:

```csharp
public void Render()
{
    if (_pipeline == null)
    {
        RenderDirect();
        return;
    }

    RenderWithPipeline();
}

private void RenderDirect()
{
    _graphicsDevice.Clear(Color.Black);

    for (int i = 0; i != _layers.Count; i++)
    {
        var layerIndex = _layers.Keys[i];
        var layer = _layers[layerIndex];

        if (!_layerConfigs.TryGetValue(layerIndex, out var layerConfig))
            _spriteBatch.Begin();
        else
            _spriteBatch.Begin(samplerState: layerConfig.SamplerState);

        foreach (var renderable in layer)
            renderable.Render(_spriteBatch);

        _spriteBatch.End();
    }
}

private void RenderWithPipeline()
{
    var context = new PipelineContext
    {
        Graphics = _graphicsDevice,
        SpriteBatch = _spriteBatch,
        Layers = _layers,
        LayerConfigs = _layerConfigs,
        CurrentLayerIndex = 0
    };

    _pipeline!.Execute(context);
}
```

**Step 4: Build**

Run: `dotnet build Solo.sln`
Expected: 0 errors

---

## Task 7: Create Blur Shader

**Files:**
- Create: `games/Solocaster/Content/Effects/Blur.fx`

**Step 1: Create the shader file**

```hlsl
#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0_level_9_1
#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

sampler2D TextureSampler : register(s0);

float2 TexelSize;
float BlurAmount = 1.0;

float4 MainPS(float2 texCoord : TEXCOORD0) : COLOR0
{
    float4 color = float4(0, 0, 0, 0);

    // 9-tap Gaussian blur
    float weights[9] = { 0.05, 0.09, 0.12, 0.15, 0.18, 0.15, 0.12, 0.09, 0.05 };
    float offsets[9] = { -4, -3, -2, -1, 0, 1, 2, 3, 4 };

    for (int i = 0; i < 9; i++)
    {
        float2 offset = float2(offsets[i] * TexelSize.x * BlurAmount, offsets[i] * TexelSize.y * BlurAmount);
        color += tex2D(TextureSampler, texCoord + offset) * weights[i];
    }

    // Darken
    color.rgb *= 0.4;

    return color;
}

technique Blur
{
    pass P0
    {
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
}
```

**Step 2: Add to Content.mgcb**

Add to `games/Solocaster/Content/Content.mgcb`:

```
#begin Effects/Blur.fx
/importer:EffectImporter
/processor:EffectProcessor
/build:Effects/Blur.fx
```

**Step 3: Build**

Run: `dotnet build games/Solocaster/Solocaster.csproj`
Expected: 0 errors

---

## Task 8: Update CharacterPanelScene with Blur Pipeline

**Files:**
- Modify: `games/Solocaster/Scenes/CharacterPanelScene.cs`

**Step 1: Add fields for pipeline resources**

Add fields:

```csharp
private RenderTarget2D? _gameplayTarget;
private Effect? _blurEffect;
```

**Step 2: Load effect and create pipeline in EnterCore**

Update `EnterCore()` to load the blur effect, create render target, and set up pipeline:

```csharp
// Load blur effect
_blurEffect = Game.Content.Load<Effect>("Effects/Blur");

// Create render target for gameplay capture
var viewport = RenderService.Instance.Graphics.Viewport;
_gameplayTarget = new RenderTarget2D(RenderService.Instance.Graphics, viewport.Width, viewport.Height);

// Set blur shader parameters
_blurEffect.Parameters["TexelSize"]?.SetValue(new Vector2(1f / viewport.Width, 1f / viewport.Height));
_blurEffect.Parameters["BlurAmount"]?.SetValue(2f);

// Create pipeline
var pipeline = new RenderPipeline()
    .Add(new RenderLayersStep { LayerEnd = RenderLayers.UI, Output = _gameplayTarget })
    .Add(new ApplyEffectStep { Effect = _blurEffect, Output = null })
    .Add(new RenderLayersStep { Output = null, ClearTarget = false });

RenderService.Instance.SetPipeline(pipeline);
```

**Step 3: Add using directives**

Add at top:

```csharp
using Solo.Services.Rendering;
using Microsoft.Xna.Framework.Graphics;
```

**Step 4: Clear pipeline in ExitCore**

Override `ExitCore()` to clean up:

```csharp
protected override void ExitCore()
{
    RenderService.Instance.SetPipeline(null);
    _gameplayTarget?.Dispose();
    _gameplayTarget = null;
}
```

**Step 5: Build**

Run: `dotnet build games/Solocaster/Solocaster.csproj`
Expected: 0 errors

---

## Task 9: Update MetricsPanelScene with Blur Pipeline

**Files:**
- Modify: `games/Solocaster/Scenes/MetricsPanelScene.cs`

**Step 1: Add same pipeline setup as CharacterPanelScene**

Follow same pattern as Task 8:
- Add fields for `_gameplayTarget` and `_blurEffect`
- Load effect and create pipeline in `EnterCore()`
- Clear pipeline and dispose target in `ExitCore()`

**Step 2: Build**

Run: `dotnet build games/Solocaster/Solocaster.csproj`
Expected: 0 errors

---

## Task 10: Final Build and Test

**Step 1: Build the solution**

Run: `dotnet build Solo.sln`
Expected: 0 errors

**Step 2: Run the game**

Run: `dotnet run --project games/Solocaster/Solocaster.csproj`

Expected:
- Game starts normally
- Press Tab: Character panel opens with blurred/darkened gameplay visible behind
- Press Tab again: Panel closes, gameplay resumes without blur
- Press C: Metrics panel opens with blurred background
- Press C again: Panel closes, gameplay resumes

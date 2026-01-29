using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Solo.Services.Rendering;

namespace Solo.Services;

public sealed class RenderService : IGameService
{
    private readonly RenderPipeline _defaultPipeline;
    private RenderPipeline _pipeline;
    private RenderContext _context;

    public RenderService(GraphicsDevice graphicsDevice)
    {
        ArgumentNullException.ThrowIfNull(graphicsDevice);

        _context = new RenderContext
        {
            Graphics = graphicsDevice,
            SpriteBatch = new SpriteBatch(graphicsDevice),
            Layers = new SortedList<int, IList<IRenderable>>(),
            LayerConfigs = new Dictionary<int, RenderLayerConfig>(),
            CurrentLayerIndex = 0
        };

        _defaultPipeline = new RenderPipeline().Add(new RenderLayersStep());
        _pipeline = _defaultPipeline;
    }

    public void SetLayerConfig(int index, RenderLayerConfig? layerConfig)
    {
        if (layerConfig is null)
        {
            _context.LayerConfigs.Remove(index);
            return;
        }

        _context.LayerConfigs[index] = layerConfig.Value;
    }

    public void SetPipeline(RenderPipeline? pipeline)
    {
        _pipeline = pipeline ?? _defaultPipeline;
    }

    public void Render()
    {
        _context.CurrentLayerIndex = 0;
        _pipeline.Execute(ref _context);
    }

    public void Update(GameTime gameTime)
    {
        var layers = _context.Layers;

        foreach (var layerIndex in layers.Keys)
            layers[layerIndex].Clear();

        var currentScene = SceneManager.Instance.Current;
        if (currentScene is null)
            return;

        RebuildSceneServicesLayers(currentScene, layers);
        RebuildGameObjectsLayers(currentScene.ObjectsGraph.Root, layers);
    }

    private static void RebuildSceneServicesLayers(Scene currentScene, SortedList<int, IList<IRenderable>> layers)
    {
        foreach (var service in currentScene.Services)
        {
            if (service is IRenderable renderableService)
            {
                if (renderableService.Hidden)
                    continue;

                if (!layers.ContainsKey(renderableService.LayerIndex))
                    layers.Add(renderableService.LayerIndex, new List<IRenderable>());
                layers[renderableService.LayerIndex].Add(renderableService);
            }
        }
    }

    private static void RebuildGameObjectsLayers(GameObject? node, SortedList<int, IList<IRenderable>> layers)
    {
        if (node == null || !node.Enabled)
            return;

        foreach (var component in node.Components)
            if (component is IRenderable renderable &&
                component.Initialized &&
                component.Owner.Enabled &&
                !renderable.Hidden)
            {
                if (!layers.ContainsKey(renderable.LayerIndex))
                    layers.Add(renderable.LayerIndex, new List<IRenderable>());
                layers[renderable.LayerIndex].Add(renderable);
            }

        foreach (var child in node.Children)
            RebuildGameObjectsLayers(child, layers);
    }
}

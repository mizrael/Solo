using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Solo.Services;

public sealed class RenderService : IGameService
{
    private readonly GraphicsDevice _graphicsDevice;

    private readonly SpriteBatch _spriteBatch;
    private SortedList<int, IList<IRenderable>> _layers = new();
    private Dictionary<int, RenderLayerConfig> _layerConfigs = new();

    public RenderService(GraphicsDevice graphicsDevice)
    {
        _graphicsDevice = graphicsDevice ?? throw new ArgumentNullException(nameof(graphicsDevice));
        _spriteBatch = new SpriteBatch(graphicsDevice);
    }

    public void SetLayerConfig(int index, RenderLayerConfig? layerConfig)
    {
        if (layerConfig is null)
        {
            if (_layerConfigs.ContainsKey(index))
                _layerConfigs.Remove(index);
            return;
        }

        _layerConfigs[index] = layerConfig!.Value;
    }

    public void Render()
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

    public void Update(GameTime gameTime)
    {
        foreach (var layerIndex in _layers.Keys)
        {
            var layer = _layers[layerIndex];
            layer.Clear();
        }

        var currentScene = SceneManager.Instance.Current;
        if (currentScene is null)
            return;

        RebuildSceneServicesLayers(currentScene);

        RebuildGameObjectsLayers(currentScene.ObjectsGraph.Root, _layers);
    }

    private void RebuildSceneServicesLayers(Scene currentScene)
    {
        foreach (var service in currentScene.Services)
        {
            if (service is IRenderable renderableService)
            {
                if (renderableService.Hidden)
                    continue;

                if (!_layers.ContainsKey(renderableService.LayerIndex))
                    _layers.Add(renderableService.LayerIndex, new List<IRenderable>());
                _layers[renderableService.LayerIndex].Add(renderableService);
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

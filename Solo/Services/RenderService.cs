﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Solo.Services;

public class RenderService : IGameService
{
    public readonly GraphicsDeviceManager Graphics;
    public readonly GameWindow Window;

    private readonly SpriteBatch _spriteBatch;
    private SceneManager _sceneManager;
    private SortedList<int, IList<IRenderable>> _layers = new();
    private Dictionary<int, RenderLayerConfig> _layerConfigs = new();

    public RenderService(GraphicsDeviceManager graphics, GameWindow window)
    {
        Graphics = graphics ?? throw new System.ArgumentNullException(nameof(graphics));
        Window = window ?? throw new ArgumentNullException(nameof(window));

        window.ClientSizeChanged += (sender, args) =>
        {
            Graphics.PreferredBackBufferWidth = window.ClientBounds.Width;
            Graphics.PreferredBackBufferHeight = window.ClientBounds.Height;
            Graphics.ApplyChanges();
        };

        _spriteBatch = new SpriteBatch(Graphics.GraphicsDevice);
    }

    public void Initialize()
    {
        _sceneManager = GameServicesManager.Instance.GetRequired<SceneManager>();
    }

    public void SetLayerConfig(int index, RenderLayerConfig? layerConfig)
    {
        if (layerConfig is null)
        {
            if(_layerConfigs.ContainsKey(index))
                _layerConfigs.Remove(index);
            return;
        }

        _layerConfigs[index] = layerConfig!.Value;
    }

    public void Step(GameTime gameTime)
    {
        foreach(var layerIndex in _layers.Keys)
        {
            var layer = _layers[layerIndex];
            layer.Clear();
        }

        RebuildLayers(_sceneManager?.Current?.Root, _layers);
    }

    public void Render()
    {
        Graphics.GraphicsDevice.Clear(Color.Black);

        for(int i=0;i!=_layers.Count;i++)
        {
            var layerIndex = _layers.Keys[i];
            var layer = _layers[layerIndex];
            
            if(!_layerConfigs.TryGetValue(layerIndex, out var layerConfig))                
                _spriteBatch.Begin();            
            else
                _spriteBatch.Begin(samplerState: layerConfig.SamplerState);

            foreach (var renderable in layer)
                renderable.Render(_spriteBatch);

            _spriteBatch.End();
        }                 
    }

    private static void RebuildLayers(GameObject? node, SortedList<int, IList<IRenderable>> layers)
    {
        if (null == node || !node.Enabled)
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
            RebuildLayers(child, layers);
    }
}

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Solo.Services;
using Solo.Services.Rendering;
using Solocaster.Components;
using Solocaster.Services;
using Solocaster.UI;

namespace Solocaster.Scenes;

public class MetricsPanelScene : Scene
{
    private readonly StatsComponent _stats;

    private InputService _inputService;
    private UIService _uiService;
    private RenderTarget2D? _gameplayTarget;
    private Effect? _blurEffect;

    public MetricsPanelScene(Game game, StatsComponent stats) : base(game)
    {
        _stats = stats;
    }
    
    protected override void InitializeCore()
    {
        _inputService = new InputService();
        Services.Add(_inputService);

        _uiService = new UIService();
        Services.Add(_uiService); 
        _renderService.SetLayerConfig(RenderLayers.UI, new RenderLayerConfig
        {
            SamplerState = SamplerState.PointClamp
        });

        var font = Game.Content.Load<SpriteFont>("Font");
        _uiService.SetTooltipFont(font);

        var metricsPanel = new MetricsPanel(_stats, font, Game);
        metricsPanel.CenterOnScreen(
            Game.GraphicsDevice.Viewport.Width,
            Game.GraphicsDevice.Viewport.Height
        );
        metricsPanel.Visible = true;
        metricsPanel.ShowCloseButton = false;
        _uiService.AddWidget(metricsPanel);

        // Load blur effect
        _blurEffect = Game.Content.Load<Effect>("Effects/Blur");

        // Create render target for gameplay capture
        var viewport = Game.GraphicsDevice.Viewport;
        _gameplayTarget = new RenderTarget2D(Game.GraphicsDevice, viewport.Width, viewport.Height);

        // Set blur shader parameters
        _blurEffect.Parameters["TexelSize"]?.SetValue(new Vector2(1f / viewport.Width, 1f / viewport.Height));
        _blurEffect.Parameters["BlurAmount"]?.SetValue(2f);

        // Create pipeline: render gameplay to texture, apply blur to screen, render UI on top
        var pipeline = new RenderPipeline()
            .Add(new RenderLayersStep { LayerEnd = RenderLayers.UI, Output = _gameplayTarget })
            .Add(new ApplyEffectStep { Effect = _blurEffect, Output = null })
            .Add(new RenderLayersStep { Output = null, ClearTarget = false });

        _renderService.SetPipeline(pipeline);
    }

    protected override void ExitCore()
    {
        _renderService.SetPipeline(null);
        _gameplayTarget?.Dispose();
        _gameplayTarget = null;
    }

    protected override void UpdateCore(GameTime gameTime)
    {
        if (_inputService.IsActionPressed(InputActions.ToggleMetrics))
            SceneManager.Instance.PopScene();
    }
}

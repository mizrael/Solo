using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Solo.Services;
using Solocaster.Components;
using Solocaster.Services;
using Solocaster.UI;

namespace Solocaster.Scenes;

public class MetricsPanelScene : Scene
{
    private readonly StatsComponent _stats;

    private InputService _inputService;
    private UIService _uiService;

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
    }

    protected override void UpdateCore(GameTime gameTime)
    {
        if (_inputService.IsActionPressed(InputActions.ToggleMetrics))
            SceneManager.Instance.PopScene();
    }
}

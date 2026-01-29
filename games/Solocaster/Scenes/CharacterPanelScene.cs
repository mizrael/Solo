using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Solo.Services;
using Solo.Services.Rendering;
using Solocaster.Components;
using Solocaster.Services;
using Solocaster.UI;

namespace Solocaster.Scenes;

public class CharacterPanelScene : Scene
{
    private readonly InventoryComponent _inventory;
    private readonly StatsComponent _stats;
    private readonly RenderTarget2D _sceneCapture;

    private InputService _inputService;
    private UIService _uiService;
    private RenderPipeline _pipeline;

    public CharacterPanelScene(Game game, InventoryComponent inventory, StatsComponent stats, RenderTarget2D sceneCapture) : base(game)
    {
        _inventory = inventory;
        _stats = stats;
        _sceneCapture = sceneCapture;
    }

    protected override void InitializeCore()
    {
        _inputService = new InputService();
        Services.Add(_inputService);

        _uiService = new UIService();
        Services.Add(_uiService);

        RenderService.SetLayerConfig(RenderLayers.UI, new RenderLayerConfig
        {
            SamplerState = SamplerState.PointClamp
        });

        var font = Game.Content.Load<SpriteFont>("Font");
        _uiService.SetTooltipFont(font);

        var characterPanel = new CharacterPanel(_inventory, _stats, _uiService.DragDropManager, font, Game);
        characterPanel.CenterOnScreen(
            Game.GraphicsDevice.Viewport.Width,
            Game.GraphicsDevice.Viewport.Height
        );
        characterPanel.Visible = true;
        _uiService.AddWidget(characterPanel);

        var blurEffect = Game.Content.Load<Effect>("Effects/Blur");
        var viewport = Game.GraphicsDevice.Viewport;
        blurEffect.Parameters["TexelSize"]?.SetValue(new Vector2(1f / viewport.Width, 1f / viewport.Height));
        blurEffect.Parameters["BlurAmount"]?.SetValue(2f);
        blurEffect.Parameters["DarkenAmount"]?.SetValue(0.5f);

        _pipeline = new RenderPipeline()
            .Add(new ApplyEffectStep { Effect = blurEffect, Output = null, Input = _sceneCapture })
            .Add(new RenderLayersStep { Output = null, ClearTarget = false });
        RenderService.SetPipeline(_pipeline);
    }

    protected override void UpdateCore(GameTime gameTime)
    {
        if (_inputService.IsActionPressed(InputActions.ToggleCharacterPanel))
            SceneManager.Instance.PopScene();
    }
}

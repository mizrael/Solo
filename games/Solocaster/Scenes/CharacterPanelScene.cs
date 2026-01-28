using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Solo.Services;
using Solocaster.Components;
using Solocaster.Services;
using Solocaster.UI;

namespace Solocaster.Scenes;

public class CharacterPanelScene : Scene
{
    private readonly InventoryComponent _inventory;
    private readonly StatsComponent _stats;

    private InputService _inputService;
    private UIService _uiService;

    public CharacterPanelScene(Game game, InventoryComponent inventory, StatsComponent stats) : base(game)
    {
        _inventory = inventory;
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

        var characterPanel = new CharacterPanel(_inventory, _stats, _uiService.DragDropManager, font, Game);
        characterPanel.CenterOnScreen(
            Game.GraphicsDevice.Viewport.Width,
            Game.GraphicsDevice.Viewport.Height
        );
        characterPanel.Visible = true;
        _uiService.AddWidget(characterPanel);
    }

    protected override void UpdateCore(GameTime gameTime)
    {
        if (_inputService.IsActionPressed(InputActions.ToggleCharacterPanel))
            SceneManager.Instance.PopScene();
    }
}

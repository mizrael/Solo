using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Solo;
using Solo.Services;
using Solocaster.Character;
using Solocaster.State;
using Solocaster.UI;
using Solocaster.UI.CharacterBuilder;

namespace Solocaster.Scenes;

public class CharacterBuilderScene : Scene
{
    private SpriteFont? _font;
    private CharacterBuilderPanel? _builderPanel;

    private UIService _uiService;

    public CharacterBuilderScene(Game game) : base(game)
    {
    }

    protected override void InitializeCore()
    {
        _uiService = new UIService();
        Services.Add(_uiService);

        RenderService.SetLayerConfig(RenderLayers.UI, new RenderLayerConfig
        {
            SamplerState = SamplerState.PointClamp
        });

        _font = Game.Content.Load<SpriteFont>("Font");

        UITheme.Load("./data/ui/theme.json");
        CharacterTemplateLoader.LoadAll("./data/templates/character/");

        GameState.EnsureCharacter();

        _builderPanel = new CharacterBuilderPanel(_font, Game);
        _builderPanel.CenterOnScreen(
            Game.GraphicsDevice.Viewport.Width,
            Game.GraphicsDevice.Viewport.Height
        );
        _builderPanel.OnStartGame += OnStartGame;
        _uiService.AddWidget(_builderPanel);
    }

    private void OnStartGame()
    {
        SceneManager.Instance.SetScene(SceneNames.Play);
    }
}

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
    private UIService? _uiService;
    private SpriteFont? _font;
    private CharacterBuilderPanel? _builderPanel;

    public CharacterBuilderScene(Game game) : base(game)
    {
    }

    protected override void EnterCore()
    {
        _uiService = GameServicesManager.Instance.GetRequired<UIService>();
        _uiService.ClearWidgets();

        var renderService = GameServicesManager.Instance.GetRequired<RenderService>();

        _font = Game.Content.Load<SpriteFont>("Font");

        UITheme.Load("./data/ui/theme.json");
        CharacterTemplateLoader.LoadAll("./data/templates/character/");

        GameState.EnsureCharacter();

        _builderPanel = new CharacterBuilderPanel(_font, Game);
        _builderPanel.CenterOnScreen(
            renderService.Graphics.GraphicsDevice.Viewport.Width,
            renderService.Graphics.GraphicsDevice.Viewport.Height
        );
        _builderPanel.OnStartGame += OnStartGame;
        _uiService.AddWidget(_builderPanel);
    }

    private void OnStartGame()
    {
        var sceneManager = GameServicesManager.Instance.GetRequired<SceneManager>();
        sceneManager.SetCurrentScene(SceneNames.Play);
    }
}

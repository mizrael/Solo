using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Solo.Services;
using Solocaster.Scenes;
using Solocaster.UI;

namespace Solocaster;

public class SolocasterGame : Game
{
    private GraphicsDeviceManager _graphics;
    private SceneManager _sceneManager;
    private RenderService _renderService;
    private UIService _uiService;

    private const int ScreenWidth = 1600;
    private const int ScreenHeight = 1200;

    public SolocasterGame()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        _graphics.PreferredBackBufferWidth = ScreenWidth;
        _graphics.PreferredBackBufferHeight = ScreenHeight;
        _graphics.ApplyChanges();

        _renderService = new RenderService(_graphics, Window);
        GameServicesManager.Instance.AddService(_renderService);

        _sceneManager = new SceneManager();
        GameServicesManager.Instance.AddService(_sceneManager);

        _uiService = new UIService(_graphics);
        GameServicesManager.Instance.AddService(_uiService);

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _sceneManager.AddScene(SceneNames.CharacterBuilder, new CharacterBuilderScene(this));
        _sceneManager.AddScene(SceneNames.Play, new PlayScene(this));
        _sceneManager.SetCurrentScene(SceneNames.CharacterBuilder);
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        GameServicesManager.Instance.Step(gameTime);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        _renderService.Render();
        _uiService.Render();

        base.Draw(gameTime);
    }
}

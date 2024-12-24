using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

using Solo.Services;
using Solo.Services.Messaging;

namespace SpaceInvaders;

public class SpaceInvadersGame : Game
{
    private GraphicsDeviceManager _graphics;
    private SceneManager _sceneManager;
    private RenderService _renderService;

    public SpaceInvadersGame()
    {
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        Window.AllowUserResizing = true;
        _graphics = new GraphicsDeviceManager(this);
    }

    protected override void Initialize()
    {
        _graphics.IsFullScreen = false;
        _graphics.PreferredBackBufferWidth = 1024;
        _graphics.PreferredBackBufferHeight = 768;
        _graphics.ApplyChanges();

        _renderService = new RenderService(_graphics, Window);
        GameServicesManager.Instance.AddService(_renderService);

        _sceneManager = new SceneManager();
        GameServicesManager.Instance.AddService(_sceneManager);

        GameServicesManager.Instance.AddService(new MessageBus());

        GameServicesManager.Instance.AddService(new CollisionService(new Point(64, 64)));

        GameServicesManager.Instance.Initialize();

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _sceneManager.AddScene(Scenes.SceneNames.Play, new Scenes.PlayScene(this));

        _sceneManager.SetCurrentScene(Scenes.SceneNames.Play);
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

        base.Draw(gameTime);
    }
}

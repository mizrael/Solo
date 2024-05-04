using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using Solo.Services;

namespace Snake;

public class SnakeGame : Game
{
    private GraphicsDeviceManager _graphics;
    private SceneManager _sceneManager;
    private RenderService _renderService;

    public SnakeGame()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        _graphics.IsFullScreen = false;
        _graphics.PreferredBackBufferWidth = 1024;
        _graphics.PreferredBackBufferHeight = 768;
        _graphics.ApplyChanges();

        _renderService = new RenderService(_graphics);
        _renderService.SetLayerConfig((int)RenderLayers.Background, new RenderLayerConfig
        {
            SamplerState = SamplerState.LinearWrap
        });
        GameServicesManager.Instance.AddService(_renderService);

        _sceneManager = new SceneManager();
        GameServicesManager.Instance.AddService(_sceneManager);

        GameServicesManager.Instance.Initialize();

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        // TODO: use this.Content to load your game content here
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        // TODO: Add your update logic here

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        // TODO: Add your drawing code here

        base.Draw(gameTime);
    }
}

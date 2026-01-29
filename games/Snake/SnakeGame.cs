using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

using Solo.Services;

namespace Snake;

public class SnakeGame : Game
{
    private GraphicsDeviceManager _graphics;

    public SnakeGame()
    {
        GraphicsDeviceManagerAccessor.Instance.Initialize(this);
        _graphics = GraphicsDeviceManagerAccessor.Instance.GraphicsDeviceManager;
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        Window.AllowUserResizing = true;
    }

    protected override void Initialize()
    {
        _graphics.IsFullScreen = false;
        _graphics.PreferredBackBufferWidth = 1024;
        _graphics.PreferredBackBufferHeight = 768;
        _graphics.ApplyChanges();

        base.Initialize();
    }

    protected override void LoadContent()
    {
        SceneManager.Instance.AddScene(Scenes.SceneNames.PreGame, () => new Scenes.PreGameScene(this, "Snake!"));
        SceneManager.Instance.AddScene<Scenes.PlayScene>(Scenes.SceneNames.Play, this);
        SceneManager.Instance.AddScene(Scenes.SceneNames.GameOver, () => new Scenes.PreGameScene(this, "Game Over!"));

        SceneManager.Instance.SetScene(Scenes.SceneNames.PreGame);
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        SceneManager.Instance.Step(gameTime);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        SceneManager.Instance.Render();

        base.Draw(gameTime);
    }
}

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Solo.Services;

namespace Pacman;

public class PacmanGame : Game
{
    public PacmanGame()
    {
        GraphicsDeviceManagerAccessor.Instance.Initialize(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        Window.AllowUserResizing = true;
    }

    protected override void Initialize()
    {
        var graphics = GraphicsDeviceManagerAccessor.Instance.GraphicsDeviceManager;
        graphics.IsFullScreen = false;
        graphics.PreferredBackBufferWidth = 1024;
        graphics.PreferredBackBufferHeight = 768;
        graphics.ApplyChanges();

        base.Initialize();
    }

    protected override void LoadContent()
    {
        SceneManager.Instance.AddScene<Scenes.IntroScene>(Scenes.SceneNames.Intro, this);
        SceneManager.Instance.AddScene<Scenes.PlayScene>(Scenes.SceneNames.Play, this);
        SceneManager.Instance.SetScene(Scenes.SceneNames.Intro);
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

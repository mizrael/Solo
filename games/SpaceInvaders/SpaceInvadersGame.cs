using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

using Solo.Services;

namespace SpaceInvaders;

public class SpaceInvadersGame : Game
{
    private GraphicsDeviceManager _graphics;

    public SpaceInvadersGame()
    {
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        Window.AllowUserResizing = true;
        GraphicsDeviceManagerAccessor.Instance.Initialize(this);
        _graphics = GraphicsDeviceManagerAccessor.Instance.GraphicsDeviceManager;
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
        SceneManager.Instance.AddScene< Scenes.MainTitleScene>(Scenes.SceneNames.MainTitle, this);
        SceneManager.Instance.AddScene<Scenes.PlayScene>(Scenes.SceneNames.Play, this);

        SceneManager.Instance.SetScene(Scenes.SceneNames.MainTitle);
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

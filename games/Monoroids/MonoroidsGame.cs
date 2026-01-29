using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Solo.Services;

namespace Monoroids;

public class MonoroidsGame : Game
{
    public MonoroidsGame()
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
        SceneManager.Instance.AddScene(Scenes.SceneNames.Welcome, new Scenes.PreGameScene(this, "Monoroids!"));
        SceneManager.Instance.AddScene(Scenes.SceneNames.ShipSelection, new Scenes.ShipSelectionScene(this));
        SceneManager.Instance.AddScene(Scenes.SceneNames.Play, new Scenes.PlayScene(this));
        SceneManager.Instance.AddScene(Scenes.SceneNames.GameOver, new Scenes.PreGameScene(this, "Game Over!"));

        SceneManager.Instance.SetScene(Scenes.SceneNames.Welcome);
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

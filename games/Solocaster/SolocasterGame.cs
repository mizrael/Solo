using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Solo.Services;
using Solocaster.Scenes;

namespace Solocaster;

public class SolocasterGame : Game
{   
    private const int ScreenWidth = 1600;
    private const int ScreenHeight = 1200;

    public SolocasterGame()
    {
        GraphicsDeviceManagerAccessor.Instance.Initialize(this);

        this.Window.ClientSizeChanged += (sender, args) =>
        {
            UpdateGraphicsSettings(this.Window.ClientBounds.Width, this.Window.ClientBounds.Height);
        };

        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    private void UpdateGraphicsSettings(int width, int height)
    {
        if (GraphicsDeviceManagerAccessor.Instance.GraphicsDeviceManager is null)
            throw new System.ApplicationException("GraphicsDeviceManager is not initialized.");

        GraphicsDeviceManagerAccessor.Instance.GraphicsDeviceManager.PreferredBackBufferWidth = width;
        GraphicsDeviceManagerAccessor.Instance.GraphicsDeviceManager.PreferredBackBufferHeight = height;
        GraphicsDeviceManagerAccessor.Instance.GraphicsDeviceManager.ApplyChanges();
    }

    protected override void Initialize()
    {
        UpdateGraphicsSettings(ScreenWidth, ScreenHeight);

        base.Initialize();
    }

    protected override void LoadContent()
    {
        SceneManager.Instance.AddScene(SceneNames.CharacterBuilder, new CharacterBuilderScene(this));
        SceneManager.Instance.AddScene(SceneNames.Play, new PlayScene(this));
        SceneManager.Instance.SetScene(SceneNames.CharacterBuilder);
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

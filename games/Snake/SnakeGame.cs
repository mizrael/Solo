﻿using Microsoft.Xna.Framework;
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
        Window.AllowUserResizing = true;
    }

    protected override void Initialize()
    {
        _graphics.IsFullScreen = false;
        _graphics.PreferredBackBufferWidth = 1024;
        _graphics.PreferredBackBufferHeight = 768;
        _graphics.ApplyChanges();

        _renderService = new RenderService(_graphics, Window);
        _renderService.SetLayerConfig((int)RenderLayers.Background, new RenderLayerConfig
        {
            SamplerState = SamplerState.LinearWrap
        });
        GameServicesManager.Instance.AddService(_renderService);

        _sceneManager = new SceneManager();
        GameServicesManager.Instance.AddService(_sceneManager);

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _sceneManager.AddScene(Scenes.SceneNames.PreGame, new Scenes.PreGameScene(this));
        _sceneManager.AddScene(Scenes.SceneNames.Play, new Scenes.PlayScene(this));
        _sceneManager.AddScene(Scenes.SceneNames.GameOver, new Scenes.PreGameScene(this, "Game Over!"));

        _sceneManager.SetCurrentScene(Scenes.SceneNames.PreGame);
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

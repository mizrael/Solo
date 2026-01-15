using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Solo;
using Solo.Assets.Loaders;
using Solo.Components;
using Solo.Services;
using System;

namespace SpaceInvaders.Scenes;

public class MainTitleScene : Scene
{
    private KeyboardState _prevKeyState = new();

    public MainTitleScene(Game game) : base(game)
    {
    }

    protected override void EnterCore()
    {
        var spriteSheet = SpriteSheetLoader.Load("meta/spritesheet.json", Game);

        var mainTitle = new GameObject();
        
        var renderer = mainTitle.Components.Add<SpriteRenderComponent>();
        renderer.Sprite = spriteSheet.Get("main_title");

        var transform = mainTitle.Components.Add<TransformComponent>();
        this.Root.AddChild(mainTitle);

        var renderService = GameServicesManager.Instance.GetRequired<RenderService>();
        var refreshMainTitleTransform = new Action(() =>
        {
            transform.Local.Position = new Vector2(
                Game.GraphicsDevice.Viewport.Width * .5f,
                Game.GraphicsDevice.Viewport.Height * .5f
            );
            transform.Local.Scale = new Vector2(2f, 2f);
        });
        refreshMainTitleTransform();
        renderService.Window.ClientSizeChanged += (s, e) => refreshMainTitleTransform();
    }

    protected override void Update(GameTime gameTime)
    {
        var keyboardState = Keyboard.GetState();
        var canStart = _prevKeyState.IsKeyDown(Keys.Enter) && keyboardState.IsKeyUp(Keys.Enter);
        if (canStart)
            GameServicesManager.Instance.GetRequired<SceneManager>().SetCurrentScene(SceneNames.Play);
        _prevKeyState = keyboardState;
    }
}
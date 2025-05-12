using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Solo;
using Solo.Assets;
using Solo.Components;
using Solo.Services;
using System;

namespace Pacman.Scenes;

public class IntroScene : Scene
{
    private KeyboardState _prevKeyState = new();

    public IntroScene(Game game) : base(game)
    {
    }

    protected override void EnterCore()
    {
        var renderService = GameServicesManager.Instance.GetRequired<RenderService>();

        var sprite = Sprite.FromTexture("pacman_intro", Game.Content);

        var introObj = new GameObject();
        var transform = introObj.Components.Add<TransformComponent>();
        var renderer = introObj.Components.Add<SpriteRenderComponent>();
        renderer.Sprite = sprite;
        renderer.LayerIndex = (int)RenderLayers.UI;

        var resize = new Action(() =>
        {
            transform.Local.Scale.X = (float)renderService.Window.ClientBounds.Width / sprite.Bounds.Width;
            transform.Local.Scale.Y = (float)renderService.Window.ClientBounds.Height / sprite.Bounds.Height;

            transform.Local.Position = new Vector2(
                (float)renderService.Window.ClientBounds.Width * .5f,
                (float)renderService.Window.ClientBounds.Height * .5f);
        });

        renderService.Window.ClientSizeChanged += (s, e) =>
        {
            resize();
        };
        resize();

        this.Root.AddChild(introObj);
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

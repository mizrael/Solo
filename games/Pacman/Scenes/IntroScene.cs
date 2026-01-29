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
        var window = Game.Window;

        var sprite = Sprite.FromTexture("pacman_intro", Game.Content);

        var introObj = new GameObject();
        var transform = introObj.Components.Add<TransformComponent>();
        var renderer = introObj.Components.Add<SpriteRenderComponent>();
        renderer.Sprite = sprite;
        renderer.LayerIndex = (int)RenderLayers.UI;

        var resize = new Action(() =>
        {
            transform.Local.Scale.X = (float)window.ClientBounds.Width / sprite.Bounds.Width;
            transform.Local.Scale.Y = (float)window.ClientBounds.Height / sprite.Bounds.Height;

            transform.Local.Position = new Vector2(
                (float)window.ClientBounds.Width * .5f,
                (float)window.ClientBounds.Height * .5f);
        });

        window.ClientSizeChanged += (s, e) =>
        {
            resize();
        };
        resize();

        this.ObjectsGraph.Root.AddChild(introObj);
    }

    protected override void UpdateCore(GameTime gameTime)
    {
        var keyboardState = Keyboard.GetState();
        var canStart = _prevKeyState.IsKeyDown(Keys.Enter) && keyboardState.IsKeyUp(Keys.Enter);
        if (canStart)
            SceneManager.Instance.SetScene(SceneNames.Play);
        _prevKeyState = keyboardState;
    }
}

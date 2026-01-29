using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Solo;
using Solo.Assets;
using Solo.Components;
using Solo.Services;
using Monoroids.Components;
using System;

namespace Monoroids.Scenes;

internal class PreGameScene : Scene
{
    private readonly string _mainText;

    public PreGameScene(Game game, string mainText) : base(game)
    {
        _mainText = mainText;
    }

    protected override void EnterCore()
    {
        var ui = BuidUI();
        ObjectsGraph.Root.AddChild(ui);

        var background = BuildBackground();
        ObjectsGraph.Root.AddChild(background);
    }

    private GameObject BuidUI()
    {
        var ui = new GameObject();

        var textComponent = ui.Components.Add<PreGameUIComponent>();
        textComponent.LayerIndex = (int)RenderLayers.UI;
        textComponent.Text = _mainText;
        textComponent.Font = Game.Content.Load<SpriteFont>("Fonts/UI");

        return ui;
    }

    private GameObject BuildBackground()
    {
        var graphicsDevice = GraphicsDeviceManagerAccessor.Instance.GraphicsDeviceManager.GraphicsDevice;
        var background = new GameObject();
        background.Components.Add<TransformComponent>();

        var sprite = Sprite.FromTexture("Backgrounds/blue", Game.Content);
        var setBackgroundSize = new Action(() =>
        {
            sprite.Bounds = new Rectangle(0, 0,
                               (int)(graphicsDevice.Viewport.Width * 1.5),
                                (int)(graphicsDevice.Viewport.Height * 1.5));
        });
        setBackgroundSize();

        Game.Window.ClientSizeChanged += (s, e) => setBackgroundSize();

        var renderer = background.Components.Add<SpriteRenderComponent>();
        renderer.Sprite = sprite;
        renderer.LayerIndex = (int)RenderLayers.Background;

        return background;
    }
}

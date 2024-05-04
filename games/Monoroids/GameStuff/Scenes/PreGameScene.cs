using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Solo;
using Solo.Assets;
using Solo.Components;
using Solo.Services;
using Monoroids.GameStuff.Components;
using System;

namespace Monoroids.GameStuff.Scenes;

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
        this.Root.AddChild(ui);

        var background = BuildBackground();
        this.Root.AddChild(background);
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
        var renderService = GameServicesManager.Instance.GetService<RenderService>(); 
        
        var background = new GameObject();
        background.Components.Add<TransformComponent>();

        var sprite = Sprite.FromTexture("Backgrounds/blue", Game.Content);
        var setBackgroundSize = new Action(() =>
        {
            sprite.Bounds = new Rectangle(0, 0,
                               (int)(renderService.Graphics.PreferredBackBufferWidth * 1.5),
                                (int)(renderService.Graphics.PreferredBackBufferHeight * 1.5));
        });
        setBackgroundSize();

        renderService.Graphics.DeviceReset += (s, e) => setBackgroundSize();

        var renderer = background.Components.Add<SpriteRenderComponent>();
        renderer.Sprite = sprite;
        renderer.LayerIndex = (int)RenderLayers.Background;

        return background;
    }
}

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Snake.Components;
using Solo;
using Solo.Services;
using System;

namespace Snake.Scenes;

public class PreGameScene : Scene
{
    private readonly string _text;

    public PreGameScene(Game game, string text = "Snake!") : base(game)
    {
        _text = text;
    }

    protected override void EnterCore()
    {
        var ui = new GameObject();
        var textComponent = ui.Components.Add<PreGameUIComponent>();
        textComponent.LayerIndex = (int)RenderLayers.UI;
        textComponent.Text = _text;
        textComponent.Font = Game.Content.Load<SpriteFont>("Fonts/UI");

        this.Root.AddChild(ui);
    }
}

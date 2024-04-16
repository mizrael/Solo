using Microsoft.Xna.Framework;
using Monoroids.Core.Components;
using Monoroids.Core;
using Monoroids.Core.Services;
using System;
using Monoroids.Core.Assets;
using Monoroids.Core.Assets.Loaders;
using System.Collections.Generic;
using System.Linq;

namespace Monoroids.GameStuff.Scenes;

public class ShipSelectionScene : Scene
{
    record ShipMeta(string Asset);

    private static readonly ShipMeta[] _shipsMeta =
    [
        new ShipMeta("playerShip2_red"),
        //new Ship("playerShip2_green"),
        //new Ship("playerShip2_blue"),
    ];

    private GameObject[] _ships;
    private int _selectedShipIndex = 0;

    public ShipSelectionScene(Game game) : base(game)
    {
    }

    protected override void EnterCore()
    {
        var background = BuildBackground();
        this.Root.AddChild(background);

        var spriteSheet = new SpriteSheetLoader().Load("meta/sheet.json", Game);

        var shipsContainer = new GameObject();
        this.Root.AddChild(shipsContainer);

        _ships = BuildShips(spriteSheet);
        foreach (var ship in _ships)
            shipsContainer.AddChild(ship);

        SelectShip(0);
    }

    private void SelectShip(int index)
    {
        _ships[_selectedShipIndex].Enabled = false;
        _selectedShipIndex = index;
        _ships[_selectedShipIndex].Enabled = true;
    }

    private GameObject[] BuildShips(SpriteSheet spriteSheet)
    {
        var renderService = GameServicesManager.Instance.GetService<RenderService>();

        var halfSize = new Vector2(renderService.Graphics.PreferredBackBufferWidth / 2,
                                       renderService.Graphics.PreferredBackBufferHeight / 2);
        var radius = 25f;
        var speed = 0.005f;

        return _shipsMeta.Select(s =>
        {
            var shipObj = new GameObject();

            var playerTransform = shipObj.Components.Add<TransformComponent>();
            
            var renderer = shipObj.Components.Add<SpriteRenderComponent>();            
            renderer.Sprite = spriteSheet.Get(s.Asset);
            renderer.LayerIndex = (int)RenderLayers.Player;            

            var brain = shipObj.Components.Add<LambdaComponent>();
            brain.OnUpdate = (owner, gameTime) =>
            {
                var dt = (float)gameTime.TotalGameTime.TotalMilliseconds * speed;
                playerTransform.Local.Position = halfSize + new Vector2(MathF.Sin(dt), MathF.Cos(dt)) * radius;
            };

            shipObj.Enabled = false;

            return shipObj;
        }).ToArray();
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
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Monoroids.Core;
using Monoroids.Core.Assets;
using Monoroids.Core.Assets.Loaders;
using Monoroids.Core.Components;
using Monoroids.Core.Services;
using Monoroids.GameStuff.Components;
using System;
using System.Linq;

namespace Monoroids.GameStuff.Scenes;

public record struct ShipTemplate(string Name, string Asset, PlayerStats Stats);

public class ShipSelectionScene : Scene
{
    private static readonly ShipTemplate[] _shipTemplates =
    [
        new ShipTemplate("Red", "playerShip2_red", PlayerStats.Default()),
        new ShipTemplate("Green", "playerShip2_green", PlayerStats.Default()),
        new ShipTemplate("Blue", "playerShip2_blue", PlayerStats.Default()),
    ];

    private GameObject _ui;
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
                     
        BuildShips(spriteSheet);
        
        BuildUI();

        SelectShip(0);
    }

    private void BuildUI()
    {
        _ui = new GameObject();

        var textComponent = _ui.Components.Add<ShipSelectionUIComponent>();
        textComponent.LayerIndex = (int)RenderLayers.UI;        
        textComponent.Font = Game.Content.Load<SpriteFont>("Fonts/UI");

        KeyboardState prevKeyState = new();
        var brain = _ui.Components.Add<LambdaComponent>();
        brain.OnUpdate = (owner, gameTime) =>
        {
            var keyboardState = Keyboard.GetState();

            if(prevKeyState.IsKeyDown(Keys.Left) && keyboardState.IsKeyUp(Keys.Left))
            {
                SelectShip(_selectedShipIndex - 1);
            }
            else if (prevKeyState.IsKeyDown(Keys.Right) && keyboardState.IsKeyUp(Keys.Right))
            {
                SelectShip(_selectedShipIndex + 1);
            }
            else if (prevKeyState.IsKeyDown(Keys.Enter) && keyboardState.IsKeyUp(Keys.Enter))
            {
                GameServicesManager.Instance.GetService<SceneManager>().SetCurrentScene(SceneNames.Play);
            }

            prevKeyState = keyboardState;
        };

        this.Root.AddChild(_ui);
    }

    private void SelectShip(int index)
    {
        index = (index < 0) ? _ships.Length - 1 : index % _ships.Length;

        _ships[_selectedShipIndex].Enabled = false;
        _selectedShipIndex = index;
        _ships[_selectedShipIndex].Enabled = true;

        _ui.Components.Get<ShipSelectionUIComponent>().SelectedShip = _shipTemplates[_selectedShipIndex];
    }

    private void BuildShips(SpriteSheet spriteSheet)
    {
        var shipsContainer = new GameObject();

        var renderService = GameServicesManager.Instance.GetService<RenderService>();

        var halfSize = new Vector2(renderService.Graphics.PreferredBackBufferWidth / 2,
                                    renderService.Graphics.PreferredBackBufferHeight / 2);
        var radius = 25f;
        var speed = 0.005f;

        renderService.Graphics.DeviceReset += (s, e) =>
        {
            halfSize = new Vector2(renderService.Graphics.PreferredBackBufferWidth / 2,
                                    renderService.Graphics.PreferredBackBufferHeight / 2);
        };

        _ships = _shipTemplates.Select(s =>
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

            shipsContainer.AddChild(shipObj);

            return shipObj;
        }).ToArray();

        this.Root.AddChild(shipsContainer);
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

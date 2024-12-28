using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Pacman.Components;
using Solo;
using Solo.Assets;
using Solo.Assets.Loaders;
using Solo.Components;
using Solo.Services;
using System;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Pacman.Scenes;

public class PlayScene : Scene
{
    public PlayScene(Game game) : base(game)
    {
    }

    protected override void EnterCore()
    {
        var spriteSheet = new SpriteSheetLoader().Load("meta/spritesheet.json", Game);

        var map = AddMap();

        AddPlayer(spriteSheet, map);
    }

    private void AddPlayer(SpriteSheet spriteSheet, GameObject map)
    {
        var player = new GameObject();
        var transform = player.Components.Add<TransformComponent>();

        var framesCount = 3;
        var fps = 6;
        var frames = Enumerable.Range(1, framesCount)
            .Select(i =>
            {
                var spriteName = $"pacman{i}";
                var sprite = spriteSheet.Get(spriteName);
                return new AnimatedSpriteSheet.Frame(sprite.Bounds);
            })
            .ToArray();

        var spriteSheetTexture = Game.Content.Load<Texture2D>(spriteSheet.ImagePath);
        var animation = new AnimatedSpriteSheet("pacman", spriteSheetTexture, fps, frames);

        var renderer = player.Components.Add<AnimatedSpriteSheetRenderer>();
        renderer.Animation = animation;
        renderer.LayerIndex = (int)RenderLayers.Player;

        var playerBrain = player.Components.Add<PlayerBrainComponent>();
        playerBrain.Map = map;

        this.Root.AddChild(player);
    }

    private GameObject AddMap()
    {
        var map = new GameObject();
        
        map.Components.Add<MapLogicComponent>();
        var transform = map.Components.Add<TransformComponent>();

        var sprite = Sprite.FromTexture("map", Game.Content);
        var renderer = map.Components.Add<SpriteRenderComponent>();
        renderer.Sprite = sprite;
        renderer.LayerIndex = (int)RenderLayers.Background;

        var renderService = GameServicesManager.Instance.GetService<RenderService>();
        var calculateSize = new Action(() =>
        {
            transform.Local.Position.X = renderService.Graphics.GraphicsDevice.Viewport.Width / 2;
            transform.Local.Position.Y = renderService.Graphics.GraphicsDevice.Viewport.Height / 2;

            transform.Local.Scale.X = renderService.Graphics.GraphicsDevice.Viewport.Width / (float)sprite.Bounds.Width;
            transform.Local.Scale.Y = renderService.Graphics.GraphicsDevice.Viewport.Height / (float)sprite.Bounds.Height;
        });
        calculateSize();

        renderService.Window.ClientSizeChanged += (s, e) => calculateSize();

        this.Root.AddChild(map);

        return map;
    }
}

public class PlayerBrainComponent : Component
{
    private TransformComponent _transform;

    public PlayerBrainComponent(GameObject owner) : base(owner)
    {
    }

    protected override void InitCore()
    {
        var mapLogic = this.Map.Components.Get<MapLogicComponent>();

        _transform = Owner.Components.Get<TransformComponent>();
        _transform.Local.Position = mapLogic.GetPlayerStartTile();

        var renderer = this.Owner.Components.Get<AnimatedSpriteSheetRenderer>();

        var renderService = GameServicesManager.Instance.GetService<RenderService>();
        var calculateSize = new Action(() =>
        {
            if (renderer.CurrentFrame is null)
                return;

            _transform.Local.Scale.X = mapLogic.TileSize.X / renderer.CurrentFrame.Bounds.Width;
            _transform.Local.Scale.Y = mapLogic.TileSize.Y / renderer.CurrentFrame.Bounds.Height;
        });
        calculateSize();

        renderService.Window.ClientSizeChanged += (s, e) => calculateSize();

        base.InitCore();
    }

    protected override void UpdateCore(GameTime gameTime)
    {
        var keyboard = Keyboard.GetState();

        var velocity = Vector2.Zero;
        if (keyboard.IsKeyDown(Keys.W))
        {
            velocity.Y = -1;
        }
        if (keyboard.IsKeyDown(Keys.S))
        {
            velocity.Y = 1;
        }
        if (keyboard.IsKeyDown(Keys.A))
        {
            velocity.X = -1;
        }
        if (keyboard.IsKeyDown(Keys.D))
        {
            velocity.X = 1;
        }
        
        _transform.Local.Position += velocity * Speed * (float)gameTime.ElapsedGameTime.TotalSeconds;
    }

    public float Speed = 100f;

    public GameObject Map;
}

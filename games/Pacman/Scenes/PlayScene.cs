using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
        var fps = 10;
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

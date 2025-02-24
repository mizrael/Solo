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

namespace Pacman.Scenes;

public class PlayScene : Scene
{
    public PlayScene(Game game) : base(game)
    {
    }

    protected override void EnterCore()
    {
        var gameState = new GameState();

        var spriteSheet = new SpriteSheetLoader().Load("meta/spritesheet.json", Game);
        var collisionService = GameServicesManager.Instance.GetRequired<CollisionService>();
        var map = AddMap(spriteSheet, collisionService, gameState);

        AddPellets(spriteSheet, collisionService, gameState, map);

        AddPlayer(spriteSheet, map, collisionService);
        
        AddUI(gameState);
    }

    private GameObject AddUI(GameState gameState)
    {
        var uiObj = new GameObject();
        var uiComponent = uiObj.Components.Add<GameUIComponent>();
        uiComponent.GameState = gameState;
        uiComponent.LayerIndex = (int)RenderLayers.UI;
        uiComponent.Font = Game.Content.Load<SpriteFont>("GameFont");
        this.Root.AddChild(uiObj);

        return uiObj;
    }

    private void AddPlayer(SpriteSheet spriteSheet, GameObject map, CollisionService collisionService)
    {
        var mapBrain = map.Components.Get<MapLogicComponent>();
        mapBrain.OnInitialized += () =>
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

            var playerBBox = player.Components.Add<BoundingBoxComponent>();
            collisionService.Add(playerBBox);

            this.Root.AddChild(player);
        };
    }

    private GameObject AddMap(SpriteSheet spriteSheet, CollisionService collisionService, GameState gameState)
    {
        var map = new GameObject();
        
        var mapBrain = map.Components.Add<MapLogicComponent>();
        var transform = map.Components.Add<TransformComponent>();
       
        var sprite = Sprite.FromTexture("map", Game.Content);
        var renderer = map.Components.Add<SpriteRenderComponent>();
        renderer.Sprite = sprite;
        renderer.LayerIndex = (int)RenderLayers.Background;

        this.Root.AddChild(map);

        return map;
    }

    private void AddPellets(SpriteSheet spriteSheet, CollisionService collisionService, GameState gameState, GameObject map)
    {
        var container = new GameObject();

        var mapBrain = map.Components.Get<MapLogicComponent>();
        var mapTransform = map.Components.Get<TransformComponent>();

        var renderService = GameServicesManager.Instance.GetRequired<RenderService>();

        var pelletSprite = spriteSheet.Get("pellet");
        var pelletPositions = mapBrain.GetTilesByType(TileTypes.Pellet);

        mapBrain.OnInitialized += () =>
        {
            foreach (var pos in pelletPositions)
            {
                AddPellet(container, mapBrain, mapTransform, pelletSprite, pos, collisionService, renderService, gameState);
            }
        };

        this.Root.AddChild(container);
    }

    private static void AddPellet(
        GameObject parent, 
        MapLogicComponent mapBrain, 
        TransformComponent mapTransform, 
        Sprite pelletSprite, 
        (int row, int col) tileCoords,
        CollisionService collisionService,
        RenderService renderService,
        GameState gameState)
    {
        var pellet = new GameObject();

        var pelletTransform = pellet.Components.Add<TransformComponent>();

        var pelletBbox = pellet.Components.Add<BoundingBoxComponent>();
        collisionService.Add(pelletBbox);

        pelletBbox.OnCollision += (collidedWith) =>
        {
            var hasPlayerBrain = collidedWith.Owner.Components.Has<PlayerBrainComponent>();
            if (hasPlayerBrain)
            {
                gameState.IncreaseScore(50);
                pellet.Enabled = false;
                pellet.Parent?.RemoveChild(pellet);
            }
        };

        var pelletRenderer = pellet.Components.Add<SpriteRenderComponent>();
        pelletRenderer.Sprite = pelletSprite;
        pelletRenderer.LayerIndex = (int)RenderLayers.Items;

        var scaleFactor = .35f;
        var bboxScaleFactor = 1.5f;

        var resize = new Action(() =>
        {
            pelletTransform.Local.Position = mapBrain.GetTileCenter(tileCoords.row, tileCoords.col);

            pelletTransform.Local.Scale.X = mapBrain.TileSize.X / (float)pelletSprite.Bounds.Width * scaleFactor;
            pelletTransform.Local.Scale.Y = mapBrain.TileSize.Y / (float)pelletSprite.Bounds.Height * scaleFactor;

            var bboxSize = new Point(
                (int)((float)pelletSprite.Bounds.Size.X * pelletTransform.Local.Scale.X * bboxScaleFactor),
                (int)((float)pelletSprite.Bounds.Size.Y * pelletTransform.Local.Scale.Y * bboxScaleFactor));
            pelletBbox.SetSize(bboxSize);
        });

        renderService.Window.ClientSizeChanged += (s, e) =>
        {
            resize();
        };
        resize();

        parent.AddChild(pellet);
    }
}

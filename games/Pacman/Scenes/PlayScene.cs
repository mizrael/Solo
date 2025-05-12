using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pacman.Components;
using Solo;
using Solo.Assets;
using Solo.Assets.Loaders;
using Solo.Components;
using Solo.Services;
using System;

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

        var mapBrain = map.Components.Get<MapLogicComponent>();
        mapBrain.OnInitialized += () =>
        {
            AddPellets(spriteSheet, collisionService, gameState, map);

            var player = AddPlayer(spriteSheet, collisionService, map, gameState);

            AddGhost(spriteSheet, collisionService, map, Ghosts.Blinky, player);
       //     AddGhost(spriteSheet, collisionService, map, Ghosts.Pinky, player);
            AddGhost(spriteSheet, collisionService, map, Ghosts.Inky, player);
            //AddGhost(spriteSheet, collisionService, map, Ghosts.Clyde, player);
        };

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

    private GameObject AddPlayer(SpriteSheet spriteSheet, CollisionService collisionService, GameObject map, GameState gameState)
    {
        var player = new GameObject();
        var transform = player.Components.Add<TransformComponent>();

        var animLoader = new AnimatedSpriteSheetLoader();
        var walkAnim = animLoader.Load("meta/animations/pacman_walk.json", Game);
        var deathAnim = animLoader.Load("meta/animations/pacman_die.json", Game);

        var playerRenderer = player.Components.Add<AnimatedSpriteSheetRenderer>();
        playerRenderer.Animation = walkAnim;
        playerRenderer.LayerIndex = (int)RenderLayers.Player;

        var playerBrain = player.Components.Add<PlayerBrainComponent>();
        playerBrain.Map = map;

        var playerBBox = player.Components.Add<BoundingBoxComponent>();
        collisionService.Add(playerBBox);
        playerBBox.OnCollision += (collidedWith) =>
        {
            if (!playerBrain.Enabled)
                return;

            var collidedWithGhost = collidedWith.Owner.Components.Has<GhostBrainComponent>();
            if (!collidedWithGhost)
                return;

            playerRenderer.Animation = deathAnim;
            playerBrain.Enabled = false;

            var timer = new System.Timers.Timer(TimeSpan.FromSeconds(5));
            timer.Elapsed += (s, e) =>
            {
                timer.Stop();
                timer.Dispose();

                GameServicesManager.Instance.GetRequired<SceneManager>().SetCurrentScene(SceneNames.Intro);
            };
            timer.Start();
        };

        this.Root.AddChild(player);

        return player;
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

        foreach (var pos in pelletPositions)
        {
            AddPellet(container, mapBrain, mapTransform, pelletSprite, pos, collisionService, renderService, gameState);
        }

        this.Root.AddChild(container);
    }

    private static void AddPellet(
        GameObject parent, 
        MapLogicComponent mapBrain, 
        TransformComponent mapTransform, 
        Sprite pelletSprite, 
        TileInfo tile,
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
                gameState.IncreaseScore(10);
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
            pelletTransform.Local.Position = mapBrain.GetTileCenter(tile);

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


    private void AddGhost(SpriteSheet spriteSheet, CollisionService collisionService, GameObject map, Ghosts ghostType, GameObject player)
    {
        var ghost = new GameObject();
        var transform = ghost.Components.Add<TransformComponent>();

        var ghostName = ghostType.ToString().ToLower();

        var animLoader = new AnimatedSpriteSheetLoader();
        var animation = animLoader.Load($"meta/animations/{ghostName}_walk.json", Game);

        var renderer = ghost.Components.Add<AnimatedSpriteSheetRenderer>();
        renderer.Animation = animation;
        renderer.LayerIndex = (int)RenderLayers.Enemies;

        var bbox = ghost.Components.Add<BoundingBoxComponent>();
        collisionService.Add(bbox);
        
        var brain = ghost.Components.Add<GhostBrainComponent>();
        brain.Map = map;
        brain.Player = player;
        brain.GhostType = ghostType;

        brain.Logic = ghostType switch
        {
            Ghosts.Blinky => AI.StateMachines.Blinky(ghost, player, map, 2000f),
            //Ghosts.Pinky => new PinkyBrain(ghost),
            Ghosts.Inky => AI.StateMachines.Inky(ghost, player, map, 2000f),
            //Ghosts.Clyde => new ClydeBrain(ghost),
            _ => throw new NotImplementedException()
        };

        this.Root.AddChild(ghost);
    }
}

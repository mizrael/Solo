using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pacman.Components;
using Solo;
using Solo.Assets;
using Solo.Assets.Loaders;
using Solo.Components;
using Solo.Services;
using Solo.Services.Messaging;
using SpaceInvaders.Logic.Messages;
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

        var bus = GameServicesManager.Instance.GetRequired<MessageBus>();
        var magicPillEatenTopic = bus.GetTopic<MagicPillEaten>();

        var map = AddMap(spriteSheet, collisionService, gameState);
        var mapBrain = map.Components.Get<MapLogicComponent>();
        mapBrain.OnInitialized += () =>
        {
            AddPellets(spriteSheet, collisionService, gameState, map, magicPillEatenTopic);

            var player = AddPlayer(spriteSheet, collisionService, map, gameState);

            AddGhost(GhostTypes.Blinky, spriteSheet, collisionService, map, player, magicPillEatenTopic);
            //   AddGhost(GhostTypes.Pinky, spriteSheet, collisionService, map, player, magicPillEatenTopic);
            AddGhost(GhostTypes.Inky, spriteSheet, collisionService, map, player, magicPillEatenTopic);
        //    AddGhost(GhostTypes.Clyde, spriteSheet, collisionService, map, player, magicPillEatenTopic);
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
        var walkAnim = AnimatedSpriteSheetLoader.Load("meta/animations/pacman_walk.json", Game);
        var deathAnim = AnimatedSpriteSheetLoader.Load("meta/animations/pacman_die.json", Game);

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

            var collidedWithGhost = collidedWith.Owner.Components.TryGet<GhostBrainComponent>(out var ghostBrain);
            if (!collidedWithGhost || ghostBrain!.State == GhostStates.Idle)
                return;

            if (ghostBrain.State == GhostStates.Scared)
            {
                ghostBrain.WasEaten();
                gameState.IncreaseScore(200u);
            }
            else
            {
                playerRenderer.Animation = deathAnim;
                playerBrain.Enabled = false;

                var timer = new System.Timers.Timer(deathAnim.Duration);
                timer.Elapsed += (s, e) =>
                {
                    timer.Stop();
                    timer.Dispose();

                    GameServicesManager.Instance.GetRequired<SceneManager>().SetCurrentScene(SceneNames.Intro);
                };
                timer.Start();
            }
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

    private void AddPellets(SpriteSheet spriteSheet, CollisionService collisionService, GameState gameState, GameObject map, MessageTopic<MagicPillEaten> magicPillEatenTopic)
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

        pelletPositions = mapBrain.GetTilesByType(TileTypes.MagicPill);
        foreach (var pos in pelletPositions)
        {
            AddPellet(container, mapBrain, mapTransform, pelletSprite, pos, collisionService, renderService, gameState, true, magicPillEatenTopic);
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
        GameState gameState,
        bool isMagicPill = false,
        MessageTopic<MagicPillEaten>? magicPillEatenTopic = null)
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
                var points = isMagicPill ? 50u : 10u;
                gameState.IncreaseScore(points);
                pellet.Enabled = false;
                pellet.Parent?.RemoveChild(pellet);

                magicPillEatenTopic?.Publish(new MagicPillEaten());
            }
        };

        var pelletRenderer = pellet.Components.Add<SpriteRenderComponent>();
        pelletRenderer.Sprite = pelletSprite;
        pelletRenderer.LayerIndex = (int)RenderLayers.Items;

        var scaleFactor = isMagicPill ? .75f : .35f;
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

    private void AddGhost(
        GhostTypes ghostType,
        SpriteSheet spriteSheet,
        CollisionService collisionService,
        GameObject map,
        GameObject player,
        MessageTopic<MagicPillEaten> magicPillEatenTopic)
    {
        var ghostName = ghostType.ToString().ToLower();

        var ghost = new GameObject();

        ghost.AddTag(ghostName);

        var transform = ghost.Components.Add<TransformComponent>();

        var ghostWalkAnim = AnimatedSpriteSheetLoader.Load($"meta/animations/{ghostName}_walk.json", Game);

        var renderer = ghost.Components.Add<AnimatedSpriteSheetRenderer>();
        renderer.Animation = ghostWalkAnim;
        renderer.LayerIndex = (int)RenderLayers.Enemies;
       
        var bbox = ghost.Components.Add<BoundingBoxComponent>();
        collisionService.Add(bbox);
        
        var brain = ghost.Components.Add<GhostBrainComponent>();
        brain.Setup(this, ghostType, map, player);

        magicPillEatenTopic.Subscribe(ghost, (s, e) =>
        {
            brain.State = GhostStates.Scared;
        });

        this.Root.AddChild(ghost);
    }
}

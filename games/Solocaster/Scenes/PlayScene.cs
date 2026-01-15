using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Solo;
using Solo.Assets.Loaders;
using Solo.Components;
using Solo.Services;
using Solocaster.Components;
using Solocaster.Persistence;
using Solocaster.Services;
using System.Linq;

namespace Solocaster.Scenes;

public class PlayScene : Scene
{
    private const int FrameBufferScale = 2;

    public PlayScene(Game game) : base(game)
    {
    }

    protected override void EnterCore()
    {
        var renderService = GameServicesManager.Instance.GetRequired<RenderService>();
        var entityManager = GameServicesManager.Instance.GetRequired<EntityManager>();
        entityManager.Clear();

        var frameBufferWidth = renderService.Graphics.GraphicsDevice.Viewport.Height / FrameBufferScale;
        var frameBufferHeight = renderService.Graphics.GraphicsDevice.Viewport.Width / FrameBufferScale;

        var map = LevelLoader.LoadFromJson("./data/levels/level1.json", Game, entityManager);

        var player = new GameObject();
        var playerTransform = player.Components.Add<TransformComponent>();
        playerTransform.Local.Position = map.GetStartingPosition();
        playerTransform.Local.Direction = new Vector2(-1, 0);

        var playerBrain = new PlayerBrain(player, map);
        player.Components.Add(playerBrain);

        this.Root.AddChild(player);

        var levelSpritesheet = SpriteSheetLoader.Load("./data/spritesheets/wolfenstein.json", this.Game);
        var textures = levelSpritesheet.Texture.Split(64, 64)
            .Select(t => t.Rotate90(RotationDirection.CounterClockwise))
            .ToArray();
        var raycaster = new Raycaster(map, frameBufferWidth, frameBufferHeight, textures);

        var frameTexture = new Texture2D(renderService.Graphics.GraphicsDevice, frameBufferWidth, frameBufferHeight);

        var mapEntity = new GameObject();
        var mapRenderer = new MapRenderer(mapEntity, player, map, raycaster, frameTexture);
        mapEntity.Components.Add(mapRenderer);
        mapRenderer.LayerIndex = 0;
        Root.AddChild(mapEntity);

        var miniMapEntity = new GameObject();
        var miniMapRenderer = new MiniMapRenderer(miniMapEntity, map, player);
        miniMapEntity.Components.Add(miniMapRenderer);
        miniMapRenderer.LayerIndex = 1;
        mapEntity.AddChild(miniMapEntity);

        var font = Game.Content.Load<SpriteFont>("Font");

        var debugUIEntity = new GameObject();
        var debugUI = new DebugUIRenderer(debugUIEntity, font, player);
        debugUIEntity.Components.Add(debugUI);
        debugUI.LayerIndex = 2;
        Root.AddChild(debugUIEntity);
    }
}

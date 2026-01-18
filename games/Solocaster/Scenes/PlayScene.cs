using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Solo;
using Solo.Components;
using Solo.Services;
using Solocaster.Components;
using Solocaster.Inventory;
using Solocaster.Persistence;
using Solocaster.UI;

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
        var uiService = GameServicesManager.Instance.GetRequired<UIService>();
        uiService.ClearWidgets();

        var spatialGrid = new SpatialGrid(bucketSize: 1f);

        ItemTemplateLoader.LoadAllFromFolder("./data/templates/items/");

        var frameBufferWidth = renderService.Graphics.GraphicsDevice.Viewport.Height / FrameBufferScale;
        var frameBufferHeight = renderService.Graphics.GraphicsDevice.Viewport.Width / FrameBufferScale;

        var level = LevelLoader.LoadFromJson("./data/levels/level1.json", Game, Root, spatialGrid);

        var player = new GameObject();
        var playerTransform = player.Components.Add<TransformComponent>();
        playerTransform.Local.Position = level.Map.GetStartingPosition();
        playerTransform.Local.Direction = new Vector2(-1, 0);
        var statsComponent = player.Components.Add<StatsComponent>();
        var inventoryComponent = player.Components.Add<InventoryComponent>();

        var playerBrain = new PlayerBrain(player, level.Map);
        player.Components.Add(playerBrain);
        playerBrain.SpatialGrid = spatialGrid;

        this.Root.AddChild(player);

        var raycaster = new Raycaster(level, spatialGrid, frameBufferWidth, frameBufferHeight);

        var frameTexture = new Texture2D(renderService.Graphics.GraphicsDevice, frameBufferWidth, frameBufferHeight);

        var mapEntity = new GameObject();
        var mapRenderer = new MapRenderer(mapEntity, player, level.Map, raycaster, frameTexture);
        mapEntity.Components.Add(mapRenderer);
        mapRenderer.LayerIndex = 0;
        Root.AddChild(mapEntity);

        var miniMapEntity = new GameObject();
        var miniMapRenderer = new MiniMapRenderer(miniMapEntity, level.Map, player);
        miniMapEntity.Components.Add(miniMapRenderer);
        miniMapRenderer.LayerIndex = 1;
        mapEntity.AddChild(miniMapEntity);

        var font = Game.Content.Load<SpriteFont>("Font");

        var debugUIEntity = new GameObject();
        var debugUI = new DebugUIRenderer(debugUIEntity, font, player);
        debugUIEntity.Components.Add(debugUI);
        debugUI.LayerIndex = 2;
        Root.AddChild(debugUIEntity);

        var inventoryPanel = new InventoryPanel(inventoryComponent, statsComponent, font, Game);
        inventoryPanel.Visible = false;
        inventoryPanel.CenterOnScreen(
            renderService.Graphics.GraphicsDevice.Viewport.Width,
            renderService.Graphics.GraphicsDevice.Viewport.Height
        );
        uiService.AddWidget(inventoryPanel);
        playerBrain.InventoryPanel = inventoryPanel;
    }
}

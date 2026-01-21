using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Solo;
using Solo.Components;
using Solo.Services;
using Solocaster.Character;
using Solocaster.Components;
using Solocaster.Inventory;
using Solocaster.Monsters;
using Solocaster.Persistence;
using Solocaster.State;
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

        UITheme.Load("./data/ui/theme.json");
        ItemTemplateLoader.LoadAllFromFolder("./data/templates/items/");
        CharacterTemplateLoader.LoadAll("./data/templates/character/");
        MonsterTemplateLoader.LoadAllFromFolder("./data/templates/monsters/");

        var frameBufferWidth = renderService.Graphics.GraphicsDevice.Viewport.Height / FrameBufferScale;
        var frameBufferHeight = renderService.Graphics.GraphicsDevice.Viewport.Width / FrameBufferScale;

        var levelPath = "./data/levels/level1.json";
        var level = LevelLoader.LoadFromJson(levelPath, Game, Root, spatialGrid);

        var player = new GameObject();
        var playerTransform = player.Components.Add<TransformComponent>();
        playerTransform.Local.Position = level.Map.GetStartingPosition();
        playerTransform.Local.Direction = new Vector2(-1, 0);
        var statsComponent = player.Components.Add<StatsComponent>();
        GameState.EnsureCharacter();
        var character = GameState.CurrentCharacter!;
        statsComponent.SetCharacter(character.RaceId, character.ClassId, character.Sex);
        statsComponent.Name = character.Name;
        var inventoryComponent = player.Components.Add<InventoryComponent>();

        var playerBrain = new PlayerBrain(player, level.Map);
        player.Components.Add(playerBrain);
        playerBrain.SpatialGrid = spatialGrid;

        this.Root.AddChild(player);

        var monsters = LevelLoader.SpawnMonsters(levelPath, level, Game, Root, spatialGrid, player);
        level.Monsters = monsters;

        var raycaster = new Raycaster(level, spatialGrid, frameBufferWidth, frameBufferHeight);
        playerBrain.Raycaster = raycaster;

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
        miniMapEntity.Enabled = false;
        Root.AddChild(miniMapEntity);
        playerBrain.MiniMapEntity = miniMapEntity;

        var font = Game.Content.Load<SpriteFont>("Font");
        uiService.SetTooltipFont(font);

        var debugUIEntity = new GameObject();
        var debugUI = new DebugUIRenderer(debugUIEntity, font, player);
        debugUIEntity.Components.Add(debugUI);
        debugUI.LayerIndex = 2;
        debugUIEntity.Enabled = false;
        Root.AddChild(debugUIEntity);
        playerBrain.DebugUIEntity = debugUIEntity;

        var characterPanel = new CharacterPanel(inventoryComponent, statsComponent, uiService.DragDropManager, font, Game);
        characterPanel.CenterOnScreen(
            renderService.Graphics.GraphicsDevice.Viewport.Width,
            renderService.Graphics.GraphicsDevice.Viewport.Height
        );
        uiService.AddWidget(characterPanel);
        playerBrain.CharacterPanel = characterPanel;

        var beltPanel = new BeltPanel(inventoryComponent, uiService.DragDropManager, font, Game);
        beltPanel.PositionAtBottom(
            renderService.Graphics.GraphicsDevice.Viewport.Width,
            renderService.Graphics.GraphicsDevice.Viewport.Height
        );
        uiService.AddWidget(beltPanel);

        var playerStatusPanel = new PlayerStatusPanel(statsComponent, Game);
        playerStatusPanel.PositionTopRight(renderService.Graphics.GraphicsDevice.Viewport.Width);
        uiService.AddWidget(playerStatusPanel);

        var metricsPanel = new MetricsPanel(statsComponent, font, Game);
        metricsPanel.CenterOnScreen(
            renderService.Graphics.GraphicsDevice.Viewport.Width,
            renderService.Graphics.GraphicsDevice.Viewport.Height
        );
        uiService.AddWidget(metricsPanel);
        playerBrain.MetricsPanel = metricsPanel;
    }
}

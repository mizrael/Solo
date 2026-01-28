using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Solo;
using Solo.Components;
using Solo.Services;
using Solo.Services.Rendering;
using Solocaster.Character;
using Solocaster.Components;
using Solocaster.Inventory;
using Solocaster.Monsters;
using Solocaster.Persistence;
using Solocaster.Services;
using Solocaster.State;
using Solocaster.UI;

namespace Solocaster.Scenes;

public class PlayScene : Scene
{
    private const int FrameBufferScale = 2;

    private InputService _inputService;
    private UIService _uiService;
    private RenderTarget2D _gameplayTarget;
    private Effect _blurEffect;

    public PlayScene(Game game) : base(game)
    {
    }

    protected override void InitializeCore()
    {
        _inputService = new InputService();
        Services.Add(_inputService);

        _uiService = new UIService();
        Services.Add(_uiService);
        _renderService.SetLayerConfig(RenderLayers.UI, new RenderLayerConfig
        {
            SamplerState = SamplerState.PointClamp
        });

        var spatialGrid = new SpatialGrid(bucketSize: 1f);

        UITheme.Load("./data/ui/theme.json");
        ItemTemplateLoader.LoadAllFromFolder("./data/templates/items/");
        CharacterTemplateLoader.LoadAll("./data/templates/character/");
        MonsterTemplateLoader.LoadAllFromFolder("./data/templates/monsters/");

        var frameBufferWidth = this.Game.GraphicsDevice.Viewport.Height / FrameBufferScale;
        var frameBufferHeight = this.Game.GraphicsDevice.Viewport.Width / FrameBufferScale;

        var levelPath = "./data/levels/level1.json";
        var level = LevelLoader.LoadFromJson(levelPath, Game, ObjectsGraph.Root, spatialGrid);

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

        var playerBrain = new PlayerBrain(player, level.Map, _inputService);
        player.Components.Add(playerBrain);

        var playerUIController = new PlayerUIController(player, _inputService);
        player.Components.Add(playerUIController);

        ObjectsGraph.Root.AddChild(player);

        var monsters = LevelLoader.SpawnMonsters(levelPath, level, Game, ObjectsGraph.Root, spatialGrid, player);
        level.Monsters = monsters;

        var raycaster = new Raycaster(level, spatialGrid, frameBufferWidth, frameBufferHeight);
        playerBrain.Raycaster = raycaster;

        var frameTexture = new Texture2D(base.Game.GraphicsDevice, frameBufferWidth, frameBufferHeight);

        var mapEntity = new GameObject();
        var mapRenderer = new MapRenderer(mapEntity, player, level.Map, raycaster, frameTexture);
        mapEntity.Components.Add(mapRenderer);
        mapRenderer.LayerIndex = 0;
        ObjectsGraph.Root.AddChild(mapEntity);

        var miniMapEntity = new GameObject();
        var miniMapRenderer = new MiniMapRenderer(miniMapEntity, level.Map, player);
        miniMapEntity.Components.Add(miniMapRenderer);
        miniMapRenderer.LayerIndex = 1;
        miniMapEntity.Enabled = false;
        ObjectsGraph.Root.AddChild(miniMapEntity);
        playerUIController.MiniMapEntity = miniMapEntity;

        var font = Game.Content.Load<SpriteFont>("Font");
        _uiService.SetTooltipFont(font);

        var debugUIEntity = new GameObject();
        var debugUI = new DebugUIRenderer(debugUIEntity, font, player);
        debugUIEntity.Components.Add(debugUI);
        debugUI.LayerIndex = 2;
        debugUIEntity.Enabled = false;
        ObjectsGraph.Root.AddChild(debugUIEntity);
        playerUIController.DebugUIEntity = debugUIEntity;

        var beltPanel = new BeltPanel(inventoryComponent, _uiService.DragDropManager, font, Game);
        beltPanel.PositionAtBottom(
            base.Game.GraphicsDevice.Viewport.Width,
            base.Game.GraphicsDevice.Viewport.Height
        );
        _uiService.AddWidget(beltPanel);

        var playerStatusPanel = new PlayerStatusPanel(statsComponent, Game);
        playerStatusPanel.PositionTopRight(base.Game.GraphicsDevice.Viewport.Width);
        _uiService.AddWidget(playerStatusPanel);

        var playerHandsEntity = new GameObject();
        var playerHandsRenderer = new PlayerHandsRenderer(playerHandsEntity, Game, inventoryComponent, playerBrain);
        playerHandsEntity.Components.Add(playerHandsRenderer);
        playerHandsRenderer.LayerIndex = 5;
        ObjectsGraph.Root.AddChild(playerHandsEntity);

        // Register overlay scenes with player data
        SceneManager.Instance.AddScene(SceneNames.CharacterPanel,
            new CharacterPanelScene(Game, inventoryComponent, statsComponent));
        SceneManager.Instance.AddScene(SceneNames.MetricsPanel,
            new MetricsPanelScene(Game, statsComponent));

        // Setup blur pipeline for gameplay layers
        _blurEffect = Game.Content.Load<Effect>("Effects/Blur");
        var viewport = Game.GraphicsDevice.Viewport;
        _gameplayTarget = new RenderTarget2D(Game.GraphicsDevice, viewport.Width, viewport.Height);

        _blurEffect.Parameters["TexelSize"]?.SetValue(new Vector2(1f / viewport.Width, 1f / viewport.Height));
        _blurEffect.Parameters["BlurAmount"]?.SetValue(2f);

        var pipeline = new RenderPipeline()
            .Add(new RenderLayersStep { LayerEnd = RenderLayers.UI, Output = _gameplayTarget })
            .Add(new ApplyEffectStep { Effect = _blurEffect, Output = null })
            .Add(new RenderLayersStep { Output = null, ClearTarget = false });

        _renderService.SetPipeline(pipeline);
    }
}

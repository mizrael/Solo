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
    private RenderTarget2D _sceneCapture;
    private RenderTarget2D _postProcess;
    private RenderPipeline _capturePipeline;
    private RenderPipeline _inGamePipeline;

    public PlayScene(Game game) : base(game)
    {
        SceneManager.Instance.OnSceneChanged += OnSceneChanged;
    }

    private void OnSceneChanged(Scene currentScene)
    {
        if (currentScene == this)
            return;

        RenderService.SetPipeline(_capturePipeline);
        RenderService.Render();
        RenderService.SetPipeline(_inGamePipeline);
    }

    protected override void InitializeCore()
    {
        _inputService = new InputService();
        Services.Add(_inputService);

        _uiService = new UIService();
        Services.Add(_uiService);
        RenderService.SetLayerConfig(RenderLayers.UI, new RenderLayerConfig
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

        // Create render targets for post-processing
        var viewport = Game.GraphicsDevice.Viewport;
        _sceneCapture = new RenderTarget2D(Game.GraphicsDevice, viewport.Width, viewport.Height);
        _postProcess = new RenderTarget2D(Game.GraphicsDevice, viewport.Width, viewport.Height);

        // Load tilt-shift effect (diorama/miniature look)
        var tiltShiftEffect = Game.Content.Load<Effect>("Effects/TiltShift");
        tiltShiftEffect.Parameters["TexelSize"]?.SetValue(new Vector2(1f / viewport.Width, 1f / viewport.Height));
        tiltShiftEffect.Parameters["FocusCenter"]?.SetValue(0.5f);
        tiltShiftEffect.Parameters["FocusBand"]?.SetValue(0.25f);
        tiltShiftEffect.Parameters["BlurAmount"]?.SetValue(4f);

        // Load CRT effect
        var crtEffect = Game.Content.Load<Effect>("Effects/CRT");
        crtEffect.Parameters["ScreenSize"]?.SetValue(new Vector2(viewport.Width, viewport.Height));
        crtEffect.Parameters["Curvature"]?.SetValue(0.05f);
        crtEffect.Parameters["ChromaticAberration"]?.SetValue(0.025f);
        crtEffect.Parameters["Vignette"]?.SetValue(0.4f);
        crtEffect.Parameters["Brightness"]?.SetValue(1.15f);

        // Pipeline: render game layers → tilt-shift → CRT → UI on top
        _inGamePipeline = new RenderPipeline()
            .Add(new RenderLayersStep { Output = _sceneCapture, LayerEnd = RenderLayers.UI })
            //.Add(new ApplyEffectStep { Effect = tiltShiftEffect, Output = null })
            .Add(new ApplyEffectStep { Effect = tiltShiftEffect, Output = _postProcess })
            .Add(new ApplyEffectStep { Effect = crtEffect, Output = null })
            .Add(new RenderLayersStep { Output = null, ClearTarget = false });

        RenderService.SetPipeline(_inGamePipeline);

        // Capture pipeline for overlay backgrounds (game layers only, no CRT)
        _capturePipeline = new RenderPipeline()
            .Add(new RenderLayersStep { Output = _sceneCapture, LayerEnd = RenderLayers.UI });

        // Register overlay scenes with player data and scene capture
        SceneManager.Instance.AddScene(SceneNames.CharacterPanel,
            new CharacterPanelScene(Game, inventoryComponent, statsComponent, _sceneCapture));
        SceneManager.Instance.AddScene(SceneNames.MetricsPanel,
            new MetricsPanelScene(Game, statsComponent, _sceneCapture));
    }
}

# Scene Services & Overlay Scenes Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Refactor to per-scene services and overlay scenes, removing pause checks from components.

**Architecture:** Extract GameServicesCollection for reusable service container. Scene gets its own services. SceneManager becomes singleton with scene stack. Modal panels become overlay scenes. InputBindings becomes InputService registered per-scene.

**Tech Stack:** C#, MonoGame, Solo engine

---

## Task 1: Create GameServicesCollection

**Files:**
- Create: `Solo/Services/GameServicesCollection.cs`

**Step 1: Create the service collection class**

```csharp
using Microsoft.Xna.Framework;

namespace Solo.Services;

public class GameServicesCollection
{
    private readonly Dictionary<Type, IGameService> _servicesMap = new();
    private readonly List<IGameService> _services = new();
    private bool _isInitialized;

    public T GetRequired<T>() where T : class, IGameService
    {
        if (!_servicesMap.TryGetValue(typeof(T), out var service))
            throw new Exceptions.ServiceNotFoundException<T>();
        return (T)service;
    }

    public T? Get<T>() where T : class, IGameService
    {
        _servicesMap.TryGetValue(typeof(T), out var service);
        return service as T;
    }

    public void Add(IGameService service)
    {
        if (service == null)
            throw new ArgumentNullException(nameof(service));
        var serviceType = service.GetType();
        if (_servicesMap.ContainsKey(serviceType))
            throw new ArgumentException($"Service of type '{serviceType.Name}' already exists");
        _services.Add(service);
        _servicesMap[serviceType] = service;
    }

    public void Step(GameTime gameTime)
    {
        if (!_isInitialized)
        {
            foreach (var service in _services)
                service.Initialize();
            _isInitialized = true;
        }

        foreach (var service in _services)
            service.Step(gameTime);
    }
}
```

**Step 2: Build to verify**

Run: `dotnet build Solo/Solo.csproj`
Expected: Build succeeded

---

## Task 2: Refactor GameServicesManager to use GameServicesCollection

**Files:**
- Modify: `Solo/Services/GameServicesManager.cs`

**Step 1: Refactor to use GameServicesCollection internally**

```csharp
namespace Solo.Services;

public sealed class GameServicesManager
{
    private readonly GameServicesCollection _services = new();

    private GameServicesManager() { }

    private static readonly Lazy<GameServicesManager> _instance = new(() => new GameServicesManager());
    public static GameServicesManager Instance => _instance.Value;

    public T GetRequired<T>() where T : class, IGameService
        => _services.GetRequired<T>();

    public T? Get<T>() where T : class, IGameService
        => _services.Get<T>();

    public void AddService(IGameService service)
        => _services.Add(service);

    public void Step(Microsoft.Xna.Framework.GameTime gameTime)
        => _services.Step(gameTime);
}
```

**Step 2: Build to verify**

Run: `dotnet build Solo/Solo.csproj`
Expected: Build succeeded

---

## Task 3: Create SceneObjectsGraph Service

**Files:**
- Create: `Solo/Services/SceneObjectsGraph.cs`

**Step 1: Create the service**

```csharp
using Microsoft.Xna.Framework;

namespace Solo.Services;

public class SceneObjectsGraph : IGameService
{
    public GameObject Root { get; } = new();

    public void Step(GameTime gameTime)
    {
        Root.Update(gameTime);
    }
}
```

**Step 2: Build to verify**

Run: `dotnet build Solo/Solo.csproj`
Expected: Build succeeded

---

## Task 4: Refactor Scene to use Services Collection

**Files:**
- Modify: `Solo/Services/Scene.cs`

**Step 1: Add services collection and SceneObjectsGraph**

```csharp
using Microsoft.Xna.Framework;

namespace Solo.Services;

public abstract class Scene
{
    public Game Game { get; }
    public GameServicesCollection Services { get; } = new();

    private Lazy<SceneObjectsGraph>? _objectsGraph;
    public SceneObjectsGraph ObjectsGraph => _objectsGraph!.Value;

    protected Scene(Game game)
    {
        Game = game ?? throw new ArgumentNullException(nameof(game));
    }

    public void Enter()
    {
        Services.Add(new SceneObjectsGraph());
        _objectsGraph = new Lazy<SceneObjectsGraph>(() => Services.GetRequired<SceneObjectsGraph>());
        EnterCore();
    }

    public void Step(GameTime gameTime)
    {
        Services.Step(gameTime);
        Update(gameTime);
    }

    public void Exit()
    {
        ExitCore();
    }

    protected virtual void EnterCore() { }
    protected virtual void ExitCore() { }
    protected virtual void Update(GameTime gameTime) { }
}
```

**Step 2: Build to verify**

Run: `dotnet build Solo/Solo.csproj`
Expected: Build succeeded

---

## Task 5: Refactor SceneManager to Singleton with Scene Stack

**Files:**
- Modify: `Solo/Services/SceneManager.cs`

**Step 1: Convert to singleton with stack**

```csharp
using Microsoft.Xna.Framework;

namespace Solo.Services;

public sealed class SceneManager
{
    private static readonly Lazy<SceneManager> _instance = new(() => new SceneManager());
    public static SceneManager Instance => _instance.Value;

    private SceneManager() { }

    private readonly Dictionary<string, Scene> _scenes = new();
    private readonly Stack<Scene> _sceneStack = new();

    public Scene? Current => _sceneStack.Count > 0 ? _sceneStack.Peek() : null;

    public void AddScene(string name, Scene scene)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentNullException(nameof(name));
        if (scene == null)
            throw new ArgumentNullException(nameof(scene));
        _scenes.Add(name, scene);
    }

    public void SetScene(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentNullException(nameof(name));
        if (!_scenes.ContainsKey(name))
            throw new ArgumentOutOfRangeException(nameof(name), $"Invalid scene name: '{name}'");

        while (_sceneStack.Count > 0)
            PopScene();

        PushScene(name);
    }

    public void PushScene(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentNullException(nameof(name));
        if (!_scenes.TryGetValue(name, out var scene))
            throw new ArgumentOutOfRangeException(nameof(name), $"Invalid scene name: '{name}'");

        _sceneStack.Push(scene);
        scene.Enter();
        OnSceneChanged?.Invoke(scene);
    }

    public void PopScene()
    {
        if (_sceneStack.Count == 0)
            return;

        var scene = _sceneStack.Pop();
        scene.Exit();

        if (_sceneStack.Count > 0)
            OnSceneChanged?.Invoke(_sceneStack.Peek());
    }

    public void Step(GameTime gameTime)
    {
        Current?.Step(gameTime);
    }

    public event OnSceneChangedHandler? OnSceneChanged;
    public delegate void OnSceneChangedHandler(Scene currentScene);
}
```

**Step 2: Build to verify**

Run: `dotnet build Solo/Solo.csproj`
Expected: Build succeeded

---

## Task 6: Refactor RenderService to Singleton

**Files:**
- Modify: `Solo/Services/RenderService.cs`

**Step 1: Convert to singleton**

```csharp
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Solo.Services;

public sealed class RenderService
{
    private static RenderService? _instance;
    public static RenderService Instance => _instance ?? throw new InvalidOperationException("RenderService not initialized");

    public readonly GraphicsDeviceManager Graphics;
    public readonly GameWindow Window;

    private readonly SpriteBatch _spriteBatch;
    private SortedList<int, IList<IRenderable>> _layers = new();
    private Dictionary<int, RenderLayerConfig> _layerConfigs = new();

    public RenderService(GraphicsDeviceManager graphics, GameWindow window)
    {
        if (_instance != null)
            throw new InvalidOperationException("RenderService already initialized");

        Graphics = graphics ?? throw new ArgumentNullException(nameof(graphics));
        Window = window ?? throw new ArgumentNullException(nameof(window));

        window.ClientSizeChanged += (sender, args) =>
        {
            Graphics.PreferredBackBufferWidth = window.ClientBounds.Width;
            Graphics.PreferredBackBufferHeight = window.ClientBounds.Height;
            Graphics.ApplyChanges();
        };

        _spriteBatch = new SpriteBatch(Graphics.GraphicsDevice);
        _instance = this;
    }

    public void SetLayerConfig(int index, RenderLayerConfig? layerConfig)
    {
        if (layerConfig is null)
        {
            if (_layerConfigs.ContainsKey(index))
                _layerConfigs.Remove(index);
            return;
        }

        _layerConfigs[index] = layerConfig!.Value;
    }

    public void Step()
    {
        foreach (var layerIndex in _layers.Keys)
        {
            var layer = _layers[layerIndex];
            layer.Clear();
        }

        var currentScene = SceneManager.Instance.Current;
        if (currentScene != null)
            RebuildLayers(currentScene.ObjectsGraph.Root, _layers);
    }

    public void Render()
    {
        Step();

        Graphics.GraphicsDevice.Clear(Color.Black);

        for (int i = 0; i != _layers.Count; i++)
        {
            var layerIndex = _layers.Keys[i];
            var layer = _layers[layerIndex];

            if (!_layerConfigs.TryGetValue(layerIndex, out var layerConfig))
                _spriteBatch.Begin();
            else
                _spriteBatch.Begin(samplerState: layerConfig.SamplerState);

            foreach (var renderable in layer)
                renderable.Render(_spriteBatch);

            _spriteBatch.End();
        }
    }

    private static void RebuildLayers(GameObject? node, SortedList<int, IList<IRenderable>> layers)
    {
        if (node == null || !node.Enabled)
            return;

        foreach (var component in node.Components)
            if (component is IRenderable renderable &&
                component.Initialized &&
                component.Owner.Enabled &&
                !renderable.Hidden)
            {
                if (!layers.ContainsKey(renderable.LayerIndex))
                    layers.Add(renderable.LayerIndex, new List<IRenderable>());
                layers[renderable.LayerIndex].Add(renderable);
            }

        foreach (var child in node.Children)
            RebuildLayers(child, layers);
    }
}
```

**Step 2: Build to verify**

Run: `dotnet build Solo/Solo.csproj`
Expected: Build succeeded

---

## Task 7: Delete GamePauseManager

**Files:**
- Delete: `Solo/Services/GamePauseManager.cs`

**Step 1: Delete the file**

Delete `Solo/Services/GamePauseManager.cs`

**Step 2: Build Solo to see dependent errors**

Run: `dotnet build Solo/Solo.csproj`
Expected: Build succeeded (no internal dependencies)

---

## Task 8: Create InputService in Solocaster

**Files:**
- Create: `games/Solocaster/Services/InputService.cs`

**Step 1: Create InputService based on InputBindings**

```csharp
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Solo.Services;

namespace Solocaster.Services;

public class InputService : IGameService
{
    private const string DefaultPath = "./data/settings/keybindings.json";

    private static readonly Dictionary<string, Keys> DefaultBindings = new()
    {
        { InputActions.MoveForward, Keys.W },
        { InputActions.MoveBackward, Keys.S },
        { InputActions.RotateLeft, Keys.A },
        { InputActions.RotateRight, Keys.D },
        { InputActions.Run, Keys.LeftShift },
        { InputActions.ToggleCombat, Keys.R },
        { InputActions.Interact, Keys.E },
        { InputActions.ToggleCharacterPanel, Keys.Tab },
        { InputActions.ToggleMinimap, Keys.M },
        { InputActions.ToggleMetrics, Keys.C },
        { InputActions.ToggleDebug, Keys.L }
    };

    private readonly Dictionary<string, Keys> _bindings = new();
    private KeyboardState _currentState;
    private KeyboardState _previousState;

    public InputService()
    {
        foreach (var kvp in DefaultBindings)
            _bindings[kvp.Key] = kvp.Value;

        LoadBindings();
    }

    private void LoadBindings()
    {
        if (!File.Exists(DefaultPath))
        {
            Console.WriteLine($"InputService: Config not found at {DefaultPath}, using defaults");
            return;
        }

        try
        {
            var json = File.ReadAllText(DefaultPath);
            var loaded = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
            if (loaded != null)
            {
                foreach (var kvp in loaded)
                {
                    if (Enum.TryParse<Keys>(kvp.Value, ignoreCase: true, out var key))
                        _bindings[kvp.Key] = key;
                    else
                        Console.WriteLine($"InputService: Unknown key '{kvp.Value}' for action '{kvp.Key}'");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"InputService: Error loading config: {ex.Message}");
        }
    }

    public void Step(GameTime gameTime)
    {
        _previousState = _currentState;
        _currentState = Keyboard.GetState();
    }

    public bool IsActionDown(string action)
    {
        if (!_bindings.TryGetValue(action, out var key))
            return false;
        return _currentState.IsKeyDown(key);
    }

    public bool IsActionPressed(string action)
    {
        if (!_bindings.TryGetValue(action, out var key))
            return false;
        return _currentState.IsKeyDown(key) && !_previousState.IsKeyDown(key);
    }

    public Keys GetKey(string action)
    {
        return _bindings.TryGetValue(action, out var key) ? key : Keys.None;
    }
}
```

**Step 2: Move InputActions to Services folder**

Create `games/Solocaster/Services/InputActions.cs`:

```csharp
namespace Solocaster.Services;

public static class InputActions
{
    public const string MoveForward = "MoveForward";
    public const string MoveBackward = "MoveBackward";
    public const string RotateLeft = "RotateLeft";
    public const string RotateRight = "RotateRight";
    public const string Run = "Run";
    public const string ToggleCombat = "ToggleCombat";
    public const string Interact = "Interact";
    public const string ToggleCharacterPanel = "ToggleCharacterPanel";
    public const string ToggleMinimap = "ToggleMinimap";
    public const string ToggleMetrics = "ToggleMetrics";
    public const string ToggleDebug = "ToggleDebug";
}
```

**Step 3: Build to verify**

Run: `dotnet build games/Solocaster/Solocaster.csproj`
Expected: Errors related to old InputBindings usage (expected at this point)

---

## Task 9: Delete InputBindings and InputActions from Input folder

**Files:**
- Delete: `games/Solocaster/Input/InputBindings.cs`
- Delete: `games/Solocaster/Input/InputActions.cs`

**Step 1: Delete the files**

Delete both files from the Input folder.

---

## Task 10: Update PlayScene to use new architecture

**Files:**
- Modify: `games/Solocaster/Scenes/PlayScene.cs`

**Step 1: Register InputService, use ObjectsGraph.Root**

Replace all `Root` references with `ObjectsGraph.Root`, register InputService, remove UIService from global:

```csharp
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
using Solocaster.Services;
using Solocaster.State;
using Solocaster.UI;

namespace Solocaster.Scenes;

public class PlayScene : Scene
{
    private const int FrameBufferScale = 2;

    private Lazy<InputService>? _inputService;
    public InputService InputService => _inputService!.Value;

    private Lazy<UIService>? _uiService;
    public UIService UIService => _uiService!.Value;

    public PlayScene(Game game) : base(game)
    {
    }

    protected override void EnterCore()
    {
        Services.Add(new InputService());
        _inputService = new Lazy<InputService>(() => Services.GetRequired<InputService>());

        Services.Add(new UIService(RenderService.Instance.Graphics));
        _uiService = new Lazy<UIService>(() => Services.GetRequired<UIService>());

        var spatialGrid = new SpatialGrid(bucketSize: 1f);

        UITheme.Load("./data/ui/theme.json");
        ItemTemplateLoader.LoadAllFromFolder("./data/templates/items/");
        CharacterTemplateLoader.LoadAll("./data/templates/character/");
        MonsterTemplateLoader.LoadAllFromFolder("./data/templates/monsters/");

        var frameBufferWidth = RenderService.Instance.Graphics.GraphicsDevice.Viewport.Height / FrameBufferScale;
        var frameBufferHeight = RenderService.Instance.Graphics.GraphicsDevice.Viewport.Width / FrameBufferScale;

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

        var playerBrain = new PlayerBrain(player, level.Map, InputService);
        player.Components.Add(playerBrain);

        var playerUIController = new PlayerUIController(player, InputService);
        player.Components.Add(playerUIController);

        ObjectsGraph.Root.AddChild(player);

        var monsters = LevelLoader.SpawnMonsters(levelPath, level, Game, ObjectsGraph.Root, spatialGrid, player);
        level.Monsters = monsters;

        var raycaster = new Raycaster(level, spatialGrid, frameBufferWidth, frameBufferHeight);
        playerBrain.Raycaster = raycaster;

        var frameTexture = new Texture2D(RenderService.Instance.Graphics.GraphicsDevice, frameBufferWidth, frameBufferHeight);

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
        UIService.SetTooltipFont(font);

        var debugUIEntity = new GameObject();
        var debugUI = new DebugUIRenderer(debugUIEntity, font, player);
        debugUIEntity.Components.Add(debugUI);
        debugUI.LayerIndex = 2;
        debugUIEntity.Enabled = false;
        ObjectsGraph.Root.AddChild(debugUIEntity);
        playerUIController.DebugUIEntity = debugUIEntity;

        var beltPanel = new BeltPanel(inventoryComponent, UIService.DragDropManager, font, Game);
        beltPanel.PositionAtBottom(
            RenderService.Instance.Graphics.GraphicsDevice.Viewport.Width,
            RenderService.Instance.Graphics.GraphicsDevice.Viewport.Height
        );
        UIService.AddWidget(beltPanel);

        var playerStatusPanel = new PlayerStatusPanel(statsComponent, Game);
        playerStatusPanel.PositionTopRight(RenderService.Instance.Graphics.GraphicsDevice.Viewport.Width);
        UIService.AddWidget(playerStatusPanel);

        var playerHandsEntity = new GameObject();
        var playerHandsRenderer = new PlayerHandsRenderer(playerHandsEntity, Game, inventoryComponent, playerBrain);
        playerHandsEntity.Components.Add(playerHandsRenderer);
        playerHandsRenderer.LayerIndex = 5;
        ObjectsGraph.Root.AddChild(playerHandsEntity);
    }
}
```

Note: CharacterPanel and MetricsPanel are removed - they become overlay scenes.

**Step 2: Build to see remaining errors**

Run: `dotnet build games/Solocaster/Solocaster.csproj`
Expected: Errors for PlayerBrain and PlayerUIController constructors (expected)

---

## Task 11: Update PlayerBrain to use InputService

**Files:**
- Modify: `games/Solocaster/Components/PlayerBrain.cs`

**Step 1: Accept InputService in constructor, remove pause check**

Add `InputService` parameter to constructor, replace `InputBindings` with injected service, remove the pause check:

```csharp
// In constructor:
private readonly InputService _inputService;

public PlayerBrain(GameObject owner, Map map, InputService inputService) : base(owner)
{
    _map = map;
    _inputService = inputService;
}

// In InitCore - remove InputBindings.Initialize() call

// In UpdateCore - remove pause check, replace InputBindings with _inputService:
protected override void UpdateCore(GameTime gameTime)
{
    float ms = (float)gameTime.ElapsedGameTime.TotalMilliseconds;
    float baseSpeed = ms * 0.005f;
    float rotSpeed = ms * 0.005f;

    var mouseState = Mouse.GetState();

    HandlePickup(mouseState);
    HandleInteract();

    _stateMachine.Update(gameTime);

    float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
    HandleMovement(baseSpeed, deltaTime);
    HandleRotation(rotSpeed);

    _previousMouseState = mouseState;
    _context.PreviousMouseState = mouseState;
}

// Update all InputBindings references to _inputService
// Example: InputBindings.IsActionPressed(...) -> _inputService.IsActionPressed(...)
```

Update usings: remove `Solocaster.Input`, add `Solocaster.Services`, remove `Solo.Services` (GamePauseManager), remove `Solocaster.UI` (GamePauseManager).

**Step 2: Build to verify**

Run: `dotnet build games/Solocaster/Solocaster.csproj`
Expected: More errors (expected, continuing fixes)

---

## Task 12: Update PlayerUIController to use InputService

**Files:**
- Modify: `games/Solocaster/Components/PlayerUIController.cs`

**Step 1: Accept InputService, push overlay scenes instead of Toggle**

```csharp
using Microsoft.Xna.Framework;
using Solo;
using Solo.Components;
using Solo.Services;
using Solocaster.Services;

namespace Solocaster.Components;

public class PlayerUIController : Component
{
    private readonly InputService _inputService;

    public GameObject? MiniMapEntity { get; set; }
    public GameObject? DebugUIEntity { get; set; }

    public PlayerUIController(GameObject owner, InputService inputService) : base(owner)
    {
        _inputService = inputService;
    }

    protected override void UpdateCore(GameTime gameTime)
    {
        if (_inputService.IsActionPressed(InputActions.ToggleCharacterPanel))
            SceneManager.Instance.PushScene(SceneNames.CharacterPanel);

        if (_inputService.IsActionPressed(InputActions.ToggleMinimap) && MiniMapEntity != null)
            MiniMapEntity.Enabled = !MiniMapEntity.Enabled;

        if (_inputService.IsActionPressed(InputActions.ToggleDebug) && DebugUIEntity != null)
            DebugUIEntity.Enabled = !DebugUIEntity.Enabled;

        if (_inputService.IsActionPressed(InputActions.ToggleMetrics))
            SceneManager.Instance.PushScene(SceneNames.MetricsPanel);
    }
}
```

**Step 2: Build to verify**

Run: `dotnet build games/Solocaster/Solocaster.csproj`
Expected: Errors for missing scene names and overlay scenes

---

## Task 13: Update SceneNames

**Files:**
- Modify: `games/Solocaster/Scenes/SceneNames.cs`

**Step 1: Add overlay scene names**

```csharp
namespace Solocaster.Scenes;

public static class SceneNames
{
    public const string CharacterBuilder = "CharacterBuilder";
    public const string Play = "Play";
    public const string CharacterPanel = "CharacterPanel";
    public const string MetricsPanel = "MetricsPanel";
}
```

**Step 2: Build to verify**

Run: `dotnet build games/Solocaster/Solocaster.csproj`
Expected: Errors for missing overlay scene classes

---

## Task 14: Create CharacterPanelScene

**Files:**
- Create: `games/Solocaster/Scenes/CharacterPanelScene.cs`

**Step 1: Create overlay scene**

```csharp
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Solo.Services;
using Solocaster.Components;
using Solocaster.Services;
using Solocaster.UI;

namespace Solocaster.Scenes;

public class CharacterPanelScene : Scene
{
    private readonly InventoryComponent _inventory;
    private readonly StatsComponent _stats;

    private Lazy<InputService>? _inputService;
    public InputService InputService => _inputService!.Value;

    private Lazy<UIService>? _uiService;
    public UIService UIService => _uiService!.Value;

    public CharacterPanelScene(Game game, InventoryComponent inventory, StatsComponent stats) : base(game)
    {
        _inventory = inventory;
        _stats = stats;
    }

    protected override void EnterCore()
    {
        Services.Add(new InputService());
        _inputService = new Lazy<InputService>(() => Services.GetRequired<InputService>());

        Services.Add(new UIService(RenderService.Instance.Graphics));
        _uiService = new Lazy<UIService>(() => Services.GetRequired<UIService>());

        var font = Game.Content.Load<SpriteFont>("Font");
        UIService.SetTooltipFont(font);

        var characterPanel = new CharacterPanel(_inventory, _stats, UIService.DragDropManager, font, Game);
        characterPanel.CenterOnScreen(
            RenderService.Instance.Graphics.GraphicsDevice.Viewport.Width,
            RenderService.Instance.Graphics.GraphicsDevice.Viewport.Height
        );
        characterPanel.Visible = true;
        UIService.AddWidget(characterPanel);
    }

    protected override void Update(GameTime gameTime)
    {
        if (InputService.IsActionPressed(InputActions.ToggleCharacterPanel))
        {
            SceneManager.Instance.PopScene();
        }
    }
}
```

**Step 2: Build to verify**

Run: `dotnet build games/Solocaster/Solocaster.csproj`
Expected: Build progresses (may still have errors)

---

## Task 15: Create MetricsPanelScene

**Files:**
- Create: `games/Solocaster/Scenes/MetricsPanelScene.cs`

**Step 1: Create overlay scene**

```csharp
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Solo.Services;
using Solocaster.Components;
using Solocaster.Services;
using Solocaster.UI;

namespace Solocaster.Scenes;

public class MetricsPanelScene : Scene
{
    private readonly StatsComponent _stats;

    private Lazy<InputService>? _inputService;
    public InputService InputService => _inputService!.Value;

    private Lazy<UIService>? _uiService;
    public UIService UIService => _uiService!.Value;

    public MetricsPanelScene(Game game, StatsComponent stats) : base(game)
    {
        _stats = stats;
    }

    protected override void EnterCore()
    {
        Services.Add(new InputService());
        _inputService = new Lazy<InputService>(() => Services.GetRequired<InputService>());

        Services.Add(new UIService(RenderService.Instance.Graphics));
        _uiService = new Lazy<UIService>(() => Services.GetRequired<UIService>());

        var font = Game.Content.Load<SpriteFont>("Font");
        UIService.SetTooltipFont(font);

        var metricsPanel = new MetricsPanel(_stats, font, Game);
        metricsPanel.CenterOnScreen(
            RenderService.Instance.Graphics.GraphicsDevice.Viewport.Width,
            RenderService.Instance.Graphics.GraphicsDevice.Viewport.Height
        );
        metricsPanel.Visible = true;
        UIService.AddWidget(metricsPanel);
    }

    protected override void Update(GameTime gameTime)
    {
        if (InputService.IsActionPressed(InputActions.ToggleMetrics))
        {
            SceneManager.Instance.PopScene();
        }
    }
}
```

**Step 2: Build to verify**

Run: `dotnet build games/Solocaster/Solocaster.csproj`
Expected: Build progresses

---

## Task 16: Update CharacterPanel - remove pause logic

**Files:**
- Modify: `games/Solocaster/UI/CharacterPanel.cs`

**Step 1: Remove Toggle method and pause references**

```csharp
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Solocaster.Components;
using Solocaster.UI.Widgets;

namespace Solocaster.UI;

public class CharacterPanel : Widget
{
    private const int PanelSpacing = 10;

    private readonly StatsPanel _statsPanel;
    private readonly InventoryPanel _inventoryPanel;

    public CharacterPanel(
        InventoryComponent inventory,
        StatsComponent stats,
        DragDropManager dragDropManager,
        SpriteFont font,
        Game game)
    {
        _statsPanel = new StatsPanel(stats, font, game);
        _inventoryPanel = new InventoryPanel(inventory, dragDropManager, font, game);

        _statsPanel.Position = Vector2.Zero;
        _inventoryPanel.Position = new Vector2(_statsPanel.Size.X + PanelSpacing, 0);

        Size = new Vector2(
            _statsPanel.Size.X + PanelSpacing + _inventoryPanel.Size.X,
            MathHelper.Max(_statsPanel.Size.Y, _inventoryPanel.Size.Y)
        );

        AddChild(_statsPanel);
        AddChild(_inventoryPanel);

        Visible = false;
    }

    public void CenterOnScreen(int screenWidth, int screenHeight)
    {
        Position = new Vector2(
            (screenWidth - Size.X) / 2,
            (screenHeight - Size.Y) / 2
        );
    }
}
```

**Step 2: Build to verify**

Run: `dotnet build games/Solocaster/Solocaster.csproj`
Expected: Build progresses

---

## Task 17: Update MetricsPanel - remove pause logic

**Files:**
- Modify: `games/Solocaster/UI/MetricsPanel.cs`

**Step 1: Remove Toggle method and pause references**

Remove the `Toggle()` method and `_pauseManager` field. Remove `GamePauseManager` using. Keep the rest of the class as-is.

**Step 2: Build to verify**

Run: `dotnet build games/Solocaster/Solocaster.csproj`
Expected: Build progresses

---

## Task 18: Update MonsterBrainComponent - remove pause check

**Files:**
- Modify: `games/Solocaster/Components/MonsterBrainComponent.cs`

**Step 1: Remove pause check and related usings**

Remove the pause check in `UpdateCore`, remove `Solo.Services` and `Solocaster.UI` usings:

```csharp
using System;
using Microsoft.Xna.Framework;
using Solo;
using Solo.Components;
using Solocaster.Monsters;

namespace Solocaster.Components;

public class MonsterBrainComponent : Component
{
    // ... existing fields ...

    protected override void UpdateCore(GameTime gameTime)
    {
        if (_spriteProvider == null || _player == null)
            return;

        UpdateDirectionToPlayer();
    }

    // ... rest unchanged ...
}
```

**Step 2: Build to verify**

Run: `dotnet build games/Solocaster/Solocaster.csproj`
Expected: Build progresses

---

## Task 19: Update UIService - remove pause overlay

**Files:**
- Modify: `games/Solocaster/UI/UIService.cs`

**Step 1: Remove GamePauseManager references and overlay**

Remove the pause overlay rendering code and `GamePauseManager` reference:

```csharp
public void Render()
{
    if (_spriteBatch == null)
        return;

    _spriteBatch.Begin(samplerState: SamplerState.PointClamp);

    foreach (var widget in _rootWidgets)
    {
        widget.Render(_spriteBatch);
    }

    DragDropManager?.Render(_spriteBatch, DragItemSize);
    _tooltip?.Render(_spriteBatch);

    _spriteBatch.End();
}
```

Remove `OverlayOpacity` constant and `_pixelTexture` field if no longer needed. Remove `Solo.Services` using.

**Step 2: Build to verify**

Run: `dotnet build games/Solocaster/Solocaster.csproj`
Expected: Build progresses

---

## Task 20: Update SolocasterGame

**Files:**
- Modify: `games/Solocaster/SolocasterGame.cs`

**Step 1: Use singleton SceneManager and RenderService**

```csharp
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Solo.Services;
using Solocaster.Scenes;

namespace Solocaster;

public class SolocasterGame : Game
{
    private GraphicsDeviceManager _graphics;

    private const int ScreenWidth = 1600;
    private const int ScreenHeight = 1200;

    public SolocasterGame()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        _graphics.PreferredBackBufferWidth = ScreenWidth;
        _graphics.PreferredBackBufferHeight = ScreenHeight;
        _graphics.ApplyChanges();

        // Initialize RenderService singleton
        _ = new RenderService(_graphics, Window);

        base.Initialize();
    }

    protected override void LoadContent()
    {
        SceneManager.Instance.AddScene(SceneNames.CharacterBuilder, new CharacterBuilderScene(this));
        SceneManager.Instance.AddScene(SceneNames.Play, new PlayScene(this));
        SceneManager.Instance.SetScene(SceneNames.CharacterBuilder);
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        SceneManager.Instance.Step(gameTime);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        RenderService.Instance.Render();

        // Render UI for current scene if it has UIService
        if (SceneManager.Instance.Current is PlayScene playScene)
            playScene.UIService.Render();
        else if (SceneManager.Instance.Current is CharacterPanelScene charScene)
            charScene.UIService.Render();
        else if (SceneManager.Instance.Current is MetricsPanelScene metricsScene)
            metricsScene.UIService.Render();

        base.Draw(gameTime);
    }
}
```

**Step 2: Build to verify**

Run: `dotnet build games/Solocaster/Solocaster.csproj`
Expected: Build progresses

---

## Task 21: Fix overlay scene registration

**Files:**
- Modify: `games/Solocaster/Scenes/PlayScene.cs`

**Step 1: Register overlay scenes with player data**

The overlay scenes need player components (inventory, stats). We need to register them after the player is created. Add to end of `EnterCore()`:

```csharp
// Register overlay scenes with player data
SceneManager.Instance.AddScene(SceneNames.CharacterPanel,
    new CharacterPanelScene(Game, inventoryComponent, statsComponent));
SceneManager.Instance.AddScene(SceneNames.MetricsPanel,
    new MetricsPanelScene(Game, statsComponent));
```

**Step 2: Build to verify**

Run: `dotnet build games/Solocaster/Solocaster.csproj`
Expected: Build succeeded

---

## Task 22: Update CharacterBuilderScene for new architecture

**Files:**
- Modify: `games/Solocaster/Scenes/CharacterBuilderScene.cs`

**Step 1: Update to use ObjectsGraph.Root and new services**

Review and update the CharacterBuilderScene to use `ObjectsGraph.Root` instead of `Root`, and adapt to the new architecture.

**Step 2: Build full solution**

Run: `dotnet build games/Solocaster/Solocaster.csproj`
Expected: Build succeeded

---

## Task 23: Final build and test

**Step 1: Full build**

Run: `dotnet build games/Solocaster/Solocaster.csproj`
Expected: Build succeeded with 0 errors

**Step 2: Run the game**

Run: `dotnet run --project games/Solocaster/Solocaster.csproj`
Expected: Game starts, Tab opens character panel overlay, C opens metrics panel overlay, pressing same key closes overlay and resumes game.

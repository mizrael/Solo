# Design: Scene Services & Overlay Scenes

## Problem

Components (PlayerBrain, MonsterBrainComponent) currently check pause state directly, coupling game logic to UI state. UIService and InputBindings are global, making it awkward to handle input when modal panels are open.

## Solution

Refactor to support per-scene services and overlay scenes. Modal panels become separate scenes that render on top of the play scene. Only the topmost scene updates, naturally pausing the game underneath.

## Architecture

### GameServicesCollection

Extracted reusable service container:

```csharp
public class GameServicesCollection
{
    private readonly Dictionary<Type, IGameService> _servicesMap = new();
    private readonly List<IGameService> _services = new();
    private bool _isInitialized;

    public T GetRequired<T>() where T : class, IGameService;
    public T? Get<T>() where T : class, IGameService;
    public void Add(IGameService service);
    public void Step(GameTime gameTime);
}
```

### SceneManager (Singleton)

Manages a stack of scenes. Only the topmost scene updates:

```csharp
public sealed class SceneManager
{
    private static readonly Lazy<SceneManager> _instance = new(() => new SceneManager());
    public static SceneManager Instance => _instance.Value;

    private readonly Dictionary<string, Scene> _scenes = new();
    private readonly Stack<Scene> _sceneStack = new();

    public Scene? Current => _sceneStack.Count > 0 ? _sceneStack.Peek() : null;

    public void AddScene(string name, Scene scene);
    public void SetScene(string name);  // Clears stack, pushes new base
    public void PushScene(string name); // Pushes overlay, calls Enter()
    public void PopScene();             // Pops top, calls Exit(), previous resumes

    public void Step(GameTime gameTime)
    {
        Current?.Step(gameTime);
    }
}
```

### SceneObjectsGraph

Service that hosts the Root GameObject:

```csharp
public class SceneObjectsGraph : IGameService
{
    public GameObject Root { get; } = new();

    public void Step(GameTime gameTime)
    {
        Root.Update(gameTime);
    }
}
```

### Scene Base Class

Each scene has its own services collection:

```csharp
public abstract class Scene
{
    public Game Game { get; }
    public GameServicesCollection Services { get; } = new();

    private Lazy<SceneObjectsGraph> _objectsGraph;
    public SceneObjectsGraph ObjectsGraph => _objectsGraph.Value;

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

### RenderService (Singleton)

```csharp
public sealed class RenderService
{
    private static readonly Lazy<RenderService> _instance = new(() => new RenderService());
    public static RenderService Instance => _instance.Value;
    // ... existing rendering logic
}
```

### InputService

Replaces static InputBindings. Registered per-scene:

```csharp
public class InputService : IGameService
{
    private readonly Dictionary<string, Keys> _bindings = new();
    private KeyboardState _currentState;
    private KeyboardState _previousState;

    public void Step(GameTime gameTime)
    {
        _previousState = _currentState;
        _currentState = Keyboard.GetState();
    }

    public bool IsActionDown(string action);
    public bool IsActionPressed(string action);
}
```

### Overlay Scenes

Modal panels become lightweight scenes:

```csharp
public class CharacterPanelScene : Scene
{
    private Lazy<UIService> _uiService;
    public UIService UIService => _uiService.Value;

    protected override void EnterCore()
    {
        Services.Add(new UIService());
        _uiService = new Lazy<UIService>(() => Services.GetRequired<UIService>());

        Services.Add(new InputService());

        var characterPanel = new CharacterPanel(/* ... */);
        UIService.AddWidget(characterPanel);
    }
}
```

Toggle from PlayScene pushes overlay:

```csharp
if (inputService.IsActionPressed(InputActions.ToggleCharacterPanel))
{
    SceneManager.Instance.PushScene(SceneNames.CharacterPanel);
}
```

Overlay closes itself:

```csharp
if (inputService.IsActionPressed(InputActions.ToggleCharacterPanel))
{
    SceneManager.Instance.PopScene();
}
```

### Game Loop

```csharp
protected override void Update(GameTime gameTime)
{
    SceneManager.Instance.Step(gameTime);
}

protected override void Draw(GameTime gameTime)
{
    RenderService.Instance.Render();
}
```

## Files

### Solo Engine (new/modified)

| File | Action |
|------|--------|
| `Services/GameServicesCollection.cs` | Create |
| `Services/GameServicesManager.cs` | Modify - use GameServicesCollection |
| `Services/SceneManager.cs` | Modify - singleton, scene stack |
| `Services/RenderService.cs` | Modify - singleton |
| `Services/Scene.cs` | Modify - own services, SceneObjectsGraph |
| `Services/SceneObjectsGraph.cs` | Create |
| `Services/GamePauseManager.cs` | Delete |

### Solocaster (new/modified)

| File | Action |
|------|--------|
| `Services/InputService.cs` | Create (from InputBindings) |
| `Input/InputBindings.cs` | Delete |
| `Scenes/CharacterPanelScene.cs` | Create |
| `Scenes/MetricsPanelScene.cs` | Create |
| `Scenes/PlayScene.cs` | Modify - register services, push overlays |
| `Components/PlayerBrain.cs` | Modify - remove pause check |
| `Components/MonsterBrainComponent.cs` | Modify - remove pause check |
| `SolocasterGame.cs` | Modify - use singleton SceneManager/RenderService |

# Design: Game Pause & Overlay System

## Problem

When opening UI panels (inventory, metrics, future game menus), the game continues running in the background. This is distracting and allows unintended gameplay while navigating menus.

## Solution

Pause the game and display a dark semi-transparent overlay when modal panels are open. The same key that opens a panel can close it.

## Architecture

### GamePauseManager

Central coordinator that tracks pause state:

```csharp
public class GamePauseManager
{
    private readonly HashSet<object> _pauseRequesters = new();

    public bool IsPaused => _pauseRequesters.Count > 0;

    public event Action<bool>? OnPauseChanged;

    public void RequestPause(object requester)
    {
        if (_pauseRequesters.Add(requester) && _pauseRequesters.Count == 1)
            OnPauseChanged?.Invoke(true);
    }

    public void ReleasePause(object requester)
    {
        if (_pauseRequesters.Remove(requester) && _pauseRequesters.Count == 0)
            OnPauseChanged?.Invoke(false);
    }
}
```

HashSet of requesters ensures:
- Multiple panels can be open simultaneously
- Pause ends only when all modal panels close
- No double-pause/unpause bugs

### Panel Integration

Modal panels request/release pause on toggle:

```csharp
public void Toggle()
{
    Visible = !Visible;
    if (Visible)
        _pauseManager.RequestPause(this);
    else
        _pauseManager.ReleasePause(this);
}
```

### Modal vs Non-Modal

| Panel | Type | Pauses Game |
|-------|------|-------------|
| CharacterPanel | Modal | Yes |
| MetricsPanel | Modal | Yes |
| Future game menu | Modal | Yes |
| Minimap | HUD | No |
| PlayerStatusPanel | HUD | No |
| BeltPanel | HUD | No |

### Dark Overlay

Simple semi-transparent rectangle drawn when paused:

```csharp
if (_pauseManager.IsPaused)
{
    spriteBatch.Draw(_pixelTexture,
        new Rectangle(0, 0, screenWidth, screenHeight),
        Color.Black * 0.6f);
}
```

Render order:
1. Game world (raycaster)
2. Dark overlay (when paused)
3. UI panels

### Game Loop Changes

```csharp
protected override void Update(GameTime gameTime)
{
    // UI input always processed (can close panels)
    _uiController.Update(gameTime);

    if (!_pauseManager.IsPaused)
    {
        // Game logic only when not paused
        _playerBrain.Update(gameTime);
        _monsters.Update(gameTime);
        _doors.Update(gameTime);
        // etc.
    }
}
```

## Future: Shader Pipeline

When implementing per-pixel lighting, shadows, or ambient occlusion, add a PostProcessPipeline:

```csharp
public class PostProcessPipeline
{
    private readonly PriorityQueue<PostProcessEffect, int> _effects;
    private RenderTarget2D _targetA, _targetB;

    public Texture2D Process(Texture2D source);
}

public abstract class PostProcessEffect
{
    public bool Enabled { get; set; }
    public abstract void Apply(SpriteBatch batch, Texture2D source, RenderTarget2D dest);
}
```

The dark overlay could migrate to a shader effect later, but simple rectangle rendering suffices for now.

## Files

| File | Action |
|------|--------|
| `Services/GamePauseManager.cs` | Create |
| `UI/CharacterPanel.cs` | Modify - add pause integration |
| `UI/MetricsPanel.cs` | Modify - add pause integration |
| `Scenes/PlayScene.cs` | Modify - render overlay, skip updates when paused |

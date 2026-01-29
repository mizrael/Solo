# Combat Mode, Running & Stamina Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Add combat mode toggle, hold-to-run with stamina consumption, and configurable key bindings.

**Architecture:** PlayerBrain manages state machine and reads from InputBindings. StatsComponent handles stamina as a new resource. PlayerHandsRenderer reads state for visibility transitions.

**Tech Stack:** C#, MonoGame, System.Text.Json for keybindings

---

## Task 1: Create PlayerState Enum

**Files:**
- Create: `games/Solocaster/Components/PlayerState.cs`

**Step 1: Create the enum file**

```csharp
namespace Solocaster.Components;

public enum PlayerState
{
    Exploring,  // Default - hands lowered, normal walk speed
    Combat,     // Hands raised, ready for action
    Running,    // Fast movement, hands visible, draining stamina
    Exhausted   // Recovery after stamina depletion, reduced speed
}
```

**Step 2: Verify it compiles**

Run: `dotnet build games/Solocaster/Solocaster.csproj`
Expected: Build succeeded

---

## Task 2: Create InputBindings Class

**Files:**
- Create: `games/Solocaster/Input/InputBindings.cs`
- Create: `games/Solocaster/data/settings/keybindings.json`

**Step 1: Create the InputBindings class**

```csharp
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Microsoft.Xna.Framework.Input;

namespace Solocaster.Input;

public class InputBindings
{
    private const string DefaultPath = "./data/settings/keybindings.json";

    private static readonly Dictionary<string, Keys> DefaultBindings = new()
    {
        { "moveForward", Keys.W },
        { "moveBackward", Keys.S },
        { "rotateLeft", Keys.A },
        { "rotateRight", Keys.D },
        { "run", Keys.LeftShift },
        { "toggleCombat", Keys.R },
        { "interact", Keys.E },
        { "toggleCharacterPanel", Keys.Tab },
        { "toggleMinimap", Keys.M },
        { "toggleMetrics", Keys.C },
        { "toggleDebug", Keys.L }
    };

    private readonly Dictionary<string, Keys> _bindings = new();
    private KeyboardState _currentState;
    private KeyboardState _previousState;

    public InputBindings()
    {
        LoadBindings();
    }

    private void LoadBindings()
    {
        // Start with defaults
        foreach (var kvp in DefaultBindings)
            _bindings[kvp.Key] = kvp.Value;

        if (!File.Exists(DefaultPath))
        {
            Console.WriteLine($"InputBindings: Config not found at {DefaultPath}, using defaults");
            return;
        }

        try
        {
            var json = File.ReadAllText(DefaultPath);
            var loaded = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
            if (loaded == null) return;

            foreach (var kvp in loaded)
            {
                if (Enum.TryParse<Keys>(kvp.Value, ignoreCase: true, out var key))
                    _bindings[kvp.Key] = key;
                else
                    Console.WriteLine($"InputBindings: Unknown key '{kvp.Value}' for action '{kvp.Key}'");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"InputBindings: Error loading config: {ex.Message}");
        }
    }

    public void Update()
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

**Step 2: Create the data/settings directory and keybindings.json**

```json
{
  "moveForward": "W",
  "moveBackward": "S",
  "rotateLeft": "A",
  "rotateRight": "D",
  "run": "LeftShift",
  "toggleCombat": "R",
  "interact": "E",
  "toggleCharacterPanel": "Tab",
  "toggleMinimap": "M",
  "toggleMetrics": "C",
  "toggleDebug": "L"
}
```

**Step 3: Verify it compiles**

Run: `dotnet build games/Solocaster/Solocaster.csproj`
Expected: Build succeeded

---

## Task 3: Add Stamina to StatsComponent

**Files:**
- Modify: `games/Solocaster/Components/StatsComponent.cs`

**Step 1: Add stamina fields and properties after `_currentMana` (around line 22)**

Add these fields:
```csharp
private float _currentStamina;
private bool _isExhausted;
private float _exhaustedTimer;
private float _lastRunTime;
private float _lastCombatActionTime;
```

Add these constants after existing constants (around line 15):
```csharp
private const float StaminaDrainRate = 15f;
private const float StaminaRegenDelay = 0.5f;
private const float CombatActionRegenPause = 1.5f;
private const float ExhaustedDuration = 2f;
```

**Step 2: Add CurrentStamina property after CurrentMana property**

```csharp
public float CurrentStamina
{
    get => _currentStamina;
    set
    {
        _currentStamina = Math.Clamp(value, 0, MaxStamina);
        OnStatsChanged?.Invoke();
    }
}

public float MaxStamina => 50 + GetTotalStat(StatType.Vitality) * 5;
public float StaminaRegenRate => 5 + GetTotalStat(StatType.Agility) * 0.5f;
public bool IsExhausted => _isExhausted;
```

**Step 3: Add stamina methods**

```csharp
public void DrainStamina(float deltaTime)
{
    CurrentStamina -= StaminaDrainRate * deltaTime;
    _lastRunTime = 0f; // Reset timer while running

    if (CurrentStamina <= 0)
    {
        CurrentStamina = 0;
        _isExhausted = true;
        _exhaustedTimer = ExhaustedDuration;
    }
}

public void UpdateStamina(float deltaTime, bool isRunning)
{
    // Update exhausted state
    if (_isExhausted)
    {
        _exhaustedTimer -= deltaTime;
        if (_exhaustedTimer <= 0)
        {
            _isExhausted = false;
        }
        return; // No regen while exhausted
    }

    // Track time since last run
    if (!isRunning)
        _lastRunTime += deltaTime;

    // Track time since combat action
    _lastCombatActionTime += deltaTime;

    // Regenerate if conditions met
    bool canRegen = !isRunning
        && _lastRunTime >= StaminaRegenDelay
        && _lastCombatActionTime >= CombatActionRegenPause;

    if (canRegen && CurrentStamina < MaxStamina)
    {
        CurrentStamina += StaminaRegenRate * deltaTime;
    }
}

public void OnCombatAction()
{
    _lastCombatActionTime = 0f;
}
```

**Step 4: Initialize stamina in InitCore (around line 199)**

Add after mana initialization:
```csharp
_currentStamina = MaxStamina;
```

**Step 5: Verify it compiles**

Run: `dotnet build games/Solocaster/Solocaster.csproj`
Expected: Build succeeded

---

## Task 4: Add Stamina Colors to UITheme

**Files:**
- Modify: `games/Solocaster/UI/UITheme.cs`

**Step 1: Add stamina colors to StatusBarColors class (around line 296)**

```csharp
public Color StaminaFill { get; set; } = new Color(200, 180, 40);
public Color StaminaBackground { get; set; } = new Color(60, 50, 20);
```

**Step 2: Add stamina to StatusBarColorsJson class (around line 220)**

```csharp
public int[]? StaminaFill { get; set; }
public int[]? StaminaBackground { get; set; }
```

**Step 3: Update ParseStatusBarColors method to include stamina (around line 119)**

Add these lines in the return statement:
```csharp
StaminaFill = ParseColor(json.StaminaFill) ?? new Color(200, 180, 40),
StaminaBackground = ParseColor(json.StaminaBackground) ?? new Color(60, 50, 20)
```

**Step 4: Verify it compiles**

Run: `dotnet build games/Solocaster/Solocaster.csproj`
Expected: Build succeeded

---

## Task 5: Add Stamina Bar to PlayerStatusPanel

**Files:**
- Modify: `games/Solocaster/UI/PlayerStatusPanel.cs`

**Step 1: Update panel size calculation (around line 39)**

Change the height calculation to accommodate 3 bars:
```csharp
int totalHeight = Padding * 2 + Math.Max(AvatarSize, BarHeight * 3 + BarSpacing * 2);
```

**Step 2: Update RenderCore to add stamina bar (after mana bar, around line 107)**

Add after mana bar rendering:
```csharp
// Stamina bar
int staminaBarY = manaBarY + BarHeight + BarSpacing;
float staminaRatio = _stats.CurrentStamina / _stats.MaxStamina;
Color staminaFill = _stats.IsExhausted
    ? PulseColor(UITheme.StatusBar.StaminaFill, 0.5f)
    : UITheme.StatusBar.StaminaFill;
DrawBar(spriteBatch, barX, staminaBarY, staminaRatio, staminaFill, UITheme.StatusBar.StaminaBackground);
```

**Step 3: Add PulseColor helper method**

```csharp
private Color PulseColor(Color baseColor, float intensity)
{
    float pulse = (float)(Math.Sin(DateTime.Now.Ticks / 1000000.0 * 10) * 0.5 + 0.5);
    float factor = 1f - (pulse * intensity);
    return new Color(
        (int)(baseColor.R * factor),
        (int)(baseColor.G * factor),
        (int)(baseColor.B * factor),
        baseColor.A
    );
}
```

**Step 4: Verify it compiles**

Run: `dotnet build games/Solocaster/Solocaster.csproj`
Expected: Build succeeded

---

## Task 6: Update PlayerBrain with State Machine and InputBindings

**Files:**
- Modify: `games/Solocaster/Components/PlayerBrain.cs`

**Step 1: Add using and fields (at top of class)**

Add using:
```csharp
using Solocaster.Input;
```

Add fields after existing fields (around line 22):
```csharp
private PlayerState _state = PlayerState.Exploring;
private PlayerState _previousStateBeforeRun = PlayerState.Exploring;
private InputBindings? _inputBindings;

private const float RunningSpeedMultiplier = 1.8f;
private const float ExhaustedSpeedMultiplier = 0.6f;
```

Add public property:
```csharp
public PlayerState State => _state;
public InputBindings? InputBindings => _inputBindings;
```

**Step 2: Update InitCore to create InputBindings**

Add in InitCore:
```csharp
_inputBindings = new InputBindings();
```

**Step 3: Refactor UpdateCore to use InputBindings and state machine**

Replace the keyboard handling with InputBindings calls. The key changes:

At start of UpdateCore, add:
```csharp
_inputBindings?.Update();
```

Replace `keyboardState.IsKeyDown(Keys.Tab) && !_previousKeyboardState.IsKeyDown(Keys.Tab)` with:
```csharp
_inputBindings?.IsActionPressed("toggleCharacterPanel") == true
```

Do similar replacements for all key checks:
- `Keys.M` → `"toggleMinimap"`
- `Keys.L` → `"toggleDebug"`
- `Keys.C` → `"toggleMetrics"`
- `Keys.E` → `"interact"`
- `Keys.W` → `"moveForward"`
- `Keys.S` → `"moveBackward"`
- `Keys.A` → `"rotateLeft"`
- `Keys.D` → `"rotateRight"`

**Step 4: Add combat mode toggle**

After the panel toggles section:
```csharp
// Toggle combat mode with R
if (_inputBindings?.IsActionPressed("toggleCombat") == true && _state != PlayerState.Running && _state != PlayerState.Exhausted)
{
    _state = _state == PlayerState.Combat ? PlayerState.Exploring : PlayerState.Combat;
}
```

**Step 5: Add running logic**

Replace the movement section with state-aware logic:

```csharp
float moveAmount = 0;
bool wantsToRun = _inputBindings?.IsActionDown("run") == true;
bool movingForward = _inputBindings?.IsActionDown("moveForward") == true;
bool movingBackward = _inputBindings?.IsActionDown("moveBackward") == true;

if (movingForward)
    moveAmount = moveSpeed;
else if (movingBackward)
    moveAmount = -moveSpeed;

// Handle running state
if (_state == PlayerState.Exhausted)
{
    // Reduced speed while exhausted
    moveAmount *= ExhaustedSpeedMultiplier;
    _stats?.UpdateStamina((float)gameTime.ElapsedGameTime.TotalSeconds, false);

    if (!_stats?.IsExhausted ?? true)
    {
        _state = _previousStateBeforeRun;
    }
}
else if (wantsToRun && movingForward && _stats?.CurrentStamina > 0 && _state != PlayerState.Exhausted)
{
    // Start or continue running
    if (_state != PlayerState.Running)
    {
        _previousStateBeforeRun = _state;
        _state = PlayerState.Running;
    }

    moveAmount *= RunningSpeedMultiplier;
    _stats?.DrainStamina((float)gameTime.ElapsedGameTime.TotalSeconds);

    if (_stats?.IsExhausted ?? false)
    {
        _state = PlayerState.Exhausted;
    }
}
else
{
    // Not running
    if (_state == PlayerState.Running)
    {
        _state = _previousStateBeforeRun;
    }
    _stats?.UpdateStamina((float)gameTime.ElapsedGameTime.TotalSeconds, false);
}

CurrentMoveSpeed = MathF.Abs(moveAmount);
```

**Step 6: Update rotation checks to use InputBindings**

Replace rotation key checks similarly.

**Step 7: Remove _previousKeyboardState usage**

Since InputBindings handles previous state internally, remove `_previousKeyboardState` field and its usages. Keep `_previousMouseState` for mouse input.

**Step 8: Verify it compiles**

Run: `dotnet build games/Solocaster/Solocaster.csproj`
Expected: Build succeeded

---

## Task 7: Update PlayerHandsRenderer with Visibility Transitions

**Files:**
- Modify: `games/Solocaster/Components/PlayerHandsRenderer.cs`

**Step 1: Add new fields for visibility (after existing fields, around line 40)**

```csharp
private float _visibilityOffset = 1f; // 0 = visible, 1 = hidden below screen
private const float HandTransitionSpeed = 6.67f; // 1/0.15 for 0.15s transition
private const float RunningBobAmplitudeMultiplier = 1.8f;
private const float RunningBobFrequencyMultiplier = 1.3f;
private const float ExhaustedShakeAmplitude = 3f;
private const float ExhaustedShakeFrequency = 15f;
```

**Step 2: Add helper property to check visibility state**

```csharp
private bool ShouldShowHands =>
    _playerBrain.State == PlayerState.Combat ||
    _playerBrain.State == PlayerState.Running ||
    _playerBrain.State == PlayerState.Exhausted;
```

**Step 3: Update UpdateCore to handle visibility transition and enhanced bob**

At the start of UpdateCore, add visibility lerp:
```csharp
float targetVisibility = ShouldShowHands ? 0f : 1f;
_visibilityOffset = MathHelper.Lerp(_visibilityOffset, targetVisibility, HandTransitionSpeed * deltaTime);
```

Update amplitude calculation to account for running:
```csharp
float amplitude = moveSpeed > 0.001f
    ? BobAmplitude
    : IdleBobAmplitude;

// Enhanced bob when running
if (_playerBrain.State == PlayerState.Running)
{
    amplitude *= RunningBobAmplitudeMultiplier;
}
```

Update bob frequency when running (in the bobSpeed calculation):
```csharp
float frequencyMultiplier = _playerBrain.State == PlayerState.Running ? RunningBobFrequencyMultiplier : 1f;
float bobSpeed = moveSpeed > 0.001f
    ? moveSpeed * BobSpeedMultiplier * BobFrequency * frequencyMultiplier
    : IdleBobFrequency;
```

**Step 4: Add exhausted shake calculation at end of UpdateCore**

```csharp
// Exhausted tremor effect
float exhaustedShakeX = 0f;
float exhaustedShakeY = 0f;
if (_playerBrain.State == PlayerState.Exhausted)
{
    float shakeTime = (float)gameTime.TotalGameTime.TotalSeconds * ExhaustedShakeFrequency;
    exhaustedShakeX = MathF.Sin(shakeTime * 7.3f) * ExhaustedShakeAmplitude * Scale;
    exhaustedShakeY = MathF.Sin(shakeTime * 5.7f) * ExhaustedShakeAmplitude * Scale;
}
```

Store these as fields to use in Render:
```csharp
private float _exhaustedShakeX;
private float _exhaustedShakeY;
```

**Step 5: Update Render to apply visibility offset**

In Render, calculate the visibility offset in pixels:
```csharp
int visibilityOffsetPixels = (int)(_visibilityOffset * viewport.Height * 0.4f);
```

Add to Y position calculations for both hands:
```csharp
int y = viewport.Height - scaledHeight + (int)_rightBobOffset + scaledHeight / 8 + rightArmedOffset + visibilityOffsetPixels + (int)_exhaustedShakeY;
int x = ... + (int)_exhaustedShakeX;
```

**Step 6: Verify it compiles**

Run: `dotnet build games/Solocaster/Solocaster.csproj`
Expected: Build succeeded

---

## Task 8: Wire Up InputBindings in PlayScene

**Files:**
- Modify: `games/Solocaster/Scenes/PlayScene.cs`

**Step 1: Verify InputBindings is created in PlayerBrain.InitCore**

The InputBindings is already created in PlayerBrain.InitCore, so no changes needed in PlayScene.

**Step 2: Verify the full system works**

Run: `dotnet run --project games/Solocaster/Solocaster.csproj`

Expected behaviors:
- R key toggles combat mode (hands raise/lower)
- Shift+W runs (faster movement, stamina drains)
- Stamina bar appears and depletes while running
- When stamina hits 0, player becomes exhausted (slow, hands shake)
- Stamina regenerates when not running and not in active combat

---

## Task 9: Final Testing Checklist

**Manual verification:**

1. [ ] Press R - hands should slide up (combat mode)
2. [ ] Press R again - hands should slide down (exploring mode)
3. [ ] Hold Shift+W - should run faster, stamina drains
4. [ ] Release Shift - should return to walk speed, stamina regens after delay
5. [ ] Drain stamina to 0 - should enter exhausted state (slow, hands shake)
6. [ ] Wait 2 seconds - should exit exhausted state
7. [ ] Verify stamina bar pulses when exhausted
8. [ ] Edit keybindings.json, change "run" to "RightShift" - verify it works

---

## Summary: Files Changed

| File | Action |
|------|--------|
| `Components/PlayerState.cs` | Create |
| `Input/InputBindings.cs` | Create |
| `data/settings/keybindings.json` | Create |
| `Components/StatsComponent.cs` | Modify - add stamina |
| `UI/UITheme.cs` | Modify - add stamina colors |
| `UI/PlayerStatusPanel.cs` | Modify - add stamina bar |
| `Components/PlayerBrain.cs` | Modify - state machine, InputBindings |
| `Components/PlayerHandsRenderer.cs` | Modify - visibility transitions |

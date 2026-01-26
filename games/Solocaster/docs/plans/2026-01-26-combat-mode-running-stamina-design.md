# Combat Mode, Running & Stamina System Design

## Overview

This design covers three interconnected features:
1. **Combat mode** - Manual toggle that raises/lowers player hands
2. **Running** - Hold-to-run movement with stamina consumption
3. **Stamina system** - New resource that governs running and regenerates based on Agility

Additionally, introduces a **key bindings system** for configurable controls.

---

## Player State System

### PlayerState Enum

```csharp
public enum PlayerState
{
    Exploring,  // Default - hands lowered, normal walk speed
    Combat,     // Hands raised, ready for action
    Running,    // Fast movement, hands visible with exaggerated bob, draining stamina
    Exhausted   // Recovery after stamina depletion, reduced speed, can't run
}
```

### State Transitions

| From | To | Trigger |
|------|----|---------|
| Exploring | Combat | Press R key |
| Combat | Exploring | Press R key |
| Exploring/Combat | Running | Hold Shift while moving forward |
| Running | Exhausted | Stamina reaches zero |
| Exhausted | Exploring/Combat | After ~2 seconds recovery |

The state lives in `PlayerBrain`. `PlayerHandsRenderer` reads the state to determine visibility.

---

## Stamina System

### New Properties in StatsComponent

| Property | Derivation | Description |
|----------|------------|-------------|
| `MaxStamina` | `50 + Vitality * 5` | Maximum stamina pool |
| `CurrentStamina` | Starts at max | Current stamina value |
| `StaminaRegenRate` | `5 + Agility * 0.5` per second | Regeneration speed |
| `IsExhausted` | Flag | Set when stamina hits zero |

### Stamina Drain

- **Drain rate**: ~15 stamina/second while running (configurable)
- **Condition**: Only drains while actually moving (holding Shift while stationary doesn't drain)

### Exhausted State

- **Trigger**: `CurrentStamina` reaches 0
- **Duration**: ~2 seconds (configurable `ExhaustedDuration`)
- **Effect**: Movement speed reduced to 60%, cannot run
- **Recovery**: After duration, `IsExhausted` clears but stamina remains low

### Regeneration Rules

Stamina regenerates when ALL of these conditions are true:
- Not currently running
- Not performing active combat actions (hitting, parrying, dodging)
- Not in exhausted state

**Implementation:**
- Track `LastCombatActionTime` timestamp
- Regen pauses for ~1-2 seconds after any combat action
- Regen delay of ~0.5 seconds after stopping a run before regen starts
- Regenerates at `StaminaRegenRate` per second

---

## Hands Visibility & Transitions

### Visibility Logic

```
Hands visible when: InCombat || IsRunning
Hands lowered when: !InCombat && !IsRunning && !IsExhausted
```

### Transition System

- New `_verticalVisibilityOffset` field (0 = fully visible, 1 = fully lowered)
- Lerp toward target offset for ~0.15s transition (quick snap feel)
- Offset applied to Y position in `Render()` to slide hands off-screen

### Running Bob Enhancement

When running, amplify hand movement:
- `BobAmplitude *= 1.8f`
- `BobFrequency *= 1.3f`

### Exhausted Visual Feedback

- Slight hand shake/tremor effect during exhausted state
- Optional: subtle screen vignette (future polish)

---

## Key Bindings System

### Configuration File

**Location**: `data/settings/keybindings.json`

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

### InputBindings Class

```csharp
public class InputBindings
{
    // Loads bindings from JSON at startup
    // Falls back to defaults if file missing or key invalid

    public bool IsActionDown(string action);    // Key currently held
    public bool IsActionPressed(string action); // Key just pressed this frame
}
```

`PlayerBrain` uses `InputBindings` instead of hardcoded `Keys.*` checks.

---

## UI Changes

### PlayerStatusPanel Updates

Add third progress bar for stamina:
- **Position**: Below Health and Mana bars
- **Color**: Yellow/gold (distinct from red health, blue mana)
- **Style**: Same as existing bars using `ProgressBarWidget`

### Visual States

| State | Appearance |
|-------|------------|
| Normal | Solid yellow fill |
| Exhausted | Pulsing/flashing effect |
| Low (<25%) | Optional subtle warning flash |

---

## Files to Create/Modify

| File | Action | Description |
|------|--------|-------------|
| `Components/PlayerBrain.cs` | Modify | Add state machine, running logic, use InputBindings |
| `Components/PlayerHandsRenderer.cs` | Modify | Add visibility transitions, running bob enhancement |
| `Components/StatsComponent.cs` | Modify | Add Stamina properties, regen logic |
| `UI/PlayerStatusPanel.cs` | Modify | Add stamina bar |
| `Input/InputBindings.cs` | Create | Key binding loader class |
| `data/settings/keybindings.json` | Create | Default key bindings |

---

## Constants & Configuration

```csharp
// Stamina
const float StaminaDrainRate = 15f;           // per second while running
const float StaminaRegenDelay = 0.5f;         // seconds after stopping run
const float CombatActionRegenPause = 1.5f;    // seconds after combat action

// Exhausted state
const float ExhaustedDuration = 2f;           // seconds
const float ExhaustedSpeedMultiplier = 0.6f;  // 60% normal speed

// Hand transitions
const float HandTransitionSpeed = 0.15f;      // seconds for full transition

// Running
const float RunningSpeedMultiplier = 1.8f;    // compared to walk
const float RunningBobAmplitudeMultiplier = 1.8f;
const float RunningBobFrequencyMultiplier = 1.3f;
```

---

## Future Considerations

- In-game key rebinding UI
- Stamina-affecting items/potions
- Stamina cost for combat actions (attacks, dodges)
- Visual/audio feedback for low stamina warning

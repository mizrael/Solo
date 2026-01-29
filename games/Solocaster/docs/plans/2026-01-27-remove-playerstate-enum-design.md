# Design: Remove PlayerState Enum

## Problem

Currently we have redundant state tracking:
- `PlayerState` enum (Exploring, Combat, Running, Exhausted)
- Actual state classes (PlayerExploringState, PlayerCombatState, etc.)
- `GetCurrentPlayerState()` method that maps state class → enum

External code queries the enum to determine behavior:
```csharp
if (_playerBrain.State == PlayerState.Combat || _playerBrain.State == PlayerState.Running)
```

This creates:
1. Duplication between enum values and state classes
2. Boilerplate mapping code
3. Coupling to state identity rather than behavior

## Solution

Replace identity-based checks with capability-based properties on a base state class.

### PlayerStateBase Abstract Class

```csharp
public abstract class PlayerStateBase : Solo.AI.State
{
    public virtual bool ShowsHands => false;
    public virtual float SpeedMultiplier => 1.0f;
    public virtual float BobSpeed => 1.5f;

    protected PlayerStateBase(GameObject owner) : base(owner) { }
}
```

### State Property Overrides

| State | ShowsHands | SpeedMultiplier | BobSpeed |
|-------|------------|-----------------|----------|
| Exploring | false (default) | 1.0 (default) | 1.5 (default) |
| Combat | **true** | 1.0 (default) | 1.5 (default) |
| Running | **true** | **1.8** | **3.0** |
| Exhausted | false (default) | **0.6** | 1.5 (default) |

### Type Check for Running Metrics

One behavior is tied to Running state identity (not a capability):
- Metrics recording (RecordRunning vs RecordWalking)

For this, use a direct type check:
```csharp
if (_stateMachine.CurrentState is PlayerRunningState)
```

### Backward Movement While Running

Instead of checking state in `HandleMovement`, add a state transition:
- Pressing backward while running → exit to Exploring state

This keeps movement logic clean with no type checks.

## Changes Required

### Files to Modify

| File | Change |
|------|--------|
| `AI/Player/PlayerStateBase.cs` | Create - abstract base with virtual properties |
| `AI/Player/PlayerExploringState.cs` | Inherit from PlayerStateBase |
| `AI/Player/PlayerCombatState.cs` | Inherit from PlayerStateBase, override ShowsHands |
| `AI/Player/PlayerRunningState.cs` | Inherit from PlayerStateBase, override all three |
| `AI/Player/PlayerExhaustedState.cs` | Inherit from PlayerStateBase, override SpeedMultiplier |
| `AI/Player/PlayerStateContext.cs` | Change PreviousStateBeforeRun to StateBeforeRun (state reference), remove SpeedMultiplier |
| `Components/PlayerBrain.cs` | Remove State property, expose CurrentState, update usages |
| `Components/PlayerHandsRenderer.cs` | Use CurrentState.ShowsHands and CurrentState.BobSpeed |
| `Components/PlayerState.cs` | Delete |

### Before/After Examples

**PlayerHandsRenderer visibility:**
```csharp
// Before
_playerBrain.State == PlayerState.Combat || _playerBrain.State == PlayerState.Running

// After
_playerBrain.CurrentState.ShowsHands
```

**PlayerHandsRenderer bob speed:**
```csharp
// Before
_playerBrain.State == PlayerState.Running ? BobSpeedRunning : BobSpeedNormal

// After
_playerBrain.CurrentState.BobSpeed
```

**PlayerBrain backward movement:**
```csharp
// Before
else if (State != PlayerState.Running && InputBindings.IsActionDown(InputActions.MoveBackward))
    moveAmount = -moveSpeed;

// After (no state check - state machine handles it via transition)
else if (InputBindings.IsActionDown(InputActions.MoveBackward))
    moveAmount = -moveSpeed;
```

**New transition in SetupTransitions:**
```csharp
// Running -> Exploring (pressed backward)
_stateMachine.AddTransition(_runningState, _exploringState,
    _ => InputBindings.IsActionDown(InputActions.MoveBackward));
```

**PlayerBrain metrics:**
```csharp
// Before
if (State == PlayerState.Running)

// After
if (_stateMachine.CurrentState is PlayerRunningState)
```

## Benefits

1. **No duplication** - state classes are the single source of truth
2. **No mapping code** - no GetCurrentPlayerState() needed
3. **Behavior-driven** - external code asks "what can you do?" not "who are you?"
4. **Extensible** - new states just override relevant properties
5. **Lean** - only three properties, type checks for edge cases

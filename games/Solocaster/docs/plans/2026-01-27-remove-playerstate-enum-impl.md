# Implementation Plan: Remove PlayerState Enum

## Overview

Replace the `PlayerState` enum with capability-based properties on a `PlayerStateBase` abstract class.

---

## Task 1: Create PlayerStateBase Abstract Class

**Location:** `games/Solocaster/AI/Player/PlayerStateBase.cs`

**Action:** Create new file

```csharp
using Solo;
using Solo.AI;

namespace Solocaster.AI.Player;

public abstract class PlayerStateBase : State
{
    public virtual bool ShowsHands => false;
    public virtual float SpeedMultiplier => 1.0f;
    public virtual float BobSpeed => 1.5f;

    protected PlayerStateBase(GameObject owner) : base(owner) { }
}
```

**Verify:** File compiles

---

## Task 2: Update PlayerExploringState

**Location:** `games/Solocaster/AI/Player/PlayerExploringState.cs`

**Changes:**
- Change base class from `Solo.AI.State` to `PlayerStateBase`
- Remove `_ctx.SpeedMultiplier = 1.0f` from OnEnter (default handles it)

**After:**
```csharp
using Microsoft.Xna.Framework;
using Solo;

namespace Solocaster.AI.Player;

public class PlayerExploringState : PlayerStateBase
{
    private readonly PlayerStateContext _ctx;

    public PlayerExploringState(GameObject owner, PlayerStateContext context) : base(owner)
    {
        _ctx = context;
    }

    protected override void OnEnter()
    {
        _ctx.LeftHandRaiseAmount = 0f;
        _ctx.RightHandRaiseAmount = 0f;
    }

    protected override void OnExecute(GameTime gameTime)
    {
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _ctx.Stats.UpdateStamina(deltaTime, false);
    }
}
```

**Verify:** File compiles

---

## Task 3: Update PlayerCombatState

**Location:** `games/Solocaster/AI/Player/PlayerCombatState.cs`

**Changes:**
- Change base class from `Solo.AI.State` to `PlayerStateBase`
- Add `public override bool ShowsHands => true;`
- Remove `_ctx.SpeedMultiplier = 1.0f` from OnEnter

**Verify:** File compiles

---

## Task 4: Update PlayerRunningState

**Location:** `games/Solocaster/AI/Player/PlayerRunningState.cs`

**Changes:**
- Change base class from `Solo.AI.State` to `PlayerStateBase`
- Add property overrides:
  ```csharp
  public override bool ShowsHands => true;
  public override float SpeedMultiplier => 1.8f;
  public override float BobSpeed => 3.0f;
  ```
- Remove the SpeedMultiplier constant and `_ctx.SpeedMultiplier` assignment from OnEnter

**After:**
```csharp
using Microsoft.Xna.Framework;
using Solo;

namespace Solocaster.AI.Player;

public class PlayerRunningState : PlayerStateBase
{
    private readonly PlayerStateContext _ctx;

    public override bool ShowsHands => true;
    public override float SpeedMultiplier => 1.8f;
    public override float BobSpeed => 3.0f;

    public PlayerRunningState(GameObject owner, PlayerStateContext context) : base(owner)
    {
        _ctx = context;
    }

    protected override void OnEnter()
    {
        _ctx.LeftHandRaiseAmount = 0f;
        _ctx.RightHandRaiseAmount = 0f;
    }

    protected override void OnExecute(GameTime gameTime)
    {
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _ctx.Stats.DrainStamina(deltaTime);
    }
}
```

**Verify:** File compiles

---

## Task 5: Update PlayerExhaustedState

**Location:** `games/Solocaster/AI/Player/PlayerExhaustedState.cs`

**Changes:**
- Change base class from `Solo.AI.State` to `PlayerStateBase`
- Add `public override float SpeedMultiplier => 0.6f;`
- Remove the constant and `_ctx.SpeedMultiplier` assignment from OnEnter

**After:**
```csharp
using Microsoft.Xna.Framework;
using Solo;

namespace Solocaster.AI.Player;

public class PlayerExhaustedState : PlayerStateBase
{
    private readonly PlayerStateContext _ctx;

    public override float SpeedMultiplier => 0.6f;

    public PlayerExhaustedState(GameObject owner, PlayerStateContext context) : base(owner)
    {
        _ctx = context;
    }

    protected override void OnEnter()
    {
        _ctx.LeftHandRaiseAmount = 0f;
        _ctx.RightHandRaiseAmount = 0f;
    }

    protected override void OnExecute(GameTime gameTime)
    {
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _ctx.Stats.UpdateStamina(deltaTime, false);
    }
}
```

**Verify:** File compiles

---

## Task 6: Update PlayerStateContext

**Location:** `games/Solocaster/AI/Player/PlayerStateContext.cs`

**Changes:**
- Change `PreviousStateBeforeRun` from `PlayerState` enum to `PlayerStateBase?` reference:
  ```csharp
  // Before
  public PlayerState PreviousStateBeforeRun { get; set; } = PlayerState.Exploring;

  // After
  public PlayerStateBase? StateBeforeRun { get; set; }
  ```
- Remove `SpeedMultiplier` property (now on state classes)

**Verify:** File compiles (will have errors until PlayerBrain is updated)

---

## Task 7: Update PlayerBrain

**Location:** `games/Solocaster/Components/PlayerBrain.cs`

**Changes:**

1. Remove the `State` property and `GetCurrentPlayerState()` method

2. Add `CurrentState` property:
   ```csharp
   public PlayerStateBase? CurrentState => _stateMachine?.CurrentState as PlayerStateBase;
   ```

3. Add new transition for backward key while running:
   ```csharp
   // Running -> Exploring (pressed backward)
   _stateMachine.AddTransition(_runningState, _exploringState,
       _ => InputBindings.IsActionDown(InputActions.MoveBackward));
   ```

4. Update transition conditions to use state references instead of `PreviousStateBeforeRun`:
   ```csharp
   // Running -> previous state (stopped running or stopped forward)
   _stateMachine.AddTransition(_runningState, _exploringState,
       _ => (!InputBindings.IsActionDown(InputActions.Run) ||
             !InputBindings.IsActionDown(InputActions.MoveForward)) &&
            _context.StateBeforeRun == _exploringState);
   ```

5. Update `HandleMovement`:
   - Change `_context.SpeedMultiplier` to `CurrentState?.SpeedMultiplier ?? 1.0f`
   - Remove the `State != PlayerState.Running` check - just check backward input directly
   - Change `State == PlayerState.Running` to `_stateMachine.CurrentState is PlayerRunningState` for metrics

6. Simplify backward movement (no state check needed):
   ```csharp
   // Before
   else if (State != PlayerState.Running && InputBindings.IsActionDown(InputActions.MoveBackward))
       moveAmount = -moveSpeed;

   // After
   else if (InputBindings.IsActionDown(InputActions.MoveBackward))
       moveAmount = -moveSpeed;
   ```

**Verify:** File compiles

---

## Task 8: Update PlayerHandsRenderer

**Location:** `games/Solocaster/Components/PlayerHandsRenderer.cs`

**Changes:**

1. Update `ShouldShowHands`:
   ```csharp
   // Before
   private bool ShouldShowHands =>
       _playerBrain.State == PlayerState.Combat ||
       _playerBrain.State == PlayerState.Running;

   // After
   private bool ShouldShowHands => _playerBrain.CurrentState?.ShowsHands ?? false;
   ```

2. Update bob speed in `UpdateCore`:
   ```csharp
   // Before
   float bobSpeed = _playerBrain.State == PlayerState.Running ? BobSpeedRunning : BobSpeedNormal;

   // After
   float bobSpeed = _playerBrain.CurrentState?.BobSpeed ?? BobSpeedNormal;
   ```

3. Remove `BobSpeedRunning` constant (no longer needed)

**Verify:** File compiles

---

## Task 9: Delete PlayerState Enum

**Location:** `games/Solocaster/Components/PlayerState.cs`

**Action:** Delete file

**Verify:** Solution builds with no errors

---

## Task 10: Final Verification

**Commands:**
```bash
dotnet build games/Solocaster/Solocaster.csproj
dotnet run --project games/Solocaster/Solocaster.csproj
```

**Manual Testing:**
- [ ] Exploring state: hands hidden, normal speed
- [ ] Combat state (R key): hands visible, normal speed
- [ ] Running state (Shift+W): hands visible, fast speed, fast bob, no backward movement
- [ ] Exhausted state: hands hidden, slow speed
- [ ] Transitions work correctly between all states

---

## Files Summary

| File | Action |
|------|--------|
| `AI/Player/PlayerStateBase.cs` | Create |
| `AI/Player/PlayerExploringState.cs` | Modify |
| `AI/Player/PlayerCombatState.cs` | Modify |
| `AI/Player/PlayerRunningState.cs` | Modify |
| `AI/Player/PlayerExhaustedState.cs` | Modify |
| `AI/Player/PlayerStateContext.cs` | Modify |
| `Components/PlayerBrain.cs` | Modify |
| `Components/PlayerHandsRenderer.cs` | Modify |
| `Components/PlayerState.cs` | Delete |

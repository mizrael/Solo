# Weapon Attack & Shield Parry Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Implement weapon attacks and shield parry with visual hand-raising feedback, controlled by mouse buttons in combat mode.

**Architecture:** Refactor PlayerBrain to use Solo.AI.StateMachine with four player states. Combat state tracks hand action states (Idle/Raising/Held/Lowering) with timers. Renderer reads raise amounts from PlayerBrain.

**Tech Stack:** C# / MonoGame / Solo engine (StateMachine from Solo.AI)

---

## Task 1: Create HandActionState Enum

**Files:**
- Create: `games/Solocaster/Components/HandActionState.cs`

**Implementation:**

```csharp
namespace Solocaster.Components;

public enum HandActionState
{
    Idle,
    Raising,
    Held,
    Lowering
}
```

**Verify:** Build succeeds with `dotnet build games/Solocaster/Solocaster.csproj`

---

## Task 2: Create PlayerStateContext Class

**Files:**
- Create: `games/Solocaster/Components/PlayerStates/PlayerStateContext.cs`

**Purpose:** Shared context passed to all player states, providing access to components and services.

**Implementation:**

```csharp
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Solo.Components;
using Solocaster.Entities;
using Solocaster.Inventory;
using Solocaster.UI;
using System;

namespace Solocaster.Components.PlayerStates;

public class PlayerStateContext
{
    public required TransformComponent Transform { get; init; }
    public required InventoryComponent Inventory { get; init; }
    public required StatsComponent Stats { get; init; }
    public required Map Map { get; init; }

    public CharacterPanel? CharacterPanel { get; set; }
    public MetricsPanel? MetricsPanel { get; set; }
    public Raycaster? Raycaster { get; set; }
    public GameObject? MiniMapEntity { get; set; }
    public GameObject? DebugUIEntity { get; set; }

    public Vector2 Plane { get; set; } = new(0, 0.45f);
    public MouseState PreviousMouseState { get; set; }
    public float CurrentMoveSpeed { get; set; }

    public PlayerState PreviousStateBeforeRun { get; set; } = PlayerState.Exploring;

    // Hand action state (managed by CombatState, read by renderer)
    public float LeftHandRaiseAmount { get; set; }
    public float RightHandRaiseAmount { get; set; }

    public void RotatePlane(float angle)
    {
        var cos = MathF.Cos(angle);
        var sin = MathF.Sin(angle);
        var oldPlane = Plane;
        Plane = new Vector2(
            oldPlane.X * cos - oldPlane.Y * sin,
            oldPlane.X * sin + oldPlane.Y * cos
        );
    }
}
```

**Verify:** Build succeeds

---

## Task 3: Create PlayerExploringState

**Files:**
- Create: `games/Solocaster/Components/PlayerStates/PlayerExploringState.cs`

**Purpose:** Default state handling movement, rotation, interaction, panel toggles. No combat actions.

**Implementation:**

```csharp
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Solo;
using Solo.AI;
using Solocaster.Input;
using Solocaster.Inventory;
using System;

namespace Solocaster.Components.PlayerStates;

public record PlayerExploringState : State
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
        float ms = (float)gameTime.ElapsedGameTime.TotalMilliseconds;
        float moveSpeed = ms * 0.005f;
        float rotSpeed = ms * 0.005f;

        var mouseState = Mouse.GetState();

        HandlePanelToggles();
        HandlePickup(mouseState);
        HandleInteract();

        _ctx.PreviousMouseState = mouseState;

        // Movement
        float moveAmount = 0f;
        if (InputBindings.IsActionDown(InputActions.MoveForward))
            moveAmount = moveSpeed;
        else if (InputBindings.IsActionDown(InputActions.MoveBackward))
            moveAmount = -moveSpeed;

        // Stamina regen while exploring
        _ctx.Stats.UpdateStamina((float)gameTime.ElapsedGameTime.TotalSeconds, false);

        _ctx.CurrentMoveSpeed = MathF.Abs(moveAmount);
        ApplyMovement(moveAmount);
        HandleRotation(rotSpeed);
    }

    private void HandlePanelToggles()
    {
        if (InputBindings.IsActionPressed(InputActions.ToggleCharacterPanel))
            _ctx.CharacterPanel?.Toggle();

        if (InputBindings.IsActionPressed(InputActions.ToggleMinimap) && _ctx.MiniMapEntity != null)
            _ctx.MiniMapEntity.Enabled = !_ctx.MiniMapEntity.Enabled;

        if (InputBindings.IsActionPressed(InputActions.ToggleDebug) && _ctx.DebugUIEntity != null)
            _ctx.DebugUIEntity.Enabled = !_ctx.DebugUIEntity.Enabled;

        if (InputBindings.IsActionPressed(InputActions.ToggleMetrics))
            _ctx.MetricsPanel?.Toggle();
    }

    private void HandlePickup(MouseState mouseState)
    {
        if (mouseState.LeftButton == ButtonState.Pressed &&
            _ctx.PreviousMouseState.LeftButton == ButtonState.Released)
        {
            TryPickupClickedItem();
        }
    }

    private bool TryPickupClickedItem()
    {
        if (_ctx.Raycaster == null)
            return false;

        var hoveredEntity = _ctx.Raycaster.HoveredEntity;
        if (hoveredEntity == null)
            return false;

        if (!hoveredEntity.Components.TryGet<PickupableComponent>(out var pickupable))
            return false;

        var playerPos = _ctx.Transform.World.Position;
        if (!pickupable.IsInRange(playerPos))
            return false;

        var itemInstance = pickupable.CreateItemInstance();
        var result = _ctx.Inventory.AddItem(itemInstance);

        if (result == AddItemResult.Success)
        {
            pickupable.OnPickedUp();
            return true;
        }

        return false;
    }

    private void HandleInteract()
    {
        if (InputBindings.IsActionPressed(InputActions.Interact))
            TryOpenDoor();
    }

    private bool TryOpenDoor()
    {
        float checkDistance = 1.5f;

        for (float dist = 0.1f; dist <= checkDistance; dist += 0.1f)
        {
            int checkX = (int)(_ctx.Transform.World.Position.X + _ctx.Transform.World.Direction.X * dist);
            int checkY = (int)(_ctx.Transform.World.Position.Y + _ctx.Transform.World.Direction.Y * dist);

            var door = _ctx.Map.GetDoor(checkX, checkY);
            if (door is not null)
            {
                door.StartOpening();
                return true;
            }
        }

        return false;
    }

    private void ApplyMovement(float moveAmount)
    {
        if (moveAmount == 0)
            return;

        var previousPos = _ctx.Transform.Local.Position;
        var moveStep = _ctx.Transform.World.Direction * moveAmount;

        if (!_ctx.Map.IsBlocked((int)(_ctx.Transform.World.Position.X + moveStep.X), (int)_ctx.Transform.World.Position.Y))
            _ctx.Transform.Local.Position.X += moveStep.X;

        if (!_ctx.Map.IsBlocked((int)_ctx.Transform.World.Position.X, (int)(_ctx.Transform.World.Position.Y + moveStep.Y)))
            _ctx.Transform.Local.Position.Y += moveStep.Y;

        var actualDistance = Vector2.Distance(previousPos, _ctx.Transform.Local.Position);
        if (actualDistance > 0)
            _ctx.Stats.Metrics.RecordWalking(actualDistance);
    }

    private void HandleRotation(float rotSpeed)
    {
        if (InputBindings.IsActionDown(InputActions.RotateLeft))
        {
            RotatePlayer(-rotSpeed);
        }
        else if (InputBindings.IsActionDown(InputActions.RotateRight))
        {
            RotatePlayer(rotSpeed);
        }
    }

    private void RotatePlayer(float angle)
    {
        var oldDirection = _ctx.Transform.Local.Direction;
        var cos = MathF.Cos(angle);
        var sin = MathF.Sin(angle);

        _ctx.Transform.Local.Direction = new Vector2(
            oldDirection.X * cos - oldDirection.Y * sin,
            oldDirection.X * sin + oldDirection.Y * cos
        );

        _ctx.RotatePlane(angle);
    }
}
```

**Verify:** Build succeeds

---

## Task 4: Create PlayerCombatState

**Files:**
- Create: `games/Solocaster/Components/PlayerStates/PlayerCombatState.cs`

**Purpose:** Combat mode with hand action tracking. Handles attacks (weapons) and parry (shields) via mouse buttons.

**Implementation:**

```csharp
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Solo;
using Solo.AI;
using Solocaster.Input;
using Solocaster.Inventory;
using System;

namespace Solocaster.Components.PlayerStates;

public record PlayerCombatState : State
{
    private const float RaisingDuration = 0.15f;
    private const float HeldDuration = 0.10f;
    private const float LoweringDuration = 0.25f;
    private const float BaseCooldown = 0.8f;
    private const float AgilityModifierRate = 0.02f;

    private readonly PlayerStateContext _ctx;

    private HandActionState _leftHandState = HandActionState.Idle;
    private HandActionState _rightHandState = HandActionState.Idle;
    private float _leftHandTimer;
    private float _rightHandTimer;
    private float _leftCooldown;
    private float _rightCooldown;

    public PlayerCombatState(GameObject owner, PlayerStateContext context) : base(owner)
    {
        _ctx = context;
    }

    protected override void OnEnter()
    {
        _leftHandState = HandActionState.Idle;
        _rightHandState = HandActionState.Idle;
        _leftHandTimer = 0f;
        _rightHandTimer = 0f;
        _leftCooldown = 0f;
        _rightCooldown = 0f;
    }

    protected override void OnExecute(GameTime gameTime)
    {
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        float ms = (float)gameTime.ElapsedGameTime.TotalMilliseconds;
        float moveSpeed = ms * 0.005f;
        float rotSpeed = ms * 0.005f;

        var mouseState = Mouse.GetState();

        HandlePanelToggles();

        // Update cooldowns
        _leftCooldown = MathF.Max(0, _leftCooldown - deltaTime);
        _rightCooldown = MathF.Max(0, _rightCooldown - deltaTime);

        // Handle hand actions
        HandleLeftHandAction(mouseState, deltaTime);
        HandleRightHandAction(mouseState, deltaTime);

        // Update raise amounts for renderer
        _ctx.LeftHandRaiseAmount = CalculateRaiseAmount(_leftHandState, _leftHandTimer);
        _ctx.RightHandRaiseAmount = CalculateRaiseAmount(_rightHandState, _rightHandTimer);

        _ctx.PreviousMouseState = mouseState;

        // Movement (same as exploring)
        float moveAmount = 0f;
        if (InputBindings.IsActionDown(InputActions.MoveForward))
            moveAmount = moveSpeed;
        else if (InputBindings.IsActionDown(InputActions.MoveBackward))
            moveAmount = -moveSpeed;

        // Stamina regen in combat
        _ctx.Stats.UpdateStamina(deltaTime, false);

        _ctx.CurrentMoveSpeed = MathF.Abs(moveAmount);
        ApplyMovement(moveAmount);
        HandleRotation(rotSpeed);
    }

    private void HandleLeftHandAction(MouseState mouseState, float deltaTime)
    {
        var leftItem = _ctx.Inventory.GetEquippedItem(EquipSlot.LeftHand);
        bool isShield = IsShield(leftItem);
        bool isWeapon = IsWeapon(leftItem);
        bool mouseHeld = mouseState.LeftButton == ButtonState.Pressed;
        bool mousePressed = mouseHeld && _ctx.PreviousMouseState.LeftButton == ButtonState.Released;

        if (isShield)
        {
            HandleParry(ref _leftHandState, ref _leftHandTimer, mouseHeld, deltaTime);
        }
        else if (isWeapon)
        {
            HandleAttack(ref _leftHandState, ref _leftHandTimer, ref _leftCooldown,
                         _rightHandState, mousePressed, deltaTime, leftItem!);
        }
        else
        {
            // Empty hand - reset to idle
            UpdateIdleState(ref _leftHandState, ref _leftHandTimer, deltaTime);
        }
    }

    private void HandleRightHandAction(MouseState mouseState, float deltaTime)
    {
        var rightItem = _ctx.Inventory.GetEquippedItem(EquipSlot.RightHand);
        bool isShield = IsShield(rightItem);
        bool isWeapon = IsWeapon(rightItem);
        bool mouseHeld = mouseState.RightButton == ButtonState.Pressed;
        bool mousePressed = mouseHeld && _ctx.PreviousMouseState.RightButton == ButtonState.Released;

        if (isShield)
        {
            HandleParry(ref _rightHandState, ref _rightHandTimer, mouseHeld, deltaTime);
        }
        else if (isWeapon)
        {
            HandleAttack(ref _rightHandState, ref _rightHandTimer, ref _rightCooldown,
                         _leftHandState, mousePressed, deltaTime, rightItem!);
        }
        else
        {
            UpdateIdleState(ref _rightHandState, ref _rightHandTimer, deltaTime);
        }
    }

    private void HandleParry(ref HandActionState state, ref float timer, bool mouseHeld, float deltaTime)
    {
        if (mouseHeld)
        {
            switch (state)
            {
                case HandActionState.Idle:
                    state = HandActionState.Raising;
                    timer = 0f;
                    break;
                case HandActionState.Raising:
                    timer += deltaTime;
                    if (timer >= RaisingDuration)
                    {
                        state = HandActionState.Held;
                        timer = 0f;
                    }
                    break;
                case HandActionState.Held:
                    // Stay held while button pressed
                    break;
                case HandActionState.Lowering:
                    // If pressed again while lowering, go back to raising
                    state = HandActionState.Raising;
                    timer = 0f;
                    break;
            }
        }
        else
        {
            // Button released
            if (state == HandActionState.Held || state == HandActionState.Raising)
            {
                state = HandActionState.Lowering;
                timer = 0f;
            }
            else if (state == HandActionState.Lowering)
            {
                timer += deltaTime;
                if (timer >= LoweringDuration)
                {
                    state = HandActionState.Idle;
                    timer = 0f;
                }
            }
        }
    }

    private void HandleAttack(ref HandActionState state, ref float timer, ref float cooldown,
                              HandActionState otherHandState, bool mousePressed, float deltaTime,
                              ItemInstance weapon)
    {
        // Progress existing attack animation
        switch (state)
        {
            case HandActionState.Raising:
                timer += deltaTime;
                if (timer >= RaisingDuration)
                {
                    state = HandActionState.Held;
                    timer = 0f;
                }
                break;
            case HandActionState.Held:
                timer += deltaTime;
                if (timer >= HeldDuration)
                {
                    state = HandActionState.Lowering;
                    timer = 0f;
                    // Attack lands here (future: hit detection)
                    _ctx.Stats.OnCombatAction();
                }
                break;
            case HandActionState.Lowering:
                timer += deltaTime;
                if (timer >= LoweringDuration)
                {
                    state = HandActionState.Idle;
                    timer = 0f;
                }
                break;
        }

        // Start new attack if conditions met
        if (mousePressed && state == HandActionState.Idle && cooldown <= 0)
        {
            // Alternating rule: can't attack if other hand is Raising or Held
            if (otherHandState != HandActionState.Raising && otherHandState != HandActionState.Held)
            {
                state = HandActionState.Raising;
                timer = 0f;
                cooldown = CalculateCooldown(weapon.Template);
            }
        }
    }

    private void UpdateIdleState(ref HandActionState state, ref float timer, float deltaTime)
    {
        if (state == HandActionState.Lowering)
        {
            timer += deltaTime;
            if (timer >= LoweringDuration)
            {
                state = HandActionState.Idle;
                timer = 0f;
            }
        }
        else if (state != HandActionState.Idle)
        {
            state = HandActionState.Lowering;
            timer = 0f;
        }
    }

    private float CalculateCooldown(ItemTemplate weapon)
    {
        float attackSpeed = weapon.AttackSpeed;
        float agility = _ctx.Stats.GetTotalStat(StatType.Agility);
        float weaponModifier = 1f / attackSpeed;
        float agilityModifier = 1f / (1f + agility * AgilityModifierRate);
        return BaseCooldown * weaponModifier * agilityModifier;
    }

    private float CalculateRaiseAmount(HandActionState state, float timer)
    {
        return state switch
        {
            HandActionState.Idle => 0f,
            HandActionState.Raising => MathF.Min(1f, timer / RaisingDuration),
            HandActionState.Held => 1f,
            HandActionState.Lowering => MathF.Max(0f, 1f - (timer / LoweringDuration)),
            _ => 0f
        };
    }

    private static bool IsShield(ItemInstance? item)
    {
        if (item == null) return false;
        var name = item.Template.Name.ToLowerInvariant();
        var id = item.TemplateId.ToLowerInvariant();
        return name.Contains("shield") || name.Contains("buckler") ||
               id.Contains("shield") || id.Contains("buckler");
    }

    private static bool IsWeapon(ItemInstance? item)
    {
        if (item == null) return false;
        return item.Template.ItemType == ItemType.Weapon;
    }

    private void HandlePanelToggles()
    {
        if (InputBindings.IsActionPressed(InputActions.ToggleCharacterPanel))
            _ctx.CharacterPanel?.Toggle();

        if (InputBindings.IsActionPressed(InputActions.ToggleMinimap) && _ctx.MiniMapEntity != null)
            _ctx.MiniMapEntity.Enabled = !_ctx.MiniMapEntity.Enabled;

        if (InputBindings.IsActionPressed(InputActions.ToggleDebug) && _ctx.DebugUIEntity != null)
            _ctx.DebugUIEntity.Enabled = !_ctx.DebugUIEntity.Enabled;

        if (InputBindings.IsActionPressed(InputActions.ToggleMetrics))
            _ctx.MetricsPanel?.Toggle();
    }

    private void ApplyMovement(float moveAmount)
    {
        if (moveAmount == 0)
            return;

        var previousPos = _ctx.Transform.Local.Position;
        var moveStep = _ctx.Transform.World.Direction * moveAmount;

        if (!_ctx.Map.IsBlocked((int)(_ctx.Transform.World.Position.X + moveStep.X), (int)_ctx.Transform.World.Position.Y))
            _ctx.Transform.Local.Position.X += moveStep.X;

        if (!_ctx.Map.IsBlocked((int)_ctx.Transform.World.Position.X, (int)(_ctx.Transform.World.Position.Y + moveStep.Y)))
            _ctx.Transform.Local.Position.Y += moveStep.Y;

        var actualDistance = Vector2.Distance(previousPos, _ctx.Transform.Local.Position);
        if (actualDistance > 0)
            _ctx.Stats.Metrics.RecordWalking(actualDistance);
    }

    private void HandleRotation(float rotSpeed)
    {
        if (InputBindings.IsActionDown(InputActions.RotateLeft))
            RotatePlayer(-rotSpeed);
        else if (InputBindings.IsActionDown(InputActions.RotateRight))
            RotatePlayer(rotSpeed);
    }

    private void RotatePlayer(float angle)
    {
        var oldDirection = _ctx.Transform.Local.Direction;
        var cos = MathF.Cos(angle);
        var sin = MathF.Sin(angle);

        _ctx.Transform.Local.Direction = new Vector2(
            oldDirection.X * cos - oldDirection.Y * sin,
            oldDirection.X * sin + oldDirection.Y * cos
        );

        _ctx.RotatePlane(angle);
    }
}
```

**Verify:** Build succeeds

---

## Task 5: Create PlayerRunningState

**Files:**
- Create: `games/Solocaster/Components/PlayerStates/PlayerRunningState.cs`

**Purpose:** Running state with stamina drain, forward movement only.

**Implementation:**

```csharp
using Microsoft.Xna.Framework;
using Solo;
using Solo.AI;
using Solocaster.Input;
using System;

namespace Solocaster.Components.PlayerStates;

public record PlayerRunningState : State
{
    private const float RunningSpeedMultiplier = 1.8f;

    private readonly PlayerStateContext _ctx;

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
        float ms = (float)gameTime.ElapsedGameTime.TotalMilliseconds;
        float moveSpeed = ms * 0.005f * RunningSpeedMultiplier;
        float rotSpeed = ms * 0.005f;

        // Drain stamina
        _ctx.Stats.DrainStamina(deltaTime);

        // Forward movement only while running
        float moveAmount = InputBindings.IsActionDown(InputActions.MoveForward) ? moveSpeed : 0f;

        _ctx.CurrentMoveSpeed = MathF.Abs(moveAmount);
        ApplyMovement(moveAmount);
        HandleRotation(rotSpeed);
    }

    private void ApplyMovement(float moveAmount)
    {
        if (moveAmount == 0)
            return;

        var previousPos = _ctx.Transform.Local.Position;
        var moveStep = _ctx.Transform.World.Direction * moveAmount;

        if (!_ctx.Map.IsBlocked((int)(_ctx.Transform.World.Position.X + moveStep.X), (int)_ctx.Transform.World.Position.Y))
            _ctx.Transform.Local.Position.X += moveStep.X;

        if (!_ctx.Map.IsBlocked((int)_ctx.Transform.World.Position.X, (int)(_ctx.Transform.World.Position.Y + moveStep.Y)))
            _ctx.Transform.Local.Position.Y += moveStep.Y;

        var actualDistance = Vector2.Distance(previousPos, _ctx.Transform.Local.Position);
        if (actualDistance > 0)
            _ctx.Stats.Metrics.RecordWalking(actualDistance);
    }

    private void HandleRotation(float rotSpeed)
    {
        if (InputBindings.IsActionDown(InputActions.RotateLeft))
            RotatePlayer(-rotSpeed);
        else if (InputBindings.IsActionDown(InputActions.RotateRight))
            RotatePlayer(rotSpeed);
    }

    private void RotatePlayer(float angle)
    {
        var oldDirection = _ctx.Transform.Local.Direction;
        var cos = MathF.Cos(angle);
        var sin = MathF.Sin(angle);

        _ctx.Transform.Local.Direction = new Vector2(
            oldDirection.X * cos - oldDirection.Y * sin,
            oldDirection.X * sin + oldDirection.Y * cos
        );

        _ctx.RotatePlane(angle);
    }
}
```

**Verify:** Build succeeds

---

## Task 6: Create PlayerExhaustedState

**Files:**
- Create: `games/Solocaster/Components/PlayerStates/PlayerExhaustedState.cs`

**Purpose:** Recovery state after stamina depletion with reduced speed.

**Implementation:**

```csharp
using Microsoft.Xna.Framework;
using Solo;
using Solo.AI;
using Solocaster.Input;
using System;

namespace Solocaster.Components.PlayerStates;

public record PlayerExhaustedState : State
{
    private const float ExhaustedSpeedMultiplier = 0.6f;

    private readonly PlayerStateContext _ctx;

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
        float ms = (float)gameTime.ElapsedGameTime.TotalMilliseconds;
        float moveSpeed = ms * 0.005f * ExhaustedSpeedMultiplier;
        float rotSpeed = ms * 0.005f;

        // Recover stamina
        _ctx.Stats.UpdateStamina(deltaTime, false);

        // Reduced movement
        float moveAmount = 0f;
        if (InputBindings.IsActionDown(InputActions.MoveForward))
            moveAmount = moveSpeed;
        else if (InputBindings.IsActionDown(InputActions.MoveBackward))
            moveAmount = -moveSpeed;

        _ctx.CurrentMoveSpeed = MathF.Abs(moveAmount);
        ApplyMovement(moveAmount);
        HandleRotation(rotSpeed);
    }

    private void ApplyMovement(float moveAmount)
    {
        if (moveAmount == 0)
            return;

        var previousPos = _ctx.Transform.Local.Position;
        var moveStep = _ctx.Transform.World.Direction * moveAmount;

        if (!_ctx.Map.IsBlocked((int)(_ctx.Transform.World.Position.X + moveStep.X), (int)_ctx.Transform.World.Position.Y))
            _ctx.Transform.Local.Position.X += moveStep.X;

        if (!_ctx.Map.IsBlocked((int)_ctx.Transform.World.Position.X, (int)(_ctx.Transform.World.Position.Y + moveStep.Y)))
            _ctx.Transform.Local.Position.Y += moveStep.Y;

        var actualDistance = Vector2.Distance(previousPos, _ctx.Transform.Local.Position);
        if (actualDistance > 0)
            _ctx.Stats.Metrics.RecordWalking(actualDistance);
    }

    private void HandleRotation(float rotSpeed)
    {
        if (InputBindings.IsActionDown(InputActions.RotateLeft))
            RotatePlayer(-rotSpeed);
        else if (InputBindings.IsActionDown(InputActions.RotateRight))
            RotatePlayer(rotSpeed);
    }

    private void RotatePlayer(float angle)
    {
        var oldDirection = _ctx.Transform.Local.Direction;
        var cos = MathF.Cos(angle);
        var sin = MathF.Sin(angle);

        _ctx.Transform.Local.Direction = new Vector2(
            oldDirection.X * cos - oldDirection.Y * sin,
            oldDirection.X * sin + oldDirection.Y * cos
        );

        _ctx.RotatePlane(angle);
    }
}
```

**Verify:** Build succeeds

---

## Task 7: Add AttackSpeed to ItemTemplate

**Files:**
- Modify: `games/Solocaster/Inventory/ItemTemplate.cs`

**Changes:** Add `AttackSpeed` property with default value 1.0.

**Implementation:**

Add after line 17 (`public Dictionary<StatType, float> Requirements { get; init; } = new();`):

```csharp
    public float AttackSpeed { get; init; } = 1.0f;
```

**Verify:** Build succeeds

---

## Task 8: Update ItemTemplateLoader for AttackSpeed

**Files:**
- Modify: `games/Solocaster/Inventory/ItemTemplateLoader.cs`

**Changes:** Add AttackSpeed to ItemTemplateData and template creation.

**Step 1:** Add to `ItemTemplateData` class (around line 128):

```csharp
        public float? AttackSpeed { get; set; }
```

**Step 2:** Add to template creation in `LoadTemplateFile` method (around line 60, after Requirements):

```csharp
                AttackSpeed = itemData.AttackSpeed ?? 1.0f,
```

**Verify:** Build succeeds

---

## Task 9: Update weapons.json with AttackSpeed

**Files:**
- Modify: `games/Solocaster/data/templates/items/weapons.json`

**Changes:** Add attackSpeed to each weapon.

**Implementation:** Replace the entire file:

```json
{
  "items": [
    {
      "id": "iron_sword",
      "name": "Iron Sword",
      "description": "A basic iron sword, reliable and sturdy.",
      "iconPath": "misc_items2:sword_ground_01",
      "worldSpritePath": "misc_items2:sword_ground_01",
      "worldSpriteScale": 0.35,
      "itemType": "Weapon",
      "equipSlot": "RightHand",
      "weight": 3.0,
      "stackable": false,
      "statModifiers": { "Damage": 8 },
      "requirements": { "Strength": 5 },
      "attackSpeed": 1.0
    },
    {
      "id": "steel_sword",
      "name": "Steel Sword",
      "description": "A well-crafted steel sword with superior edge.",
      "iconPath": "misc_items2:sword_ground_02",
      "worldSpritePath": "misc_items2:sword_ground_02",
      "worldSpriteScale": 0.35,
      "itemType": "Weapon",
      "equipSlot": "RightHand",
      "weight": 3.5,
      "stackable": false,
      "statModifiers": { "Damage": 12 },
      "requirements": { "Strength": 8 },
      "attackSpeed": 1.0
    },
    {
      "id": "battle_axe",
      "name": "Battle Axe",
      "description": "A heavy two-handed axe that cleaves through armor.",
      "iconPath": "misc_items2:axe_ground_01",
      "worldSpritePath": "misc_items2:axe_ground_01",
      "worldSpriteScale": 0.4,
      "itemType": "Weapon",
      "equipSlot": "RightHand",
      "weight": 5.0,
      "stackable": false,
      "statModifiers": { "Damage": 15 },
      "requirements": { "Strength": 12 },
      "attackSpeed": 0.8
    },
    {
      "id": "morningstar",
      "name": "Morningstar",
      "description": "A spiked mace that crushes armor and bone alike.",
      "iconPath": "misc_items2:mace_morningstar",
      "worldSpritePath": "misc_items2:mace_morningstar",
      "worldSpriteScale": 0.38,
      "itemType": "Weapon",
      "equipSlot": "RightHand",
      "weight": 4.0,
      "stackable": false,
      "statModifiers": { "Damage": 10, "CriticalChance": 5 },
      "requirements": { "Strength": 7 },
      "attackSpeed": 0.7
    },
    {
      "id": "scepter",
      "name": "Arcane Scepter",
      "description": "A magical scepter that channels arcane energy.",
      "iconPath": "misc_items2:mace_scepter",
      "worldSpritePath": "misc_items2:mace_scepter",
      "worldSpriteScale": 0.35,
      "itemType": "Weapon",
      "equipSlot": "RightHand",
      "weight": 2.0,
      "stackable": false,
      "statModifiers": { "SpellPower": 8, "Damage": 3 },
      "requirements": { "Intelligence": 8 },
      "attackSpeed": 1.2
    }
  ]
}
```

**Verify:** Build succeeds, JSON is valid

---

## Task 10: Refactor PlayerBrain to use StateMachine

**Files:**
- Modify: `games/Solocaster/Components/PlayerBrain.cs`

**Purpose:** Replace inline state logic with Solo.AI.StateMachine and player state classes.

**Implementation:** Replace the entire file:

```csharp
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Solo;
using Solo.AI;
using Solo.Components;
using Solocaster.Components.PlayerStates;
using Solocaster.Entities;
using Solocaster.Input;
using Solocaster.Inventory;
using Solocaster.UI;

namespace Solocaster.Components;

public class PlayerBrain : Component
{
    private readonly Map _map;

    private TransformComponent _transform = null!;
    private InventoryComponent _inventory = null!;
    private StatsComponent _stats = null!;

    private StateMachine _stateMachine = null!;
    private PlayerStateContext _context = null!;

    private PlayerExploringState _exploringState = null!;
    private PlayerCombatState _combatState = null!;
    private PlayerRunningState _runningState = null!;
    private PlayerExhaustedState _exhaustedState = null!;

    public CharacterPanel? CharacterPanel { get; set; }
    public MetricsPanel? MetricsPanel { get; set; }
    public SpatialGrid? SpatialGrid { get; set; }
    public Raycaster? Raycaster { get; set; }
    public GameObject? MiniMapEntity { get; set; }
    public GameObject? DebugUIEntity { get; set; }

    public Vector2 Plane => _context?.Plane ?? new Vector2(0, 0.45f);
    public PlayerState State => GetCurrentPlayerState();
    public float CurrentMoveSpeed => _context?.CurrentMoveSpeed ?? 0f;

    public float LeftHandRaiseAmount => _context?.LeftHandRaiseAmount ?? 0f;
    public float RightHandRaiseAmount => _context?.RightHandRaiseAmount ?? 0f;

    public PlayerBrain(GameObject owner, Map map) : base(owner)
    {
        _map = map;
    }

    protected override void InitCore()
    {
        _transform = Owner.Components.Get<TransformComponent>();
        _inventory = Owner.Components.Get<InventoryComponent>();
        _stats = Owner.Components.Get<StatsComponent>();

        InputBindings.Initialize();

        _context = new PlayerStateContext
        {
            Transform = _transform,
            Inventory = _inventory,
            Stats = _stats,
            Map = _map
        };

        _exploringState = new PlayerExploringState(Owner, _context);
        _combatState = new PlayerCombatState(Owner, _context);
        _runningState = new PlayerRunningState(Owner, _context);
        _exhaustedState = new PlayerExhaustedState(Owner, _context);

        _stateMachine = new StateMachine(_exploringState);
        SetupTransitions();

        base.InitCore();
    }

    private void SetupTransitions()
    {
        // Exploring -> Combat (R key)
        _stateMachine.AddTransition(_exploringState, _combatState,
            _ => InputBindings.IsActionPressed(InputActions.ToggleCombat));

        // Combat -> Exploring (R key)
        _stateMachine.AddTransition(_combatState, _exploringState,
            _ => InputBindings.IsActionPressed(InputActions.ToggleCombat));

        // Exploring -> Running (Shift + Forward + has stamina)
        _stateMachine.AddTransition(_exploringState, _runningState,
            _ => InputBindings.IsActionDown(InputActions.Run) &&
                 InputBindings.IsActionDown(InputActions.MoveForward) &&
                 _stats.CurrentStamina > 0,
            state => _context.PreviousStateBeforeRun = PlayerState.Exploring);

        // Combat -> Running (Shift + Forward + has stamina)
        _stateMachine.AddTransition(_combatState, _runningState,
            _ => InputBindings.IsActionDown(InputActions.Run) &&
                 InputBindings.IsActionDown(InputActions.MoveForward) &&
                 _stats.CurrentStamina > 0,
            state => _context.PreviousStateBeforeRun = PlayerState.Combat);

        // Running -> Exhausted (stamina depleted)
        _stateMachine.AddTransition(_runningState, _exhaustedState,
            _ => _stats.IsExhausted);

        // Running -> Exploring (stopped running, was exploring)
        _stateMachine.AddTransition(_runningState, _exploringState,
            _ => (!InputBindings.IsActionDown(InputActions.Run) ||
                  !InputBindings.IsActionDown(InputActions.MoveForward)) &&
                 _context.PreviousStateBeforeRun == PlayerState.Exploring);

        // Running -> Combat (stopped running, was in combat)
        _stateMachine.AddTransition(_runningState, _combatState,
            _ => (!InputBindings.IsActionDown(InputActions.Run) ||
                  !InputBindings.IsActionDown(InputActions.MoveForward)) &&
                 _context.PreviousStateBeforeRun == PlayerState.Combat);

        // Exhausted -> Exploring (recovered, was exploring)
        _stateMachine.AddTransition(_exhaustedState, _exploringState,
            _ => !_stats.IsExhausted && _context.PreviousStateBeforeRun == PlayerState.Exploring);

        // Exhausted -> Combat (recovered, was in combat)
        _stateMachine.AddTransition(_exhaustedState, _combatState,
            _ => !_stats.IsExhausted && _context.PreviousStateBeforeRun == PlayerState.Combat);
    }

    protected override void UpdateCore(GameTime gameTime)
    {
        InputBindings.Update();

        // Sync external references to context
        _context.CharacterPanel = CharacterPanel;
        _context.MetricsPanel = MetricsPanel;
        _context.Raycaster = Raycaster;
        _context.MiniMapEntity = MiniMapEntity;
        _context.DebugUIEntity = DebugUIEntity;

        _stateMachine.Update(gameTime);
    }

    private PlayerState GetCurrentPlayerState()
    {
        if (_stateMachine == null)
            return PlayerState.Exploring;

        // Use reflection or type checking to determine current state
        // This is a bit awkward but necessary since StateMachine doesn't expose current state
        return _context?.PreviousStateBeforeRun switch
        {
            _ when _stats?.IsExhausted == true => PlayerState.Exhausted,
            _ when InputBindings.IsActionDown(InputActions.Run) &&
                   InputBindings.IsActionDown(InputActions.MoveForward) &&
                   _stats?.CurrentStamina > 0 => PlayerState.Running,
            _ => _context?.PreviousStateBeforeRun ?? PlayerState.Exploring
        };
    }
}
```

**Note:** The `GetCurrentPlayerState()` method is a workaround since `StateMachine` doesn't expose the current state. Consider adding a `CurrentState` property to `StateMachine` in the future.

**Verify:** Build succeeds

---

## Task 11: Update PlayerHandsRenderer for Raise Offset

**Files:**
- Modify: `games/Solocaster/Components/PlayerHandsRenderer.cs`

**Changes:** Add raise offset based on `PlayerBrain.LeftHandRaiseAmount` and `RightHandRaiseAmount`.

**Step 1:** Add constant after line 27 (`private const float LeftHandBobMultiplier = 1.15f;`):

```csharp
    private const float RaiseHeight = 150f;
```

**Step 2:** Update the `Render` method to apply raise offset. Replace lines 141-151 (right hand rendering):

```csharp
        // Right hand
        if (_rightHandTexture != null)
        {
            int w = (int)(_rightHandTexture.Width * Scale);
            int h = (int)(_rightHandTexture.Height * Scale);
            int armedOffset = _rightHandKey != TextureKeyEmpty ? (int)(ArmedVerticalOffset * Scale) : 0;
            int raiseOffset = (int)(-_playerBrain.RightHandRaiseAmount * RaiseHeight * Scale);

            int x = viewport.Width - w - (int)(HorizontalOffset * Scale);
            int y = viewport.Height - h + (int)bob + armedOffset + hideOffset + raiseOffset;

            spriteBatch.Draw(_rightHandTexture, new Rectangle(x, y, w, h), Color.White);
        }
```

**Step 3:** Replace lines 154-165 (left hand rendering):

```csharp
        // Left hand (mirrored, slightly larger/lower)
        if (_leftHandTexture != null)
        {
            int w = (int)(_leftHandTexture.Width * LeftHandScale);
            int h = (int)(_leftHandTexture.Height * LeftHandScale);
            int armedOffset = _leftHandKey != TextureKeyEmpty ? (int)(ArmedVerticalOffset * Scale) : 0;
            int raiseOffset = (int)(-_playerBrain.LeftHandRaiseAmount * RaiseHeight * Scale);

            int x = (int)(HorizontalOffset * Scale);
            int y = viewport.Height - h + (int)(-bob * LeftHandBobMultiplier) + (int)(LeftHandVerticalOffset * Scale) + armedOffset + hideOffset + raiseOffset;

            spriteBatch.Draw(_leftHandTexture, new Rectangle(x, y, w, h), null, Color.White, 0f, Vector2.Zero, SpriteEffects.FlipHorizontally, 0f);
        }
```

**Verify:** Build succeeds

---

## Task 12: Add CurrentState Property to StateMachine

**Files:**
- Modify: `Solo/AI/StateMachine.cs`

**Purpose:** Expose current state so PlayerBrain can determine which state is active.

**Changes:** Add property after line 9 (`private State? _currState;`):

```csharp
    public State? CurrentState => _currState;
```

**Verify:** Build succeeds

---

## Task 13: Fix PlayerBrain GetCurrentPlayerState

**Files:**
- Modify: `games/Solocaster/Components/PlayerBrain.cs`

**Purpose:** Use the new `CurrentState` property instead of the workaround.

**Changes:** Replace the `GetCurrentPlayerState` method:

```csharp
    private PlayerState GetCurrentPlayerState()
    {
        if (_stateMachine?.CurrentState == null)
            return PlayerState.Exploring;

        return _stateMachine.CurrentState switch
        {
            PlayerExploringState => PlayerState.Exploring,
            PlayerCombatState => PlayerState.Combat,
            PlayerRunningState => PlayerState.Running,
            PlayerExhaustedState => PlayerState.Exhausted,
            _ => PlayerState.Exploring
        };
    }
```

**Verify:** Build succeeds

---

## Final Verification

Run the game and verify:

```bash
dotnet run --project games/Solocaster/Solocaster.csproj
```

**Test checklist:**
1. [ ] Game starts in Exploring mode (hands hidden)
2. [ ] Press R to toggle Combat mode (hands visible)
3. [ ] In Combat mode, right-click raises right hand (attack animation)
4. [ ] Attack animation: raise → brief hold → lower
5. [ ] Cooldown prevents spam clicking
6. [ ] Cannot attack while other hand is mid-swing (alternating rule)
7. [ ] If shield equipped, hold mouse button keeps hand raised (parry)
8. [ ] Release parry button lowers hand smoothly
9. [ ] Shift+W starts Running (hands visible but no attacks)
10. [ ] Running drains stamina, leads to Exhausted state
11. [ ] Exhausted state has reduced speed, recovers to previous state

---

## Files Summary

| File | Action |
|------|--------|
| `Components/HandActionState.cs` | Create |
| `Components/PlayerStates/PlayerStateContext.cs` | Create |
| `Components/PlayerStates/PlayerExploringState.cs` | Create |
| `Components/PlayerStates/PlayerCombatState.cs` | Create |
| `Components/PlayerStates/PlayerRunningState.cs` | Create |
| `Components/PlayerStates/PlayerExhaustedState.cs` | Create |
| `Components/PlayerBrain.cs` | Refactor |
| `Components/PlayerHandsRenderer.cs` | Modify |
| `Inventory/ItemTemplate.cs` | Modify |
| `Inventory/ItemTemplateLoader.cs` | Modify |
| `Solo/AI/StateMachine.cs` | Modify |
| `data/templates/items/weapons.json` | Modify |

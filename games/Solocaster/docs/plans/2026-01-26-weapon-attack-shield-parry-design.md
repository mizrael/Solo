# Weapon Attack & Shield Parry Design

## Goal

Handle player combat actions: weapons attack, shields parry. Visual feedback by raising the acting hand. Mouse buttons control left/right hands. Actions only available in combat mode.

## Architecture

Refactor `PlayerBrain` to use `Solo.AI.StateMachine` with four player states. Combat state handles hand actions with simple enum + timer per hand.

---

## Player State Machine

### States

| Class | Responsibilities |
|-------|------------------|
| `PlayerExploringState` | Movement, rotation, interact, pickup, toggle panels |
| `PlayerCombatState` | Movement, rotation, attack/parry (mouse), hand action tracking |
| `PlayerRunningState` | Forward movement, stamina drain |
| `PlayerExhaustedState` | Reduced speed, stamina regen |

### Transitions

| From | To | Predicate |
|------|----|-----------|
| Exploring | Combat | `ToggleCombat` pressed |
| Combat | Exploring | `ToggleCombat` pressed |
| Exploring/Combat | Running | `Run` held + moving forward + has stamina |
| Running | Previous | `Run` released or stopped moving |
| Running | Exhausted | `stats.IsExhausted` |
| Exhausted | Previous | `!stats.IsExhausted` |

### PlayerBrain Refactored

- Creates `StateMachine` with `PlayerExploringState` as start
- Sets up transitions with predicates
- `UpdateCore()` delegates to `_stateMachine.Update(gameTime)`
- Exposes shared context: Transform, Inventory, Stats, Map
- Exposes `LeftHandRaiseAmount` / `RightHandRaiseAmount` (0-1 floats) for renderer

---

## Hand Action System

### Input Mapping

- Left Mouse Button → Left hand action
- Right Mouse Button → Right hand action
- Only processed when in `PlayerCombatState`

### Hand Action States

```csharp
enum HandActionState { Idle, Raising, Held, Lowering }
```

Per hand tracking:
- `HandActionState` current state
- `float` timer for current phase
- `float` cooldown remaining (weapons only)

### Timing

| Phase | Duration | Notes |
|-------|----------|-------|
| Raising | 0.15s | Hand moves up |
| Held (attack) | 0.10s | Brief pause at top |
| Held (parry) | unlimited | While button held |
| Lowering | 0.25s | Return to rest |

Total attack swing: ~0.5s

### Attack Flow

```
if (mousePressed && hand.IsWeapon && cooldownExpired):
    if (otherHand is Idle or Lowering):  // alternating rule
        state = Raising
        cooldown = CalculateCooldown(weapon, agility)

// State progression:
Raising → Held (after 0.15s)
Held → Lowering (after 0.10s, fire attack event here)
Lowering → Idle (after 0.25s)
```

### Parry Flow

```
if (mouseHeld && hand.IsShield):
    if state == Idle: state = Raising
    if state == Raising && timer >= 0.15s: state = Held
    // stays Held while button held
else if state in (Held, Raising):
    state = Lowering
```

### Alternating Rule

Cannot start a new attack while the other hand is in Raising or Held state. Prevents spam-clicking both buttons.

---

## Attack Speed System

### ItemTemplate Extension

```csharp
public float AttackSpeed { get; init; } = 1.0f;
```

### Cooldown Calculation

```csharp
const float BaseCooldown = 0.8f;

float CalculateCooldown(ItemTemplate weapon, float agility)
{
    float weaponModifier = 1f / weapon.AttackSpeed;
    float agilityModifier = 1f / (1f + agility * 0.02f);
    return BaseCooldown * weaponModifier * agilityModifier;
}
```

### Example Values

| Weapon | AttackSpeed | Cooldown (0 Agi) |
|--------|-------------|------------------|
| Longsword | 1.0 | 0.80s |
| Dagger | 1.5 | 0.53s |
| Morningstar | 0.7 | 1.14s |
| Axe | 0.8 | 1.00s |

---

## Renderer Integration

### PlayerHandsRenderer Changes

Reads `LeftHandRaiseAmount` / `RightHandRaiseAmount` from `PlayerBrain`.

```csharp
private const float RaiseHeight = 150f;

// In Render(), per hand:
float raiseAmount = _playerBrain.RightHandRaiseAmount;
int raiseOffset = (int)(-raiseAmount * RaiseHeight * Scale);
int y = baseY + raiseOffset;
```

### Decoupling

Renderer doesn't know about `PlayerCombatState` directly. Brain exposes raise amounts as simple floats. Other states return 0.

---

## Shield Parry Mechanics (Future)

- Free to hold shield raised
- Stamina cost only when actually blocking a hit
- Requires hit detection system (not in this scope)

---

## Scope

**In scope:**
- Player state machine refactor
- Hand action states and timers
- Mouse input for attack/parry
- Visual feedback (hand raising)
- Attack cooldown with weapon speed + agility

**Out of scope (future):**
- Hit detection and damage dealing
- Monster attacks and blocking
- Different attack types (two-handed, ranged, magic)
- Sound effects

---

## Files to Create/Modify

| File | Action |
|------|--------|
| `Components/PlayerStates/PlayerExploringState.cs` | Create |
| `Components/PlayerStates/PlayerCombatState.cs` | Create |
| `Components/PlayerStates/PlayerRunningState.cs` | Create |
| `Components/PlayerStates/PlayerExhaustedState.cs` | Create |
| `Components/HandActionState.cs` | Create |
| `Components/PlayerBrain.cs` | Refactor to use StateMachine |
| `Components/PlayerHandsRenderer.cs` | Add raise offset support |
| `Inventory/ItemTemplate.cs` | Add AttackSpeed property |
| `data/templates/items/weapons.json` | Add AttackSpeed values |

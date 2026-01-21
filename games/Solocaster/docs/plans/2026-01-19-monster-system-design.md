# Monster System Design

## Overview

Add skeleton monsters to Solocaster with animated sprites, directional rendering, and random level spawning.

## Monster Templates

Location: `data/templates/monsters/skeleton.json`

```json
{
  "id": "skeleton",
  "name": "Skeleton",
  "stats": {
    "Strength": 8,
    "Agility": 12,
    "Vitality": 6,
    "Intelligence": 2,
    "Wisdom": 2
  },
  "behavior": {
    "detectionRange": 8.0,
    "attackRange": 1.2,
    "moveSpeed": 2.0
  },
  "animations": {
    "idle": "data/animations/skeleton/skeleton_idle",
    "walk": "data/animations/skeleton/skeleton_walk",
    "attack": "data/animations/skeleton/skeleton_attack",
    "hit": "data/animations/skeleton/skeleton_hit",
    "death": "data/animations/skeleton/skeleton_death"
  },
  "scale": 0.8,
  "anchor": "Bottom"
}
```

### Derived Stats

Calculated using same formulas as players:
- `MaxHealth` = 50 + Vitality × 5 + equipment
- `MaxMana` = 20 + Intelligence × 3 + Wisdom × 2 + equipment
- `Damage` = Strength × 0.5 + equipment
- `Defense` = Agility × 0.3 + equipment

## Animation System

### IFrameProvider Interface

```csharp
public interface IFrameProvider
{
    Rectangle GetCurrentBounds();
    Texture2D GetTexture();
}
```

### StaticFrameProvider

Wraps existing `Sprite` for non-animated billboards:

```csharp
public class StaticFrameProvider : IFrameProvider
{
    private readonly Sprite _sprite;
    public Rectangle GetCurrentBounds() => _sprite.Bounds;
    public Texture2D GetTexture() => _sprite.Texture;
}
```

### AnimationController

Manages animated sprites with state and direction:

```csharp
public class AnimationController : IFrameProvider
{
    private Dictionary<string, DirectionalAnimation> _animations;
    private string _currentState; // "idle", "walk", "attack", etc.
    private Direction _currentDirection; // Front, Back, Left, Right
    private int _currentFrame;
    private double _frameTimer;

    public void SetState(string state);
    public void SetDirection(Direction dir);
    public void Update(GameTime gameTime);
    public Rectangle GetCurrentBounds();
    public Texture2D GetTexture();

    public event Action OnAnimationComplete;
}
```

### DirectionalAnimation

Holds animations for all 4 directions with fallback:

```csharp
public class DirectionalAnimation
{
    private Dictionary<Direction, AnimatedSpriteSheet> _directions;

    public AnimatedSpriteSheet Get(Direction dir)
    {
        if (_directions.TryGetValue(dir, out var anim))
            return anim;
        return _directions[Direction.Front]; // fallback
    }
}
```

### Direction Enum

```csharp
public enum Direction { Front, Back, Left, Right }
```

### Animation File Naming

Animation paths in template are base paths. System appends direction suffix:
- `skeleton_idle` → `skeleton_idle_front.json`, `skeleton_idle_back.json`, etc.

Missing directions fall back to `_front`.

## Monster Spawning

### Level JSON Structure

```json
{
  "spritesheets": ["dungeon1", "dungeon_doors1"],
  "map": { ... },
  "monsters": {
    "density": 0.02,
    "encounters": [
      {
        "weight": 50,
        "groups": [
          { "id": "skeleton", "min": 1, "max": 3 }
        ]
      },
      {
        "weight": 30,
        "groups": [
          { "id": "skeleton", "min": 2, "max": 2 },
          { "id": "skeleton_warrior", "min": 1, "max": 1 }
        ]
      },
      {
        "weight": 20,
        "groups": [
          { "id": "skeleton_warrior", "min": 1, "max": 2 }
        ]
      }
    ]
  }
}
```

### MonsterSpawner

Responsibilities:
1. Iterate floor tiles with `density` probability during map building
2. Pick encounter using weighted random from `encounters`
3. For each group, spawn `random(min, max)` monsters
4. Place in nearby valid floor tiles (not on walls, doors, other entities)
5. Create monster GameObjects with required components

## Rendering

### BillboardComponent Changes

Replace `Sprite` property with `IFrameProvider`:

```csharp
public class BillboardComponent : Component
{
    public IFrameProvider FrameProvider { get; set; }
    public Vector2 Scale { get; set; } = Vector2.One;
    public BillboardAnchor Anchor { get; set; } = BillboardAnchor.Center;
}
```

### Raycaster Changes

In `RenderBillboards()`:

```csharp
// Before:
var sprite = billboard.Sprite;
// uses sprite.Bounds, sprite.Texture

// After:
var frameProvider = billboard.FrameProvider;
var bounds = frameProvider.GetCurrentBounds();
var texture = frameProvider.GetTexture();
```

No changes to pixel rendering logic - it already works with Rectangle bounds and Texture2D.

## Monster AI

### MonsterBrainComponent

Responsibilities:
- State machine: idle, walk, attack, hit, death
- Calculate direction relative to player
- Update AnimationController state and direction
- Handle detection, pathfinding, combat (future)

### Direction Calculation

```csharp
var toPlayer = playerPos - monsterPos;
var angle = MathF.Atan2(toPlayer.Y, toPlayer.X) - monsterFacingAngle;
// Map angle to Front/Back/Left/Right quadrants
```

## Class Structure

### New Classes

```
Solocaster/
├── Monsters/
│   ├── MonsterTemplate.cs          # Data class for template properties
│   ├── MonsterTemplateLoader.cs    # Loads from data/templates/monsters/
│   └── Direction.cs                # Enum: Front, Back, Left, Right
├── Components/
│   ├── MonsterBrainComponent.cs    # AI, state machine, direction calc
│   ├── IFrameProvider.cs           # Interface for billboard frames
│   ├── StaticFrameProvider.cs      # Wraps Sprite for static billboards
│   └── AnimationController.cs      # Manages animated sprites
├── Animations/
│   └── DirectionalAnimation.cs     # Holds 4-direction animation set
└── Persistence/
    └── MonsterSpawner.cs           # Spawns encounters during map build
```

### Modified Classes

- `BillboardComponent` - Replace `Sprite` with `IFrameProvider`
- `StatsComponent` - Add `CalculateDamage()`, `CalculateDefense()` methods
- `LevelLoader` - Parse `monsters` section, invoke `MonsterSpawner`
- `Raycaster.RenderBillboards()` - Use `IFrameProvider` instead of `Sprite`
- `EntityFactory` - Handle monster entity creation

### Data Files

- `data/templates/monsters/skeleton.json`

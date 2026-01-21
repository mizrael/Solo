# Monster System Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Add animated skeleton monsters with directional sprites, random level spawning via encounter groups, and foundational AI.

**Architecture:** Extend BillboardComponent with IFrameProvider abstraction for animated sprites. Monster templates define stats and animation paths. MonsterSpawner places encounter groups during map building. MonsterBrainComponent manages state and calculates direction relative to player.

**Tech Stack:** C# / .NET, MonoGame, Solo engine, JSON templates

---

## Task 1: Direction Enum

**Files:**
- Create: `Solocaster/Monsters/Direction.cs`

**Step 1: Create Direction enum**

```csharp
namespace Solocaster.Monsters;

public enum Direction
{
    Front,
    Back,
    Left,
    Right
}
```

**Step 2: Verify build**

Run: `dotnet build games/Solocaster/Solocaster.csproj`
Expected: Build succeeded

**Step 3: Commit**

```bash
git add games/Solocaster/Monsters/Direction.cs
git commit -m "feat: add Direction enum for monster animations"
```

---

## Task 2: IFrameProvider Interface

**Files:**
- Create: `Solocaster/Components/IFrameProvider.cs`

**Step 1: Create interface**

```csharp
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Solocaster.Components;

public interface IFrameProvider
{
    Rectangle GetCurrentBounds();
    Texture2D GetTexture();
}
```

**Step 2: Verify build**

Run: `dotnet build games/Solocaster/Solocaster.csproj`
Expected: Build succeeded

**Step 3: Commit**

```bash
git add games/Solocaster/Components/IFrameProvider.cs
git commit -m "feat: add IFrameProvider interface for billboard frames"
```

---

## Task 3: StaticFrameProvider

**Files:**
- Create: `Solocaster/Components/StaticFrameProvider.cs`

**Step 1: Create StaticFrameProvider**

```csharp
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Solo.Assets;

namespace Solocaster.Components;

public class StaticFrameProvider : IFrameProvider
{
    private readonly Sprite _sprite;

    public StaticFrameProvider(Sprite sprite)
    {
        _sprite = sprite;
    }

    public Rectangle GetCurrentBounds() => _sprite.Bounds;

    public Texture2D GetTexture() => _sprite.Texture;
}
```

**Step 2: Verify build**

Run: `dotnet build games/Solocaster/Solocaster.csproj`
Expected: Build succeeded

**Step 3: Commit**

```bash
git add games/Solocaster/Components/StaticFrameProvider.cs
git commit -m "feat: add StaticFrameProvider for non-animated billboards"
```

---

## Task 4: Update BillboardComponent

**Files:**
- Modify: `Solocaster/Components/BillboardComponent.cs`

**Step 1: Replace Sprite with IFrameProvider**

```csharp
using Microsoft.Xna.Framework;
using Solo;
using Solo.Components;
using System;

namespace Solocaster.Components;

public enum BillboardAnchor
{
    Bottom,
    Center,
    Top
}

public class BillboardComponent : Component
{
    private IFrameProvider _frameProvider;

    private BillboardComponent(GameObject owner) : base(owner)
    {
    }

    public IFrameProvider FrameProvider
    {
        get => _frameProvider;
        set => _frameProvider = value ?? throw new ArgumentNullException(nameof(value));
    }

    public Vector2 Scale { get; set; } = Vector2.One;
    public BillboardAnchor Anchor { get; set; } = BillboardAnchor.Center;
}
```

**Step 2: Verify build - expect errors**

Run: `dotnet build games/Solocaster/Solocaster.csproj`
Expected: Build errors in files that use `billboard.Sprite`

**Step 3: Note files to fix**

The build will show which files reference `Sprite`. These need updating in subsequent tasks.

---

## Task 5: Update Raycaster Billboard Rendering

**Files:**
- Modify: `Solocaster/Raycaster.cs` (RenderBillboards method, around line 750-830)

**Step 1: Update RenderBillboards to use IFrameProvider**

Find and replace in the `RenderBillboards` method:

```csharp
// Change from:
var billboard = entity.Components.Get<BillboardComponent>();
var sprite = billboard.Sprite;
var isPickupable = entity.Components.Has<PickupableComponent>();

// Change to:
var billboard = entity.Components.Get<BillboardComponent>();
var frameProvider = billboard.FrameProvider;
var isPickupable = entity.Components.Has<PickupableComponent>();
```

And further down where sprite properties are used:

```csharp
// Change from:
var texturePtr = GetOrCacheSpriteTexture(sprite.Texture);

projections.Add(new SpriteProjection
{
    // ...
    TexturePtr = texturePtr,
    TexWidth = sprite.Texture.Width,
    TexHeight = sprite.Texture.Height,
    SpriteBounds = sprite.Bounds,
    // ...
});

// Change to:
var texture = frameProvider.GetTexture();
var bounds = frameProvider.GetCurrentBounds();
var texturePtr = GetOrCacheSpriteTexture(texture);

projections.Add(new SpriteProjection
{
    // ...
    TexturePtr = texturePtr,
    TexWidth = texture.Width,
    TexHeight = texture.Height,
    SpriteBounds = bounds,
    // ...
});
```

**Step 2: Verify build - may still have errors**

Run: `dotnet build games/Solocaster/Solocaster.csproj`
Expected: Build errors reduced, remaining in EntityFactory or similar

---

## Task 6: Update EntityFactory

**Files:**
- Modify: `Solocaster/Entities/EntityFactory.cs`

**Step 1: Read current EntityFactory**

Examine how billboards are created to understand the change needed.

**Step 2: Update billboard creation to use StaticFrameProvider**

Find where `BillboardComponent` is created and `Sprite` is set. Change to:

```csharp
// Change from:
billboard.Sprite = sprite;

// Change to:
billboard.FrameProvider = new StaticFrameProvider(sprite);
```

**Step 3: Add using statement**

Add at top of file:
```csharp
using Solocaster.Components;
```

**Step 4: Verify build**

Run: `dotnet build games/Solocaster/Solocaster.csproj`
Expected: Build succeeded (or shows remaining files to fix)

**Step 5: Fix any remaining usages**

Search for any remaining `billboard.Sprite` usages and update similarly.

**Step 6: Commit**

```bash
git add games/Solocaster/Components/BillboardComponent.cs games/Solocaster/Raycaster.cs games/Solocaster/Entities/EntityFactory.cs
git commit -m "refactor: update billboard system to use IFrameProvider"
```

---

## Task 7: Verify Game Still Runs

**Step 1: Run the game**

Run: `dotnet run --project games/Solocaster/Solocaster.csproj`
Expected: Game runs, existing billboards (barrels, items, etc.) render correctly

**Step 2: Commit if any fixes were needed**

If fixes were required, commit them:
```bash
git add -A
git commit -m "fix: resolve billboard rendering issues"
```

---

## Task 8: DirectionalAnimation Class

**Files:**
- Create: `Solocaster/Animations/DirectionalAnimation.cs`

**Step 1: Create DirectionalAnimation**

```csharp
using System.Collections.Generic;
using Solo.Assets;
using Solocaster.Monsters;

namespace Solocaster.Animations;

public class DirectionalAnimation
{
    private readonly Dictionary<Direction, AnimatedSpriteSheet> _directions = new();

    public void Add(Direction direction, AnimatedSpriteSheet animation)
    {
        _directions[direction] = animation;
    }

    public AnimatedSpriteSheet Get(Direction direction)
    {
        if (_directions.TryGetValue(direction, out var animation))
            return animation;

        return _directions.TryGetValue(Direction.Front, out var fallback)
            ? fallback
            : throw new KeyNotFoundException($"No animation for direction {direction} and no Front fallback");
    }

    public bool HasDirection(Direction direction) => _directions.ContainsKey(direction);

    public bool HasAny => _directions.Count > 0;
}
```

**Step 2: Verify build**

Run: `dotnet build games/Solocaster/Solocaster.csproj`
Expected: Build succeeded

**Step 3: Commit**

```bash
git add games/Solocaster/Animations/DirectionalAnimation.cs
git commit -m "feat: add DirectionalAnimation for 4-direction animation sets"
```

---

## Task 9: AnimationController

**Files:**
- Create: `Solocaster/Components/AnimationController.cs`

**Step 1: Create AnimationController**

```csharp
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Solo.Assets;
using Solocaster.Animations;
using Solocaster.Monsters;

namespace Solocaster.Components;

public class AnimationController : IFrameProvider
{
    private readonly Dictionary<string, DirectionalAnimation> _animations = new();
    private string _currentState = "idle";
    private Direction _currentDirection = Direction.Front;
    private int _currentFrame;
    private double _frameTimer;

    public void AddAnimation(string state, DirectionalAnimation animation)
    {
        _animations[state] = animation;
    }

    public void SetState(string state)
    {
        if (_currentState == state)
            return;

        if (!_animations.ContainsKey(state))
            return;

        _currentState = state;
        _currentFrame = 0;
        _frameTimer = 0;
    }

    public void SetDirection(Direction direction)
    {
        _currentDirection = direction;
    }

    public void Update(GameTime gameTime)
    {
        var animation = GetCurrentAnimation();
        if (animation == null)
            return;

        _frameTimer += gameTime.ElapsedGameTime.TotalMilliseconds;
        var frameDuration = 1000.0 / animation.Fps;

        if (_frameTimer >= frameDuration)
        {
            _frameTimer -= frameDuration;
            _currentFrame++;

            if (_currentFrame >= animation.Frames.Length)
            {
                _currentFrame = 0;
                OnAnimationComplete?.Invoke(_currentState);
            }
        }
    }

    public Rectangle GetCurrentBounds()
    {
        var animation = GetCurrentAnimation();
        if (animation == null)
            return Rectangle.Empty;

        return animation.Frames[_currentFrame].Bounds;
    }

    public Texture2D GetTexture()
    {
        var animation = GetCurrentAnimation();
        return animation?.Texture;
    }

    private AnimatedSpriteSheet GetCurrentAnimation()
    {
        if (!_animations.TryGetValue(_currentState, out var directional))
            return null;

        return directional.Get(_currentDirection);
    }

    public string CurrentState => _currentState;
    public Direction CurrentDirection => _currentDirection;
    public int CurrentFrame => _currentFrame;

    public event Action<string> OnAnimationComplete;
}
```

**Step 2: Verify build**

Run: `dotnet build games/Solocaster/Solocaster.csproj`
Expected: Build succeeded

**Step 3: Commit**

```bash
git add games/Solocaster/Components/AnimationController.cs
git commit -m "feat: add AnimationController for animated billboards"
```

---

## Task 10: MonsterTemplate Data Class

**Files:**
- Create: `Solocaster/Monsters/MonsterTemplate.cs`

**Step 1: Create MonsterTemplate**

```csharp
using System.Collections.Generic;
using Solocaster.Components;
using Solocaster.Inventory;

namespace Solocaster.Monsters;

public class MonsterTemplate
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public Dictionary<StatType, float> Stats { get; init; } = new();
    public MonsterBehavior Behavior { get; init; } = new();
    public Dictionary<string, string> Animations { get; init; } = new();
    public float Scale { get; init; } = 1.0f;
    public BillboardAnchor Anchor { get; init; } = BillboardAnchor.Bottom;
}

public class MonsterBehavior
{
    public float DetectionRange { get; init; } = 8.0f;
    public float AttackRange { get; init; } = 1.2f;
    public float MoveSpeed { get; init; } = 2.0f;
}
```

**Step 2: Verify build**

Run: `dotnet build games/Solocaster/Solocaster.csproj`
Expected: Build succeeded

**Step 3: Commit**

```bash
git add games/Solocaster/Monsters/MonsterTemplate.cs
git commit -m "feat: add MonsterTemplate data class"
```

---

## Task 11: MonsterTemplateLoader

**Files:**
- Create: `Solocaster/Monsters/MonsterTemplateLoader.cs`

**Step 1: Create MonsterTemplateLoader**

```csharp
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Solocaster.Components;
using Solocaster.Inventory;

namespace Solocaster.Monsters;

public static class MonsterTemplateLoader
{
    private static readonly Dictionary<string, MonsterTemplate> Templates = new();

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public static void LoadAllFromFolder(string folderPath)
    {
        if (!Directory.Exists(folderPath))
            return;

        foreach (var file in Directory.GetFiles(folderPath, "*.json"))
        {
            LoadFromFile(file);
        }
    }

    public static void LoadFromFile(string filePath)
    {
        var json = File.ReadAllText(filePath);
        var dto = JsonSerializer.Deserialize<MonsterTemplateDto>(json, JsonOptions);

        if (dto == null)
            throw new InvalidOperationException($"Failed to deserialize monster template: {filePath}");

        var template = new MonsterTemplate
        {
            Id = dto.Id,
            Name = dto.Name,
            Stats = ParseStats(dto.Stats),
            Behavior = new MonsterBehavior
            {
                DetectionRange = dto.Behavior?.DetectionRange ?? 8.0f,
                AttackRange = dto.Behavior?.AttackRange ?? 1.2f,
                MoveSpeed = dto.Behavior?.MoveSpeed ?? 2.0f
            },
            Animations = dto.Animations ?? new Dictionary<string, string>(),
            Scale = dto.Scale,
            Anchor = dto.Anchor
        };

        Templates[template.Id] = template;
    }

    public static MonsterTemplate Get(string id)
    {
        if (Templates.TryGetValue(id, out var template))
            return template;

        throw new KeyNotFoundException($"Monster template not found: {id}");
    }

    public static bool TryGet(string id, out MonsterTemplate template)
    {
        return Templates.TryGetValue(id, out template);
    }

    public static IEnumerable<MonsterTemplate> GetAll() => Templates.Values;

    private static Dictionary<StatType, float> ParseStats(Dictionary<string, float> stats)
    {
        var result = new Dictionary<StatType, float>();
        if (stats == null)
            return result;

        foreach (var (key, value) in stats)
        {
            if (Enum.TryParse<StatType>(key, true, out var statType))
                result[statType] = value;
        }

        return result;
    }

    private class MonsterTemplateDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public Dictionary<string, float> Stats { get; set; }
        public BehaviorDto Behavior { get; set; }
        public Dictionary<string, string> Animations { get; set; }
        public float Scale { get; set; } = 1.0f;
        public BillboardAnchor Anchor { get; set; } = BillboardAnchor.Bottom;

        public class BehaviorDto
        {
            public float DetectionRange { get; set; } = 8.0f;
            public float AttackRange { get; set; } = 1.2f;
            public float MoveSpeed { get; set; } = 2.0f;
        }
    }
}
```

**Step 2: Verify build**

Run: `dotnet build games/Solocaster/Solocaster.csproj`
Expected: Build succeeded

**Step 3: Commit**

```bash
git add games/Solocaster/Monsters/MonsterTemplateLoader.cs
git commit -m "feat: add MonsterTemplateLoader"
```

---

## Task 12: Create Skeleton Template JSON

**Files:**
- Create: `Solocaster/data/templates/monsters/skeleton.json`

**Step 1: Create skeleton template**

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
    "attack": "data/animations/skeleton/skeleton_attack"
  },
  "scale": 0.8,
  "anchor": "Bottom"
}
```

**Step 2: Commit**

```bash
git add games/Solocaster/data/templates/monsters/skeleton.json
git commit -m "feat: add skeleton monster template"
```

---

## Task 13: Update StatsComponent with Damage/Defense Formulas

**Files:**
- Modify: `Solocaster/Components/StatsComponent.cs`

**Step 1: Add Damage and Defense calculation methods**

Add these methods to `StatsComponent`:

```csharp
public float GetTotalStat(StatType stat)
{
    return stat switch
    {
        StatType.MaxHealth => CalculateMaxHealth(),
        StatType.MaxWeight => CalculateMaxWeight(),
        StatType.MaxMana => CalculateMaxMana(),
        StatType.Damage => CalculateDamage(),
        StatType.Defense => CalculateDefense(),
        _ => GetBaseStat(stat) + GetEquipmentBonus(stat)
    };
}

private float CalculateDamage()
{
    float strength = GetBaseStat(StatType.Strength) + GetEquipmentBonus(StatType.Strength);
    float bonusDamage = GetEquipmentBonus(StatType.Damage);
    return strength * 0.5f + bonusDamage;
}

private float CalculateDefense()
{
    float agility = GetBaseStat(StatType.Agility) + GetEquipmentBonus(StatType.Agility);
    float bonusDefense = GetEquipmentBonus(StatType.Defense);
    return agility * 0.3f + bonusDefense;
}
```

**Step 2: Verify build**

Run: `dotnet build games/Solocaster/Solocaster.csproj`
Expected: Build succeeded

**Step 3: Commit**

```bash
git add games/Solocaster/Components/StatsComponent.cs
git commit -m "feat: add Damage and Defense formulas to StatsComponent"
```

---

## Task 14: MonsterBrainComponent (Basic)

**Files:**
- Create: `Solocaster/Components/MonsterBrainComponent.cs`

**Step 1: Create basic MonsterBrainComponent**

```csharp
using System;
using Microsoft.Xna.Framework;
using Solo;
using Solo.Components;
using Solocaster.Monsters;

namespace Solocaster.Components;

public class MonsterBrainComponent : Component
{
    private TransformComponent _transform;
    private BillboardComponent _billboard;
    private AnimationController _animationController;
    private GameObject _player;

    private string _currentState = "idle";
    private float _facingAngle;

    public MonsterTemplate Template { get; set; }

    public MonsterBrainComponent(GameObject owner) : base(owner)
    {
    }

    public void Initialize(AnimationController animationController, GameObject player)
    {
        _animationController = animationController;
        _player = player;
    }

    protected override void InitCore()
    {
        _transform = Owner.Components.Get<TransformComponent>();
        _billboard = Owner.Components.Get<BillboardComponent>();
    }

    protected override void UpdateCore(GameTime gameTime)
    {
        if (_animationController == null || _player == null)
            return;

        UpdateDirectionToPlayer();
        _animationController.Update(gameTime);
    }

    private void UpdateDirectionToPlayer()
    {
        var playerTransform = _player.Components.Get<TransformComponent>();
        if (playerTransform == null)
            return;

        var toPlayer = playerTransform.World.Position - _transform.World.Position;
        var angleToPlayer = MathF.Atan2(toPlayer.Y, toPlayer.X);

        var relativeAngle = angleToPlayer - _facingAngle;

        // Normalize to -PI to PI
        while (relativeAngle > MathF.PI) relativeAngle -= MathF.PI * 2;
        while (relativeAngle < -MathF.PI) relativeAngle += MathF.PI * 2;

        // Map to direction (from monster's perspective, so inverted)
        // If player is in front of monster, monster shows its front to player
        var direction = relativeAngle switch
        {
            >= -MathF.PI / 4 and < MathF.PI / 4 => Direction.Front,
            >= MathF.PI / 4 and < 3 * MathF.PI / 4 => Direction.Right,
            >= -3 * MathF.PI / 4 and < -MathF.PI / 4 => Direction.Left,
            _ => Direction.Back
        };

        _animationController.SetDirection(direction);
    }

    public void SetState(string state)
    {
        _currentState = state;
        _animationController?.SetState(state);
    }

    public string CurrentState => _currentState;
}
```

**Step 2: Verify build**

Run: `dotnet build games/Solocaster/Solocaster.csproj`
Expected: Build succeeded

**Step 3: Commit**

```bash
git add games/Solocaster/Components/MonsterBrainComponent.cs
git commit -m "feat: add MonsterBrainComponent with direction calculation"
```

---

## Task 15: DirectionalAnimationLoader Helper

**Files:**
- Create: `Solocaster/Animations/DirectionalAnimationLoader.cs`

**Step 1: Create helper to load directional animations**

```csharp
using System.IO;
using Microsoft.Xna.Framework;
using Solo.Assets.Loaders;
using Solocaster.Monsters;

namespace Solocaster.Animations;

public static class DirectionalAnimationLoader
{
    private static readonly Direction[] AllDirections = { Direction.Front, Direction.Back, Direction.Left, Direction.Right };

    public static DirectionalAnimation Load(string basePath, Game game)
    {
        var animation = new DirectionalAnimation();

        foreach (var direction in AllDirections)
        {
            var suffix = direction.ToString().ToLower();
            var fullPath = $"{basePath}_{suffix}.json";

            if (File.Exists(fullPath))
            {
                var sheet = AnimatedSpriteSheetLoader.Load(fullPath, game);
                animation.Add(direction, sheet);
            }
        }

        // If no directions loaded, try loading without suffix (backwards compat)
        if (!animation.HasAny)
        {
            var defaultPath = $"{basePath}.json";
            if (File.Exists(defaultPath))
            {
                var sheet = AnimatedSpriteSheetLoader.Load(defaultPath, game);
                animation.Add(Direction.Front, sheet);
            }
        }

        return animation;
    }
}
```

**Step 2: Verify build**

Run: `dotnet build games/Solocaster/Solocaster.csproj`
Expected: Build succeeded

**Step 3: Commit**

```bash
git add games/Solocaster/Animations/DirectionalAnimationLoader.cs
git commit -m "feat: add DirectionalAnimationLoader helper"
```

---

## Task 16: MonsterFactory

**Files:**
- Create: `Solocaster/Entities/MonsterFactory.cs`

**Step 1: Create MonsterFactory**

```csharp
using Microsoft.Xna.Framework;
using Solo;
using Solo.Components;
using Solocaster.Animations;
using Solocaster.Components;
using Solocaster.Monsters;

namespace Solocaster.Entities;

public static class MonsterFactory
{
    public static GameObject Create(
        MonsterTemplate template,
        Vector2 position,
        Game game,
        GameObject sceneRoot,
        SpatialGrid spatialGrid,
        GameObject player)
    {
        var monster = new GameObject();
        sceneRoot.AddChild(monster);

        var transform = monster.Components.Add<TransformComponent>();
        transform.Local.Position = position;

        var animationController = CreateAnimationController(template, game);

        var billboard = monster.Components.Add<BillboardComponent>();
        billboard.FrameProvider = animationController;
        billboard.Scale = new Vector2(template.Scale, template.Scale);
        billboard.Anchor = template.Anchor;

        var brain = monster.Components.Add<MonsterBrainComponent>();
        brain.Template = template;
        brain.Initialize(animationController, player);

        spatialGrid.Add(monster, position);

        return monster;
    }

    private static AnimationController CreateAnimationController(MonsterTemplate template, Game game)
    {
        var controller = new AnimationController();

        foreach (var (state, basePath) in template.Animations)
        {
            var directionalAnim = DirectionalAnimationLoader.Load(basePath, game);
            if (directionalAnim.HasAny)
            {
                controller.AddAnimation(state, directionalAnim);
            }
        }

        controller.SetState("idle");
        return controller;
    }
}
```

**Step 2: Verify build**

Run: `dotnet build games/Solocaster/Solocaster.csproj`
Expected: Build succeeded

**Step 3: Commit**

```bash
git add games/Solocaster/Entities/MonsterFactory.cs
git commit -m "feat: add MonsterFactory for creating monster GameObjects"
```

---

## Task 17: MonsterSpawner

**Files:**
- Create: `Solocaster/Persistence/MonsterSpawner.cs`

**Step 1: Create MonsterSpawner**

```csharp
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Solo;
using Solocaster.Entities;
using Solocaster.Monsters;

namespace Solocaster.Persistence;

public class MonsterSpawner
{
    private readonly Random _random = new();

    public List<GameObject> Spawn(
        MonsterSpawnConfig config,
        Map map,
        Game game,
        GameObject sceneRoot,
        SpatialGrid spatialGrid,
        GameObject player)
    {
        var monsters = new List<GameObject>();
        var occupiedTiles = new HashSet<(int, int)>();

        for (int y = 0; y < map.Height; y++)
        {
            for (int x = 0; x < map.Width; x++)
            {
                if (!map.IsFloor(x, y))
                    continue;

                if (occupiedTiles.Contains((x, y)))
                    continue;

                if (_random.NextDouble() > config.Density)
                    continue;

                var encounter = PickEncounter(config.Encounters);
                if (encounter == null)
                    continue;

                var spawnedMonsters = SpawnEncounter(
                    encounter, x, y, map, game, sceneRoot, spatialGrid, player, occupiedTiles);
                monsters.AddRange(spawnedMonsters);
            }
        }

        return monsters;
    }

    private EncounterConfig PickEncounter(List<EncounterConfig> encounters)
    {
        if (encounters == null || encounters.Count == 0)
            return null;

        var totalWeight = 0;
        foreach (var e in encounters)
            totalWeight += e.Weight;

        var roll = _random.Next(totalWeight);
        var cumulative = 0;

        foreach (var encounter in encounters)
        {
            cumulative += encounter.Weight;
            if (roll < cumulative)
                return encounter;
        }

        return encounters[^1];
    }

    private List<GameObject> SpawnEncounter(
        EncounterConfig encounter,
        int startX, int startY,
        Map map,
        Game game,
        GameObject sceneRoot,
        SpatialGrid spatialGrid,
        GameObject player,
        HashSet<(int, int)> occupiedTiles)
    {
        var monsters = new List<GameObject>();

        foreach (var group in encounter.Groups)
        {
            if (!MonsterTemplateLoader.TryGet(group.Id, out var template))
                continue;

            var count = _random.Next(group.Min, group.Max + 1);

            for (int i = 0; i < count; i++)
            {
                var (tileX, tileY) = FindNearbyFloorTile(startX, startY, map, occupiedTiles);
                if (tileX < 0)
                    continue;

                occupiedTiles.Add((tileX, tileY));

                var worldPos = new Vector2(tileX + 0.5f, tileY + 0.5f);
                var monster = MonsterFactory.Create(template, worldPos, game, sceneRoot, spatialGrid, player);
                monsters.Add(monster);
            }
        }

        return monsters;
    }

    private (int x, int y) FindNearbyFloorTile(int centerX, int centerY, Map map, HashSet<(int, int)> occupied)
    {
        // Spiral outward from center
        for (int radius = 0; radius <= 3; radius++)
        {
            for (int dy = -radius; dy <= radius; dy++)
            {
                for (int dx = -radius; dx <= radius; dx++)
                {
                    if (Math.Abs(dx) != radius && Math.Abs(dy) != radius)
                        continue;

                    var x = centerX + dx;
                    var y = centerY + dy;

                    if (x < 0 || x >= map.Width || y < 0 || y >= map.Height)
                        continue;

                    if (!map.IsFloor(x, y))
                        continue;

                    if (occupied.Contains((x, y)))
                        continue;

                    return (x, y);
                }
            }
        }

        return (-1, -1);
    }
}

public class MonsterSpawnConfig
{
    public float Density { get; set; }
    public List<EncounterConfig> Encounters { get; set; } = new();
}

public class EncounterConfig
{
    public int Weight { get; set; }
    public List<MonsterGroupConfig> Groups { get; set; } = new();
}

public class MonsterGroupConfig
{
    public string Id { get; set; }
    public int Min { get; set; } = 1;
    public int Max { get; set; } = 1;
}
```

**Step 2: Verify build**

Run: `dotnet build games/Solocaster/Solocaster.csproj`
Expected: Build succeeded

**Step 3: Commit**

```bash
git add games/Solocaster/Persistence/MonsterSpawner.cs
git commit -m "feat: add MonsterSpawner with encounter group support"
```

---

## Task 18: Update LevelLoader to Parse Monsters

**Files:**
- Modify: `Solocaster/Persistence/LevelLoader.cs`

**Step 1: Add monster config parsing to LevelData**

Update the `LevelData` class:

```csharp
private class LevelData
{
    public required string[] Spritesheets { get; init; }
    public required MapData Map { get; init; }
    public List<EntityData>? Entities { get; init; }
    public MonsterSpawnConfig? Monsters { get; init; }
}
```

**Step 2: Add using statement**

```csharp
using Solocaster.Monsters;
```

**Step 3: Update Level class to hold monsters reference**

Check `Entities/Level.cs` and add if needed:
```csharp
public List<GameObject> Monsters { get; set; } = new();
```

**Step 4: Update LoadFromJson to spawn monsters**

Add monster loading after `LoadEntities`:

```csharp
public static Level LoadFromJson(string path, Game game, GameObject sceneRoot, SpatialGrid spatialGrid, GameObject player)
{
    var levelData = ParseLevelFile(path);
    var spritesheets = LoadSpritesheets(path, game, levelData);
    var doorSprites = BuildDoorSprites(levelData.Map, spritesheets);

    var context = new MapBuildContext
    {
        Game = game,
        SceneRoot = sceneRoot,
        SpatialGrid = spatialGrid,
        SpriteSheets = spritesheets,
        TemplateLoader = TemplateLoader
    };

    var builder = BuilderFactory.Create(levelData.Map, spritesheets, doorSprites);
    var mapResult = builder.Build(context);

    LoadEntities(game, sceneRoot, spatialGrid, levelData);

    var monsters = new List<GameObject>();
    if (levelData.Monsters != null && player != null)
    {
        var spawner = new MonsterSpawner();
        monsters = spawner.Spawn(levelData.Monsters, mapResult.Map, game, sceneRoot, spatialGrid, player);
    }

    return new Level
    {
        Map = mapResult.Map,
        SpriteSheets = spritesheets,
        WallSprites = mapResult.WallSprites,
        DoorSprites = doorSprites,
        Monsters = monsters
    };
}
```

**Note:** The signature now requires `player` parameter. Update callers accordingly.

**Step 5: Verify build**

Run: `dotnet build games/Solocaster/Solocaster.csproj`
Expected: Build may fail if Level.cs or callers need updates

**Step 6: Fix Level.cs if needed**

Add to `Entities/Level.cs`:
```csharp
public List<GameObject> Monsters { get; set; } = new();
```

**Step 7: Fix PlayScene or other callers**

Update `LoadFromJson` call sites to pass the player GameObject.

**Step 8: Commit**

```bash
git add games/Solocaster/Persistence/LevelLoader.cs games/Solocaster/Entities/Level.cs
git commit -m "feat: integrate monster spawning into LevelLoader"
```

---

## Task 19: Load Monster Templates at Startup

**Files:**
- Modify: `Solocaster/SolocasterGame.cs` or appropriate startup location

**Step 1: Find where templates are loaded**

Look for where `EntityTemplateLoader` or `ItemTemplateLoader` is initialized.

**Step 2: Add monster template loading**

```csharp
// In LoadContent or Initialize
var monstersPath = Path.Combine("./data", "templates", "monsters");
MonsterTemplateLoader.LoadAllFromFolder(Path.GetFullPath(monstersPath));
```

**Step 3: Add using**

```csharp
using Solocaster.Monsters;
```

**Step 4: Verify build**

Run: `dotnet build games/Solocaster/Solocaster.csproj`
Expected: Build succeeded

**Step 5: Commit**

```bash
git add games/Solocaster/SolocasterGame.cs
git commit -m "feat: load monster templates at startup"
```

---

## Task 20: Update Level1 JSON with Monster Config

**Files:**
- Modify: `Solocaster/data/levels/level1.json`

**Step 1: Add monsters section**

Add after the `pickupableItems` section:

```json
"monsters": {
  "density": 0.015,
  "encounters": [
    {
      "weight": 70,
      "groups": [
        { "id": "skeleton", "min": 1, "max": 2 }
      ]
    },
    {
      "weight": 30,
      "groups": [
        { "id": "skeleton", "min": 2, "max": 3 }
      ]
    }
  ]
}
```

**Step 2: Commit**

```bash
git add games/Solocaster/data/levels/level1.json
git commit -m "feat: add monster spawning config to level1"
```

---

## Task 21: Integration Test - Run Game

**Step 1: Run the game**

Run: `dotnet run --project games/Solocaster/Solocaster.csproj`

**Step 2: Verify**

Expected behavior:
- Game loads without errors
- Skeletons spawn in the dungeon
- Skeletons display idle animation
- Skeletons face the player (direction changes as you move around them)
- Existing items and decorations still render correctly

**Step 3: Debug any issues**

If issues occur, check:
- Animation JSON files exist and are valid
- Monster template JSON is valid
- Content pipeline has the skeleton textures

**Step 4: Final commit if fixes needed**

```bash
git add -A
git commit -m "fix: resolve monster system integration issues"
```

---

## Summary

This plan implements:
1. **IFrameProvider abstraction** - Decouples billboard rendering from static sprites
2. **Animation system** - AnimationController with directional support and state management
3. **Monster templates** - JSON-defined stats, behavior, and animations
4. **Encounter spawning** - Weighted random groups with min/max counts
5. **Monster AI foundation** - Direction calculation relative to player

The skeleton will idle and face the player. Combat, pathfinding, and state transitions (walk/attack/hit/death) are future work built on this foundation.

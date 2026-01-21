# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build and Run

```bash
dotnet run --project games/Solocaster/Solocaster.csproj
```

## Game Overview

Solocaster is a Wolfenstein-style raycaster RPG with procedurally generated dungeons. It features:
- First-person 3D view using software raycasting
- Inventory system with equipment, backpack, and belt slots
- Character system with races, classes, and action-based stat progression
- Screen-space UI widget framework with drag-drop support

## Controls

- **W/S** - Move forward/backward
- **A/D** - Rotate left/right
- **E** - Open doors, pick up items
- **Tab** - Toggle character panel (inventory + stats)
- **C** - Toggle metrics panel (progress tracking)
- **M** - Toggle minimap
- **Escape** - Exit

---

## Architecture

### Raycasting Pipeline

The rendering uses a rotated coordinate system where the framebuffer is built column-by-column (as rows), then rotated 90 degrees when displayed:

1. `Raycaster.Update()` - Casts rays for each column, renders walls/floor/ceiling/doors to `FrameBuffer`
2. `MapRenderer.UpdateCore()` - Calls raycaster, copies framebuffer to texture
3. `MapRenderer.Render()` - Draws texture rotated 90 degrees to fill screen

The raycaster uses unsafe code with pinned memory and SIMD (AVX2/SSE2) for performance. Wall textures are pre-rotated 90 degrees at load time.

### Key Rendering Components

- **`Raycaster`** - DDA raycasting with distance-based fog shading, z-buffer for sprite depth testing
- **`MapRenderer`** - Component that drives raycasting and renders the framebuffer
- **`BillboardComponent`** - Sprite entities rendered in 3D space with anchor points (Bottom/Center/Top)
- **`MiniMapRenderer`** - Top-down 2D map overlay

---

## UI Widget System

### Widget Hierarchy

```
UI/
├── Widgets/
│   ├── Widget.cs              # Base class: Position, Size, Parent/Children, Update/Render
│   ├── PanelWidget.cs         # Window with background, border, close button, scrolling
│   ├── LabelWidget.cs         # Text display
│   ├── ImageWidget.cs         # Sprite/texture display
│   ├── ButtonWidget.cs        # Clickable button (extends PanelWidget)
│   ├── ItemSlotWidget.cs      # Inventory slot with drag-drop (extends PanelWidget)
│   ├── TooltipWidget.cs       # Hover tooltip (extends PanelWidget)
│   ├── ProgressBarWidget.cs   # Progress bar with fill/border
│   ├── MetricRowWidget.cs     # Label + value row for metrics display
│   └── StatProgressRowWidget.cs # Stat name + progress bar
├── UIService.cs               # Manages root widgets, input routing, tooltip display
├── UITheme.cs                 # Centralized styling loaded from JSON
├── DragDropManager.cs         # Handles item drag-drop between panels
├── CharacterPanel.cs          # Container for StatsPanel + InventoryPanel
├── InventoryPanel.cs          # Equipment slots + backpack grid
├── StatsPanel.cs              # Character info and derived stats
├── BeltPanel.cs               # Quick-access consumable slots
├── PlayerStatusPanel.cs       # Health/mana bars with avatar
└── MetricsPanel.cs            # Scrollable metrics and stat progress display
```

### Widget Features

- **PanelWidget**: Supports optional close button (`ShowCloseButton`), scrollable content (`Scrollable`), configurable padding
- **Scrolling**: Uses scissor rectangles for clipping, `ChildRenderOffset` for scroll position
- **Drag-Drop**: `ItemSlotWidget` fires `OnDragStart`, `DragDropManager` tracks drag state, panels handle drop logic

### UI Theme System

Styling is centralized in `data/ui/theme.json` and loaded via `UITheme.Load()`:

```json
{
  "panel": { "backgroundColor": [20,20,25,240], "borderColor": [100,80,60], "borderWidth": 3, "contentPadding": 20 },
  "button": { "backgroundColor": [60,60,60,230], "borderColor": [100,100,100], "borderWidth": 2 },
  "itemSlot": { "backgroundColor": [30,30,30,200], "borderColor": [70,70,70], "borderWidth": 2 },
  "tooltip": { "backgroundColor": [20,20,25,250], "borderColor": [100,80,60], "borderWidth": 2 }
}
```

Access via `UITheme.Panel`, `UITheme.Button`, `UITheme.ItemSlot`, `UITheme.Tooltip`.

**Note**: Panels with `ShowCloseButton = false` that handle their own layout should set `ContentPadding = 0` to avoid double-offset from `ChildRenderOffset`.

---

## Inventory System

### Core Classes

```
Inventory/
├── StatType.cs          # Enum: Strength, Agility, Vitality, Intelligence, Wisdom, Damage, Defense, MaxHealth, MaxMana
├── ItemType.cs          # Enum: Weapon, Armor, Consumable, Accessory, Misc
├── EquipSlot.cs         # Enum: None, Head, Chest, LeftHand, RightHand, Legs, Neck, LeftRing, RightRing, Belt
├── ItemTemplate.cs      # Item definition: stats, requirements, sprites
├── ItemInstance.cs      # Runtime instance with unique ID and stack count
└── ItemTemplateLoader.cs # Loads templates from data/templates/items/*.json
```

### Components

- **`InventoryComponent`** - Player inventory: equipment slots, backpack (weight-limited), belt (consumables)
- **`PickupableComponent`** - World items that can be picked up

### Item Template JSON

```json
{
  "items": [{
    "id": "iron_sword",
    "name": "Iron Sword",
    "description": "A sturdy iron blade.",
    "iconPath": "misc_items2:sword_iron",
    "worldSpritePath": "misc_items2:sword_ground",
    "worldSpriteScale": 0.35,
    "itemType": "Weapon",
    "equipSlot": "RightHand",
    "weight": 3.0,
    "stackable": false,
    "statModifiers": { "Damage": 8 },
    "requirements": { "Strength": 5 }
  }]
}
```

---

## Character System

### Core Classes

```
Character/
├── Sex.cs                    # Enum: Male, Female
├── RaceTemplate.cs           # Race definition with base stats and action progress multipliers
├── ClassTemplate.cs          # Class definition with stat bonuses
├── CharacterTemplateLoader.cs # Loads from data/templates/character/
└── PlayerMetrics.cs          # Tracks all player actions (combat, movement, social, etc.)
```

### Components

- **`StatsComponent`** - Base stats, equipment bonuses, derived stats, stat progression, metrics tracking

### Action-Based Stat Progression

Stats improve through actions, not XP. Each race defines `actionProgress` multipliers:

```json
{
  "id": "human",
  "name": "Human",
  "baseStats": { "Strength": 10, "Agility": 10, "Vitality": 10, "Intelligence": 10, "Wisdom": 10 },
  "actionProgress": {
    "MeleeAttack": { "Strength": 1.0 },
    "Walking": { "Vitality": 0.05 },
    "SpellCast": { "Intelligence": 0.8, "Wisdom": 0.4 }
  }
}
```

### MetricType Enum

Tracks: `MeleeAttack`, `RangedAttack`, `DamageTaken`, `DamageBlocked`, `EnemyKilled`, `SpellCast`, `MagicDamage`, `Healing`, `Walking`, `Running`, `Hiding`, `Sneaking`, `NPCInteraction`, `ItemBought`, `ItemSold`, `LockPick`, `PotionUsed`, `ScrollUsed`

---

## Level System

Levels are loaded from JSON files in `data/levels/`. Two map types:

- **Static**: Hand-crafted `cells` array
- **Random**: Procedurally generated using `DungeonGenerator`

### Map Cell Values

- `-1` (Floor) - Walkable
- `0+` - Wall with sprite index
- `99` (StartingPosition) - Player spawn
- `101` (DoorVertical) - Door N-S
- `102` (DoorHorizontal) - Door E-W

### Level JSON Structure

```json
{
  "spritesheets": ["dungeon1", "dungeon_doors1"],
  "map": {
    "type": "random",
    "width": 32,
    "height": 32,
    "wallSprites": { "base": {"stone_wall": 1}, "accent": ["mossy_wall"] },
    "doorSprites": ["wooden_door"],
    "decorations": [{ "density": 0.04, "placement": "floor", "items": {"misc_items:barrel": 1} }]
  },
  "entities": [{ "template": "misc_items:torch", "tileX": 5, "tileY": 3 }]
}
```

---

## Data Files Structure

```
data/
├── levels/                    # Level definitions
│   ├── level1.json
│   └── level2.json
├── spritesheets/              # Sprite sheet definitions (reference Content/ textures)
│   ├── dungeon1.json
│   ├── avatars.json
│   └── misc_items2.json
├── templates/
│   ├── items/                 # Item templates
│   │   ├── weapons.json
│   │   ├── armors.json
│   │   ├── consumables.json
│   │   └── accessories.json
│   ├── character/             # Race and class definitions
│   │   ├── races.json
│   │   └── classes.json
│   └── misc_items.json        # World decoration templates
└── ui/
    └── theme.json             # UI styling configuration
```

---

## Entity System

- **`EntityManager`** - Tracks world entities (decorations, pickups)
- **`EntityFactory`** - Creates entities from template definitions
- **`EntityTemplateLoader`** - Loads entity templates from JSON

Entities with `BillboardComponent` are rendered by the raycaster after walls.

---

## Coding Conventions

- Avoid magic strings; use `const string`, `nameof()`, or enums instead
- Use JSON configuration files for data-driven content (items, races, UI themes)
- Components follow the pattern: constructor takes `GameObject owner`, override `UpdateCore()` for logic
- UI widgets use composition (parent/children) and `AddChild()` to build hierarchies

# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build and Run

```bash
dotnet run --project games/Solocaster/Solocaster.csproj
```

## Game Overview

Solocaster is a Wolfenstein-style raycaster game with procedurally generated dungeons. It renders a first-person 3D view using software raycasting onto a framebuffer that gets rotated 90 degrees for display.

## Controls

- **W/S** - Move forward/backward
- **A/D** - Rotate left/right
- **E** - Open doors
- **Escape** - Exit

## Architecture

### Raycasting Pipeline

The rendering uses a rotated coordinate system where the framebuffer is built column-by-column (as rows), then rotated 90 degrees when displayed:

1. `Raycaster.Update()` - Casts rays for each column, renders walls/floor/ceiling/doors to `FrameBuffer`
2. `MapRenderer.UpdateCore()` - Calls raycaster, copies framebuffer to texture
3. `MapRenderer.Render()` - Draws texture rotated 90 degrees to fill screen

The raycaster uses unsafe code with pinned memory and SIMD (AVX2/SSE2) for performance. Wall textures are pre-rotated 90 degrees at load time to match the rendering orientation.

### Key Components

- **`Raycaster`** - DDA raycasting with distance-based fog shading, z-buffer for sprite depth testing
- **`MapRenderer`** - Component that drives raycasting and renders the framebuffer
- **`PlayerBrain`** - WASD movement, collision detection against map, door interaction
- **`BillboardComponent`** - Sprite entities rendered in 3D space with anchor points (Bottom/Center/Top)

### Level System

Levels are loaded from JSON files in `data/levels/`. Two map types:

- **Static**: Hand-crafted `cells` array where values are sprite indices (0+ = wall sprite, -1 = floor, 99 = starting position, 101/102 = doors)
- **Random**: Procedurally generated using `DungeonGenerator` with configurable wall sprites and decorations

### Data Files

```
data/
├── levels/          # Level definitions (JSON)
├── spritesheets/    # Sprite sheet definitions (JSON) - reference Content/ textures
└── templates/       # Entity templates for decorations/items (JSON)
```

**Level JSON structure:**
```json
{
  "spritesheets": ["spritesheet_name"],
  "map": {
    "type": "random|static",
    "wallSprites": { "base": {"sprite_name": weight}, "accent": ["sprite_name"] },
    "doorSprites": ["sprite_name"],
    "decorations": [{ "density": 0.04, "placement": "floor|wall", "items": {"template:name": weight} }]
  },
  "entities": [{ "template": "namespace:name", "tileX": 5, "tileY": 3 }]
}
```

**Template JSON structure:**
```json
{
  "namespace": "template_namespace",
  "items": [{
    "name": "item_name",
    "itemType": "sprite",
    "properties": { "spritesheet": "...", "sprite": "...", "scaleX": 0.5, "anchor": "bottom" }
  }]
}
```

### Entity System

`EntityManager` tracks all world entities (decorations, items). Entities with `BillboardComponent` are rendered by the raycaster after walls. `EntityFactory` creates entities from template definitions.

### Map Cell Values

- `-1` (TileTypes.Floor) - Walkable floor
- `0+` - Wall with sprite index
- `99` (TileTypes.StartingPosition) - Player spawn (converted to floor)
- `101` (TileTypes.DoorVertical) - Door spanning N-S
- `102` (TileTypes.DoorHorizontal) - Door spanning E-W

### Dungeon Generator

`DungeonGenerator/` contains maze generation with:
- Configurable corridor sparseness and dead-end removal
- Room placement via `RoomGenerator`
- Door placement at room entrances
- Tile expansion for wall thickness

# SpriteSheet Editor Tool - Design Document

## Overview

A visual tool for creating and editing spritesheet JSON definition files for the Solo engine. Built with .NET MAUI for cross-platform support.

## Project Location

`/tools/SpriteSheetEditor/`

## Technology Stack

- .NET MAUI for cross-platform UI
- SkiaSharp for canvas rendering (image display, drawing overlays, zoom/pan)
- System.Text.Json for JSON serialization

## Project Structure

```
tools/
└── SpriteSheetEditor/
    ├── SpriteSheetEditor.csproj
    ├── App.xaml / App.xaml.cs
    ├── MainPage.xaml / MainPage.xaml.cs
    ├── Models/
    │   ├── SpriteDefinition.cs      # name, x, y, width, height
    │   └── SpriteSheetDocument.cs   # spriteSheetName, List<SpriteDefinition>, image reference (runtime only)
    ├── ViewModels/
    │   └── MainViewModel.cs         # MVVM state management
    ├── Controls/
    │   └── SpriteCanvas.cs          # Custom SkiaSharp canvas control
    └── Services/
        └── JsonExporter.cs          # Saves to existing format
```

## UI Layout

```
┌─────────────────────────────────────────────────────────────────┐
│  [Open Image] [Open JSON] [Save JSON]  │  Tools: [Select] [Draw] [Grid...]  │
├─────────────────────────────────────────┬───────────────────────┤
│                                         │  Sprite List          │
│                                         │  ┌─────────────────┐  │
│                                         │  │ warrior_male    │  │
│           Canvas                        │  │ cleric_female   │  │
│      (image + sprite overlays)          │  │ > thief_male    │  │
│                                         │  │ mage_female     │  │
│                                         │  └─────────────────┘  │
│                                         ├───────────────────────┤
│                                         │  Properties           │
│                                         │  Name: [thief_male ]  │
│                                         │  X: [512]  Y: [0]     │
│                                         │  W: [256]  H: [256]   │
│                                         │  [Delete Sprite]      │
├─────────────────────────────────────────┴───────────────────────┤
│  Zoom: 100%  │  Image: 1024x1024  │  Sprites: 16                │
└─────────────────────────────────────────────────────────────────┘
```

### Toolbar Actions

- **Open Image**: Load PNG/JPG for editing
- **Open JSON**: Load existing spritesheet definition (prompts for image if not loaded)
- **Save JSON**: Export to existing format
- **Select Tool**: Click sprites to select, drag to move, drag handles to resize
- **Draw Tool**: Click-drag to create new rectangle
- **Grid Button**: Opens dialog for grid generation

### Status Bar

Shows zoom level, image dimensions, sprite count

## Canvas Behavior

### Rendering Layers (bottom to top)

1. Checkerboard background (shows transparency)
2. Loaded image
3. Sprite rectangles (semi-transparent fill + border)
4. Selected sprite highlight (different color, resize handles)
5. Grid overlay (when visible, dashed lines)
6. Drawing preview (when using Draw tool)

### Navigation

- **Mouse wheel**: Zoom in/out (centered on cursor)
- **Middle-click drag**: Pan the canvas
- **Fit button** (or double-click middle button): Reset to fit image in view

### Select Tool Interactions

- **Click on sprite**: Select it (highlights in canvas, shows in properties)
- **Click on empty area**: Deselect
- **Drag sprite**: Move it
- **Drag corner handles**: Resize sprite rectangle
- **Delete key**: Remove selected sprite

### Draw Tool Interactions

- **Click-drag**: Draw rectangle preview
- **Release**: Create sprite with auto-generated name (`{sheetname}_sprite_{N}`)
- **Escape while drawing**: Cancel

### Sprite Visuals

- Unselected: Blue border, 20% blue fill
- Selected: Orange border, 20% orange fill, white corner handles

## Data Model

### Runtime Model

```csharp
public class SpriteDefinition
{
    public string Name { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
}

public class SpriteSheetDocument
{
    public string SpriteSheetName { get; set; }
    public List<SpriteDefinition> Sprites { get; set; }

    // Runtime only (not serialized)
    public SKBitmap LoadedImage { get; set; }
    public string ImageFilePath { get; set; }
}
```

### JSON Output Format

Matches existing Solo engine format:

```json
{
  "spriteSheetName": "avatars",
  "sprites": [
    { "name": "warrior_male", "x": 0, "y": 0, "width": 256, "height": 256 },
    { "name": "cleric_female", "x": 256, "y": 0, "width": 256, "height": 256 }
  ]
}
```

## Grid Generation Dialog

```
┌─────────────────────────────────┐
│  Generate Grid                  │
├─────────────────────────────────┤
│  Columns: [4      ]             │
│  Rows:    [4      ]             │
│                                 │
│  Calculated tile size:          │
│  256 × 256 px                   │
│                                 │
│  Will create 16 sprites         │
│                                 │
│      [Cancel]  [Generate]       │
└─────────────────────────────────┘
```

### With Warning (when dimensions don't divide evenly)

```
┌─────────────────────────────────┐
│  Generate Grid                  │
├─────────────────────────────────┤
│  Columns: [3      ]             │
│  Rows:    [4      ]             │
│                                 │
│  Calculated tile size:          │
│  341 × 256 px                   │
│                                 │
│  ⚠ Image width (1024) does not │
│    divide evenly by 3 columns.  │
│    1 pixel column uncovered.    │
│                                 │
│  Will create 12 sprites         │
│                                 │
│      [Cancel]  [Generate]       │
└─────────────────────────────────┘
```

### Behavior

- User enters number of columns and rows
- Tile size calculated: `width = imageWidth / columns`, `height = imageHeight / rows`
- Preview shows calculated tile dimensions and total sprite count
- On "Generate" click: shows confirmation "This will replace all N existing sprites. Continue?" (if sprites exist)
- Sprites named automatically: `{spriteSheetName}_sprite_0`, `_sprite_1`, etc.
- Numbering: left-to-right, top-to-bottom
- Warning shown if image dimensions don't divide evenly (remainder pixels uncovered)

## Workflows

### Create New Spritesheet

1. Open Image - canvas shows image, spriteSheetName defaults to filename without extension
2. Use Grid to generate initial sprites, or Draw tool to create manually
3. Rename sprites via properties panel
4. Save JSON - file dialog, saves to chosen location

### Edit Existing Spritesheet

1. Open JSON - loads sprite definitions, prompts "Load image file?" with file dialog
2. Sprites displayed as rectangles on canvas (even without image, shown on checkerboard)
3. Select, move, resize, rename, add, delete sprites
4. Save JSON - overwrites or save-as

## Keyboard Shortcuts

| Shortcut | Action |
|----------|--------|
| `Ctrl+O` | Open Image |
| `Ctrl+Shift+O` | Open JSON |
| `Ctrl+S` | Save JSON |
| `Delete` | Remove selected sprite |
| `Escape` | Cancel drawing / deselect |
| `1` | Select tool |
| `2` | Draw tool |
| `Ctrl+G` | Open grid dialog |
| `Ctrl+0` | Fit to window |

## Future Extensibility

The architecture supports these future features without major refactoring:

- **Merge sprites**: Select multiple, merge into one rectangle
- **Animation support**: Extend SpriteDefinition with optional animation data
- **Snap to grid**: Toggle for precise alignment
- **Undo/redo**: Command pattern in ViewModel
- **Copy/paste sprites**: Duplicate definitions
- **Sprite preview**: Show isolated sprite in properties panel

## Dependencies

- `Microsoft.Maui.Controls`
- `SkiaSharp.Views.Maui.Controls`
- `System.Text.Json` (built-in)

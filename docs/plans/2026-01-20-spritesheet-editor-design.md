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
    │   └── SpriteSheetDocument.cs   # spriteSheetName, List<SpriteDefinition>, image reference
    ├── ViewModels/
    │   └── MainViewModel.cs         # MVVM state management
    ├── Controls/
    │   ├── SpriteCanvas.cs          # Custom SkiaSharp canvas control
    │   ├── ImportImagesDialog.xaml  # Dialog for load/import/rearrange
    │   └── GridDialog.xaml          # Dialog for grid generation
    ├── Services/
    │   ├── JsonExporter.cs          # Saves to existing format
    │   ├── BinPacker.cs             # Sprite packing algorithms
    │   └── ImageImporter.cs         # Load/import/rearrange images
    ├── Filters/
    │   └── ColorToTransparentFilter.cs
    └── UndoRedo/
        ├── IUndoableCommand.cs      # Command interface (extends IDisposable)
        ├── UndoRedoManager.cs       # Manages undo/redo stacks
        └── Commands/                # Individual command implementations
```

## UI Layout

```
┌─────────────────────────────────────────────────────────────────┐
│  [File ▼] [Edit ▼] [Sprites ▼] [Filters ▼]  │  Zoom: [+] [-]   │
├─────────────────────────────────────────────┬───────────────────┤
│                                             │  Sprite List      │
│                                             │  ┌─────────────┐  │
│                                             │  │ warrior     │  │
│           Canvas                            │  │ cleric      │  │
│      (image + sprite overlays)              │  │ > thief     │  │
│                                             │  │ mage        │  │
│                                             │  └─────────────┘  │
│                                             ├───────────────────┤
│                                             │  Properties       │
│                                             │  Name: [thief   ] │
│                                             │  X: [512]  Y: [0] │
│                                             │  W: [256]  H: [256]│
│                                             │  [Delete Sprite]  │
├─────────────────────────────────────────────┴───────────────────┤
│  Zoom: 100%  │  Image: 1024x1024  │  Sprites: 16                │
└─────────────────────────────────────────────────────────────────┘
```

## Menu Structure

### File Menu
- **Load Images...** - Load multiple images and pack into spritesheet
- **Save Image...** - Export composite image (disabled when no document)
- **Import Images...** - Append images to existing sheet (disabled when no document)
- **Open JSON...** - Load existing spritesheet definition
- **Save JSON...** - Export to JSON format (disabled when no document)
- **Close Project** - Close current document (disabled when no document)

### Edit Menu
- **Undo** (Ctrl+Z) - Undo last action
- **Redo** (Ctrl+Y) - Redo undone action
- **Fit to window** - Reset zoom to fit image (disabled when no document)
- **Rearrange layout...** - Reorganize sprites with different layout (disabled when < 2 sprites)

### Sprites Menu
- **Select** (1) - Click to select, drag to move, handles to resize (checkmark when active)
- **Draw** (2) - Click-drag to create new rectangle (checkmark when active)

### Filters Menu
- **Color to Transparent** - Pick a color to make transparent

### Status Bar

Shows zoom level, image dimensions, sprite count

## Canvas Behavior

### Rendering Layers (bottom to top)

1. Checkerboard background (clipped to image bounds, shows transparency)
2. Loaded image
3. Sprite rectangles (semi-transparent fill + border)
4. Selected sprite highlight (different color, resize handles)
5. Grid overlay (when visible, dashed lines)
6. Drawing preview (when using Draw tool)

### Navigation

- **Mouse wheel**: Zoom in/out (disabled when no document)
- **Middle mouse button drag**: Pan/scroll the canvas
- **Scrollbars**: Appear when image extends beyond visible area
- **Toolbar buttons**: Zoom in (+) / Zoom out (-)

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

## Packing Layouts

When loading or importing multiple images, three layout algorithms are available:

### Grid Layout
- Places sprites in rows, wrapping when target width exceeded
- Target width calculated as sqrt(total area)
- Best for mixed-size sprites

### Single Column Layout
- Stacks all sprites vertically
- Width equals widest sprite

### Single Row Layout
- Places all sprites horizontally
- Height equals tallest sprite

## Workflows

### Create New Spritesheet from Multiple Images

1. File → Load Images... - Select multiple image files
2. Choose layout (Grid, Column, or Row)
3. Images packed into composite, sprites created from filenames
4. Rename sprites via properties panel if needed
5. Save JSON - file dialog, saves to chosen location

### Create New Spritesheet with Grid

1. File → Load Images... - Select single image
2. Use Grid to generate sprites based on rows/columns
3. Rename sprites via properties panel
4. Save JSON

### Edit Existing Spritesheet

1. Open JSON - loads sprite definitions, prompts "Load image file?" with file dialog
2. Sprites displayed as rectangles on canvas (even without image, shown on checkerboard)
3. Select, move, resize, rename, add, delete sprites
4. Save JSON - overwrites or save-as

### Import Additional Images

1. Have a document already loaded
2. File → Import Images... - Select additional image files
3. All sprites (existing + new) repacked using Grid layout
4. Canvas resized to fit new layout

### Rearrange Existing Sprites

1. Have a document with 2+ sprites
2. Edit → Rearrange layout... - Select new layout
3. Sprites extracted and repacked
4. Canvas resized to fit new layout

## Undo/Redo System

All document modifications are undoable:

- **IUndoableCommand** interface extends IDisposable for proper bitmap cleanup
- **UndoRedoManager** disposes commands when removed from stacks
- Commands store previous state for undo, new state for redo/execute

Commands with bitmap resources (properly disposed):
- LoadImagesCommand (Load Images)
- ImportImagesCommand (Import Images)
- RearrangeLayoutCommand (Rearrange Layout)
- ApplyFilterCommand (Filters)

Commands without resources:
- AddSpriteCommand
- RemoveSpriteCommand
- ModifySpriteCommand
- GenerateGridCommand

## Keyboard Shortcuts

| Shortcut | Action |
|----------|--------|
| `Ctrl+Z` | Undo |
| `Ctrl+Y` | Redo |
| `Delete` | Remove selected sprite |
| `Escape` | Cancel drawing / deselect |
| `1` | Select tool |
| `2` | Draw tool |

## Future Extensibility

The architecture supports these future features without major refactoring:

- **Merge sprites**: Select multiple, merge into one rectangle
- **Animation support**: Extend SpriteDefinition with optional animation data
- **Snap to grid**: Toggle for precise alignment
- **Copy/paste sprites**: Duplicate definitions
- **Sprite preview**: Show isolated sprite in properties panel

## Dependencies

- `Microsoft.Maui.Controls`
- `SkiaSharp.Views.Maui.Controls`
- `System.Text.Json` (built-in)

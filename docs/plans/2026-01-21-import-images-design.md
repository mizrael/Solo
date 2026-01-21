# SpriteSheetEditor: Import Images Feature Design

## Overview

Add functionality to import a group of images and arrange them into a spritesheet using bin packing. Imported images can be of different sizes. The system calculates sprite bounds based on individual image sizes. Filenames (without extensions) become sprite names. Importing replaces the active document entirely.

## User Flow

1. User clicks **File → Import Images...**
2. If there are unsaved changes, show confirmation dialog: *"Import will replace the current document. You have unsaved changes. Continue?"*
3. Multi-file picker opens (PNG, JPG, BMP supported)
4. Import dialog appears showing: file count, padding input (default 0px), OK/Cancel buttons
5. System processes images:
   - Load all selected images
   - Run bin packing algorithm to determine positions
   - Calculate smallest power-of-two canvas that fits the packed result
   - Create composite image with configured padding between sprites
   - Generate sprite definitions using filenames (without extension) as names
6. Replace current document with new spritesheet image and sprite definitions
7. Register as undoable command (can undo back to previous state)

## Bin Packing Algorithm

**Approach: Shelf-based packing with height-sorted input**

1. Sort images by height descending (taller images first produces better packing)
2. Estimate initial canvas size: square root of (sum of all areas + padding), rounded to power-of-two
3. Place rectangles left-to-right on "shelves" (rows):
   - When a rectangle doesn't fit on current shelf, start a new shelf below
   - Track actual used bounds during placement
4. If packing fails (exceeds canvas), double canvas size and retry
5. Final step: find smallest power-of-two dimensions (width × height) that contain all placed sprites

**Padding:** User-configurable (default 0px). Padding is added around each sprite during placement, but sprite bounds in JSON remain the actual image dimensions.

## Data Structures

```csharp
// Input: what needs to be packed
public record PackingItem(string Name, int Width, int Height, SKBitmap Image);

// Output: where each item was placed
public record PackedResult(
    IReadOnlyList<PackedItem> Items,
    int CanvasWidth,
    int CanvasHeight
);

public record PackedItem(string Name, int X, int Y, int Width, int Height, SKBitmap Image);
```

## Implementation Components

### New Files

1. **`Services/BinPacker.cs`**
   - `Pack(IEnumerable<PackingItem> items, int padding)` method
   - Returns `PackedResult` with positions and power-of-two canvas dimensions

2. **`Services/ImageImporter.cs`**
   - `ImportImagesAsync(IEnumerable<string> filePaths, int padding)` method
   - Loads images, calls bin packer, composites final bitmap
   - Returns new `SpriteSheetDocument` with image and sprite definitions

3. **`ImportImagesDialog.xaml/.cs`**
   - Simple dialog showing: file count, padding input (default 0), OK/Cancel buttons

4. **`UndoRedo/Commands/ImportImagesCommand.cs`**
   - Captures previous document state (image + sprites + sheet name)
   - Enables full undo of the import operation

### Modified Files

5. **`MainPage.xaml`**
   - Add "Import Images..." menu item under File menu

6. **`MainPage.xaml.cs`**
   - Handle menu click: check unsaved changes → file picker → dialog → execute import

## Undo/Redo Integration

```csharp
public class ImportImagesCommand : IUndoableCommand
{
    // Captures previous state
    private readonly SKBitmap _previousImage;
    private readonly IReadOnlyList<SpriteDefinition> _previousSprites;
    private readonly string _previousSheetName;

    // New state to apply
    private readonly SKBitmap _newImage;
    private readonly IReadOnlyList<SpriteDefinition> _newSprites;
    private readonly string _newSheetName;

    public string Description => "Import Images";
}
```

## Sprite Naming

Use `Path.GetFileNameWithoutExtension(filePath)` for each imported file.

Example: importing `walk_01.png`, `walk_02.png`, `idle.png` produces sprites named `walk_01`, `walk_02`, `idle`.

# SpriteSheetEditor: Load, Import & Rearrange Images Feature Design

## Overview

Functionality to load, import, and rearrange images in the SpriteSheetEditor:

1. **Load Images** - Replaces the current document with a new spritesheet created from packed images
2. **Import Images** - Appends images to the existing spritesheet, expanding the canvas to the right
3. **Rearrange Layout** - Reorganizes existing sprites with a different layout algorithm

## Menu Structure

### File Menu
- **Load Images...** - Always enabled
- **Save Image...** - Disabled when no document loaded
- **Import Images...** - Disabled when no document loaded
- **Open JSON...** - Always enabled
- **Save JSON...** - Disabled when no document loaded
- **Close Project** - Disabled when no document loaded

### Edit Menu
- **Undo/Redo** - Standard undo/redo
- **Fit to window** - Disabled when no document loaded
- **Rearrange layout...** - Disabled when no document loaded or only 1 sprite

### Sprites Menu
- **Select** - Disabled when no document loaded (checkmark when active, shortcut: 1)
- **Draw** - Disabled when no document loaded (checkmark when active, shortcut: 2)

## User Flow

### Load Images (File → Load Images...)
1. User clicks **File → Load Images...**
2. If there are unsaved changes, show confirmation dialog
3. Multi-file picker opens (PNG, JPG, BMP supported)
4. **Single image**: Load directly without dialog (Grid layout)
5. **Multiple images**: Show layout dialog with options:
   - Layout selection: Grid, Column, or Row
   - Load/Cancel buttons
6. System processes images:
   - Load all selected images
   - Run packing algorithm with selected layout
   - Calculate exact canvas size to fit packed result
   - Create composite image
   - Generate sprite definitions using filenames (without extension) as names
7. Replace current document with new spritesheet
8. Register as undoable command

### Import Images (File → Import Images...)
1. User clicks **File → Import Images...** (disabled if no document loaded)
2. Multi-file picker opens
3. System processes images:
   - Extract existing sprites from current image
   - Load new images from selected files
   - Pack all sprites (existing + new) together using Grid layout
   - Create new composite image
   - Replace all sprite definitions with new positions
4. Register as undoable command

### Rearrange Layout (Edit → Rearrange layout...)
1. User clicks **Edit → Rearrange layout...** (disabled if no document or only 1 sprite)
2. Rearrange dialog appears showing:
   - Sprite count
   - Layout selection: Grid, Column, or Row
   - Apply/Cancel buttons
3. System processes:
   - Extract each sprite's pixels from current image
   - Pack sprites using selected layout
   - Create new composite image
   - Update sprite positions
4. Register as undoable command

## Packing Layouts

### Grid Layout
- Places sprites in rows, wrapping to next row when target width exceeded
- Target width calculated as sqrt(total area)
- Sprites placed left-to-right, top-to-bottom

### Single Column Layout
- Places all sprites vertically, one below another
- Width equals widest sprite

### Single Row Layout
- Places all sprites horizontally, one beside another
- Height equals tallest sprite

## Implementation Components

### Services

**`Services/BinPacker.cs`**
- `PackingLayout` enum: Grid, SingleColumn, SingleRow
- `PackingItem` record: Name, Width, Height, Image
- `PackedItem` record: Name, X, Y, Width, Height, Image
- `PackedResult` record: Items, CanvasWidth, CanvasHeight
- `Pack(items, layout)` - Main packing method

**`Services/ImageImporter.cs`**
- `ImportResult` record: Document, CompositeImage
- `AppendResult` record: AllSprites, Image
- `RearrangeResult` record: Sprites, Image
- `LoadImagesAsync(filePaths, layout)` - Load and pack images into new document
- `AppendImagesAsync(filePaths, existingImage, existingSprites)` - Append and rearrange all using Grid
- `RearrangeLayout(sourceImage, sprites, layout)` - Rearrange existing sprites

### Controls

**`Controls/ImportImagesDialog.xaml/.cs`**
- Reusable dialog for Load and Rearrange operations
- `Show(filePaths, isImportMode)` - For Load Images
- `ShowForRearrange(spriteCount)` - For Rearrange
- `ImportImagesEventArgs`: FilePaths, Layout

### Undo/Redo

**`UndoRedo/IUndoableCommand.cs`**
- Extends `IDisposable` to properly clean up bitmap resources

**`UndoRedo/UndoRedoManager.cs`**
- `DisposeAndClear()` helper to dispose commands when removed from stacks
- Commands disposed when redo stack cleared (new action after undo)
- Commands disposed when `Clear()` called

**Commands with bitmap disposal:**
- `LoadImagesCommand` - For Load Images (full document replace)
- `ImportImagesCommand` - For Import Images (append only)
- `RearrangeLayoutCommand` - For Rearrange Layout
- `ApplyFilterCommand` - For filter operations

### MainPage

**`MainPage.xaml`**
- Two dialog instances: LoadImagesDialog, RearrangeDialog
- Sprites menu button replacing Select/Draw toolbar buttons

**`MainPage.xaml.cs`**
- Menu handlers with proper enable/disable logic
- `OnSpritesClicked` - Shows Sprites menu with checkmarks
- `OnImportImagesClicked` - Directly imports with Grid layout (no dialog)
- `OnRearrangeLayoutClicked` - Shows rearrange dialog
- `OnRearrangeDialogConfirm` - Executes rearrangement

## Sprite Naming

Base name uses `Path.GetFileNameWithoutExtension(filePath)` for each imported file.

Names are guaranteed unique:
- Within a single load operation (e.g., loading "1.png" twice creates "1", "1_1")
- When importing into existing document (e.g., importing "warrior.png" when "warrior" exists creates "warrior_1")

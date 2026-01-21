# SpriteSheetEditor: Load & Import Images Feature Design

## Overview

Add functionality to load and import groups of images into the SpriteSheetEditor:

1. **Load Images** - Replaces the current document with a new spritesheet created from bin-packed images
2. **Import Images** - Appends images to the existing spritesheet, expanding the canvas to the right

## User Flow

### Load Images (File → Load Images...)
1. User clicks **File → Load Images...**
2. If there are unsaved changes, show confirmation dialog
3. Multi-file picker opens (PNG, JPG, BMP supported)
4. Load dialog appears showing: file count, padding input (default 0px), Load/Cancel buttons
5. System processes images:
   - Load all selected images
   - Run bin packing algorithm to determine positions
   - Calculate smallest power-of-two canvas that fits the packed result
   - Create composite image with configured padding between sprites
   - Generate sprite definitions using filenames (without extension) as names
6. Replace current document with new spritesheet
7. Register as undoable command

### Import Images (File → Import Images...)
1. User clicks **File → Import Images...**
2. If no image is loaded, show error "Please load images first"
3. Multi-file picker opens
4. Import dialog appears showing: file count, padding input (default 0px), Import/Cancel buttons
5. System processes images:
   - Pack new images among themselves
   - Position packed block to the right of existing image
   - Expand canvas to power-of-two dimensions
   - Append new sprite definitions
6. Register as undoable command (preserves existing sprites)

## Bin Packing Algorithm

**Approach: Shelf-based packing with height-sorted input**

1. Sort images by height descending
2. Place rectangles left-to-right on "shelves" (rows)
3. Find smallest power-of-two dimensions that contain all sprites

## Implementation Components

### New Files

1. **`Services/BinPacker.cs`** - Shelf-based bin packing with power-of-two sizing
2. **`Services/ImageImporter.cs`** - `LoadImagesAsync` (replace) and `AppendImagesAsync` (append)
3. **`Controls/ImportImagesDialog.xaml/.cs`** - Reusable dialog with configurable title
4. **`UndoRedo/Commands/ImportImagesCommand.cs`** - For Load Images (full document replace)
5. **`UndoRedo/Commands/AppendImagesCommand.cs`** - For Import Images (append only)

### Modified Files

6. **`MainPage.xaml`** - Two dialog instances (LoadImagesDialog, ImportDialog)
7. **`MainPage.xaml.cs`** - Handlers for both operations

## Sprite Naming

Use `Path.GetFileNameWithoutExtension(filePath)` for each imported file.

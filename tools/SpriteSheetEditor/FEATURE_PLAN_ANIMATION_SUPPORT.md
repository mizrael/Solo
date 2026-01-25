# Animation Support for SpriteSheetEditor - Implementation Plan

## Overview

Add animation support to the SpriteSheetEditor tool, enabling users to:
1. Define animations as sequences of sprite frames
2. Preview animations with playback controls (play/pause, FPS, loop)
3. Export animations as individual JSON files

## Export Format (must match exactly)

```json
{
  "animationName": "attack_front",
  "spriteSheetName": "skeleton",
  "fps": 10,
  "frames": [
    { "x": 0, "y": 0, "width": 170, "height": 330 }
  ]
}
```

---

## Implementation Phases

### Phase 1: Data Models

**Create:** `Models/AnimationFrame.cs`
```csharp
public partial class AnimationFrame : ObservableObject
{
    [ObservableProperty]
    private SpriteDefinition _sprite = null!;
}
```

**Create:** `Models/AnimationDefinition.cs`
```csharp
public partial class AnimationDefinition : ObservableObject
{
    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private int _fps = 10;
    [ObservableProperty] private bool _loop = true;
    public ObservableCollection<AnimationFrame> Frames { get; } = [];
}
```

**Modify:** `Models/SpriteSheetDocument.cs`
- Add: `public ObservableCollection<AnimationDefinition> Animations { get; set; } = [];`

---

### Phase 2: ViewModel Extensions

**Modify:** `ViewModels/MainViewModel.cs`

Add properties:
- `SelectedAnimation` (AnimationDefinition?)
- `SelectedFrame` (AnimationFrame?)
- `IsAnimationPlaying` (bool)
- `CurrentPreviewFrameIndex` (int)
- `SelectedSprites` (ObservableCollection<SpriteDefinition>) - for multi-select

Add methods:
- `CreateNewAnimation()`
- `DeleteSelectedAnimation()`
- `AddSelectedSpritesToAnimation()`

---

### Phase 3: Undo/Redo Commands

**Create:** `UndoRedo/Commands/`
- `AddAnimationCommand.cs`
- `RemoveAnimationCommand.cs`
- `AddFramesToAnimationCommand.cs`
- `RemoveFrameCommand.cs`
- `ReorderFrameCommand.cs`
- `ModifyAnimationPropertiesCommand.cs`

---

### Phase 4: Animation Export Service

**Create:** `Services/AnimationExporter.cs`
- `ExportAsync(AnimationDefinition, spriteSheetName, filePath)`
- Uses DTOs for exact JSON format match

---

### Phase 5: Animation Preview Canvas

**Create:** `Controls/AnimationPreviewCanvas.axaml` + `.cs`
- SkiaSharp-based custom control
- Checkerboard background (same pattern as SpriteCanvas)
- Renders current frame from source image
- DispatcherTimer for playback (interval = 1000/fps ms)
- Play/Pause/Stop methods

---

### Phase 6: Animation Panel

**Create:** `Controls/AnimationPanel.axaml` + `.cs`

Layout:
```
┌─────────────────────────────────┐
│ Animations          [+] [-]    │
├─────────────────────────────────┤
│ ▶ walk_cycle                    │
│ ● attack_front (selected)       │
├─────────────────────────────────┤
│ Name: [__________]              │
│ FPS: [10]  ☑ Loop               │
├─────────────────────────────────┤
│ Frames:                         │
│   1. sprite_2                   │
│   2. sprite_5     [Remove]      │
│ [Add Selected Sprites]          │
├─────────────────────────────────┤
│ ┌─────────────────────┐         │
│ │   Preview Canvas    │         │
│ └─────────────────────┘         │
│ [▶ Play]  Frame 3/6             │
├─────────────────────────────────┤
│ [Export Animation JSON]         │
└─────────────────────────────────┘
```

---

### Phase 7: MainWindow Integration

**Modify:** `MainWindow.axaml`
- Add "Animations" menu (New Animation, Export Animation)
- Use TabControl for right panel: "Sprites" tab + "Animations" tab

**Modify:** `MainWindow.axaml.cs`
- Wire up AnimationPanel events
- Handle animation operations through ViewModel + UndoRedo

---

### Phase 8: Multi-Select Support

**Modify:** `Controls/SpriteCanvas.cs`
- Ctrl+Click to toggle selection
- Track selection in `ViewModel.SelectedSprites`
- Render highlight for all selected sprites

---

### Phase 9: Drag-to-Reorder Frames

**Enhance:** `Controls/AnimationPanel.axaml.cs`
- Avalonia drag-drop for frame list reordering

---

## Implementation Order

1. Phase 1: Data Models (foundation)
2. Phase 2: ViewModel Extensions
3. Phase 3: Undo/Redo Commands
4. Phase 4: Animation Export Service
5. Phase 5: Animation Preview Canvas
6. Phase 6: Animation Panel
7. Phase 7: MainWindow Integration
8. Phase 8: Multi-Select Support
9. Phase 9: Drag-to-Reorder

---

## Critical Files

| File | Action |
|------|--------|
| `Models/AnimationFrame.cs` | Create |
| `Models/AnimationDefinition.cs` | Create |
| `Models/SpriteSheetDocument.cs` | Modify |
| `ViewModels/MainViewModel.cs` | Modify |
| `UndoRedo/Commands/*.cs` | Create (6 files) |
| `Services/AnimationExporter.cs` | Create |
| `Controls/AnimationPreviewCanvas.axaml(.cs)` | Create |
| `Controls/AnimationPanel.axaml(.cs)` | Create |
| `Controls/SpriteCanvas.cs` | Modify |
| `MainWindow.axaml` | Modify |
| `MainWindow.axaml.cs` | Modify |

---

## Key Design Decisions

1. **Frame references sprites by object** - If sprite moves, animation updates automatically
2. **Sprite deletion cleanup** - Remove frames referencing deleted sprites
3. **Serialization** - Store sprite name in frame, resolve by name on load
4. **TabControl UI** - Cleaner than cramming both panels together

---

## Verification

1. Build: `dotnet build tools/SpriteSheetEditor/SpriteSheetEditor.csproj`
2. Run: `dotnet run --project tools/SpriteSheetEditor/SpriteSheetEditor.csproj`
3. Test workflow:
   - Load a spritesheet image
   - Define sprite bounding boxes
   - Create an animation
   - Add sprites to animation (multi-select with Ctrl+Click)
   - Reorder frames
   - Preview with play/pause
   - Adjust FPS and loop settings
   - Export animation JSON
   - Verify exported JSON matches expected format
4. Test undo/redo for all animation operations
5. Test edge cases: delete sprite that's in animation, empty animation export

# Color to Transparent Filter - Design

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Add a filter to replace pixels of a specified color (with tolerance) with transparent pixels.

**Architecture:** Inline filter panel with live preview, accessed via Filters dropdown menu.

**Tech Stack:** SkiaSharp for image manipulation, MAUI for UI controls.

---

## UI Layout & Access

The toolbar gets a new **"Filters" dropdown button** after the existing buttons. Clicking it shows a menu with "Color to Transparent" as the first option (extensible for future filters).

When activated, a **filter panel** appears between the toolbar and the canvas area. This panel contains:
- **Color section**: A color preview swatch, hex input field (#FF00FF default), and a "Pick" button that enables eyedropper mode on the canvas
- **Tolerance section**: A slider (0-100%) with a numeric label showing current value
- **Action buttons**: "Apply" and "Cancel" on the right side

The canvas shows the **live preview** of the filtered image. The original image is preserved in memory until the user clicks Apply or Cancel.

While the filter panel is open:
- Clicking on the canvas with eyedropper mode active picks that pixel's color
- Other tools (Select, Draw) are disabled
- The sprite list and properties panel remain visible but non-interactive

---

## Filter Algorithm & Tolerance

The **color distance calculation** uses Euclidean distance in RGB space:

```
distance = sqrt((r1-r2)² + (g1-g2)² + (b1-b2)²)
maxDistance = sqrt(255² + 255² + 255²) ≈ 441.67
```

The **tolerance slider (0-100%)** maps to this distance:
- 0% = exact match only (distance = 0)
- 50% = matches colors within ~220 RGB distance
- 100% = matches everything

For each pixel in the image:
1. Calculate RGB distance from the target color
2. If `distance <= (tolerance * maxDistance)`, set pixel alpha to 0 (transparent)
3. Otherwise, keep pixel unchanged

**Performance**: For live preview, the filter runs on every slider/color change. Sprite sheets are typically small (1024x1024 or less), so processing is fast. Operates on an `SKBitmap` copy to preserve the original.

**Edge case**: Existing transparent pixels are left unchanged.

---

## Implementation Structure

**New files:**

1. **`Filters/ColorFilter.cs`** - Static class with the filter logic:
   - `ApplyColorToTransparent(SKBitmap source, SKColor targetColor, float tolerance)` → returns new `SKBitmap`
   - `CalculateColorDistance(SKColor a, SKColor b)` → returns float (0-1 normalized)

2. **`Controls/FilterPanel.xaml`** - The inline filter panel control:
   - Color swatch, hex Entry, "Pick" button
   - Tolerance Slider (0-100) with Label
   - Apply/Cancel buttons
   - Exposes events: `ApplyClicked`, `CancelClicked`, `SettingsChanged`

**Modified files:**

1. **`MainPage.xaml`** - Add Filters dropdown to toolbar, add FilterPanel (initially hidden) between toolbar and canvas area

2. **`MainPage.xaml.cs`** - Handle filter activation, eyedropper mode, preview updates, apply/cancel logic

3. **`MainViewModel.cs`** - Add `OriginalImage` property to store backup during filtering, add `IsFilterActive` state

**Flow:**
1. User clicks Filters → Color to Transparent
2. Panel appears, `OriginalImage` = current image copy
3. Any setting change triggers preview: apply filter to copy, display result
4. Apply: keep filtered image, hide panel
5. Cancel: restore `OriginalImage`, hide panel

---

## Testing

**Unit tests in `SpriteSheetEditor.Tests/Filters/ColorFilterTests.cs`:**

1. `ApplyColorToTransparent_ExactMatch_MakesPixelTransparent`
2. `ApplyColorToTransparent_NoMatch_LeavesPixelUnchanged`
3. `ApplyColorToTransparent_WithTolerance_MatchesSimilarColors`
4. `ApplyColorToTransparent_PreservesExistingTransparency`
5. `CalculateColorDistance_IdenticalColors_ReturnsZero`
6. `CalculateColorDistance_OppositeColors_ReturnsOne`
7. `ApplyColorToTransparent_ReturnsNewBitmap_OriginalUnchanged`

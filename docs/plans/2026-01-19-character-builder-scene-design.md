# Character Builder Scene Design

## Overview

A wizard-style character creation scene that appears before PlayScene, allowing players to customize their character before starting the game.

## Wizard Flow

**6 Steps:**
1. **Race** - Select from Human, Dwarf, Elf
2. **Class** - Select from Warrior, Cleric, Thief, Mage
3. **Sex** - Select Male or Female
4. **Avatar** - Pick from available avatars for the race/class/sex combo
5. **Name** - Text input + Random name button
6. **Summary** - Review all choices, see final stats, Start Game button

## Navigation

- Step indicators at top: `Race • Class • Sex • Avatar • Name • Summary`
- Current step highlighted, completed steps clickable to jump back
- Back/Next buttons at bottom
- Back disabled on step 1, Next disabled until selection made
- On Summary, Next becomes "Start Game"

## Global Character Data

**CharacterData class:**
```csharp
public class CharacterData
{
    public string RaceId { get; set; }      // e.g., "human"
    public string ClassId { get; set; }     // e.g., "warrior"
    public Sex Sex { get; set; }
    public string AvatarSpriteName { get; set; }  // e.g., "human_warrior_male"
    public string Name { get; set; }
}
```

**GameState static class:**
```csharp
public static class GameState
{
    public static CharacterData? CurrentCharacter { get; set; }
    public static void Clear() => CurrentCharacter = null;
}
```

PlayScene reads from `GameState.CurrentCharacter` to initialize player.

## UI Layout

```
┌─────────────────────────────────────────────────────────┐
│  Step Indicators (top)                                  │
│  [Race] • [Class] • [Sex] • [Avatar] • [Name] • [Summary]│
├─────────────────────────────────────────────────────────┤
│                                                         │
│  Step Content Area (varies by step)                     │
│                                                         │
│  ┌─────────────┐  ┌───────────────────────────────┐    │
│  │ Selection   │  │ Detail Panel                  │    │
│  │ List        │  │                               │    │
│  │             │  │ Name: Human                   │    │
│  │ > Human     │  │ Description: Versatile and...│    │
│  │   Dwarf     │  │                               │    │
│  │   Elf       │  │ Stat Bonuses:                 │    │
│  │             │  │ STR: +0  AGI: +0  VIT: +0     │    │
│  │             │  │ INT: +0  WIS: +0              │    │
│  └─────────────┘  └───────────────────────────────┘    │
│                                                         │
├─────────────────────────────────────────────────────────┤
│  Navigation (bottom)                                    │
│              [ Back ]              [ Next ]             │
└─────────────────────────────────────────────────────────┘
```

**Step-specific content:**
- **Race/Class**: Vertical list on left + detail panel on right (description, stat bonuses/maluses)
- **Sex**: Two large buttons (Male / Female) centered
- **Avatar**: Grid of clickable avatar thumbnails filtered by race/class/sex
- **Name**: Text input field + "Random" button centered
- **Summary**: Avatar image + all choices and final combined stats

## Input Handling

**Mouse:**
- Click list items, step indicators, buttons, avatar thumbnails
- Click text field to focus
- Mouse wheel scrolls lists

**Keyboard:**
- Up/Down: Navigate selection lists
- Left/Right: Navigate avatar grid
- Enter: Confirm and advance (Next)
- Escape/Backspace: Go back
- Tab: Cycle interactive elements
- Typing: Input to text field on Name step

**Scrolling:**
- Lists use `Scrollable = true` on panel
- Keyboard navigation auto-scrolls to keep selection visible

## Random Name Generation

Simple prefix + suffix combination:

```csharp
Prefixes: "Gor", "Eld", "Thar", "Mor", "Fen", "Bri", "Ash", "Kael", "Vor", "Zyn"
Suffixes: "ian", "ius", "ak", "en", "ara", "is", "oth", "rim", "wyn", "ax"
```

Static `NameGenerator.Generate(Sex sex)` method returns combined name.
Single universal pool initially; race-specific names can be added later.

## New Widgets

1. **TextInputWidget** - Text field for name entry
   - Properties: Text, MaxLength, Font, PlaceholderText
   - Handles keyboard input, cursor blinking, character limit

2. **SelectableListWidget** - Vertical selectable list
   - Properties: Items, SelectedIndex, Font
   - Events: OnSelectionChanged
   - Keyboard nav, mouse click, highlights selection, scrollable

3. **StepIndicatorWidget** - Horizontal step progress
   - Properties: Steps, CurrentStep, CompletedSteps
   - Clickable completed steps, highlights current

## File Structure

**New files:**
```
Character/
└── CharacterData.cs

State/
└── GameState.cs

Scenes/
└── CharacterBuilderScene.cs

UI/CharacterBuilder/
├── CharacterBuilderPanel.cs
├── RaceStepPanel.cs
├── ClassStepPanel.cs
├── SexStepPanel.cs
├── AvatarStepPanel.cs
├── NameStepPanel.cs
└── SummaryStepPanel.cs

UI/Widgets/
├── TextInputWidget.cs
├── SelectableListWidget.cs
└── StepIndicatorWidget.cs

Utilities/
└── NameGenerator.cs
```

**Modified files:**
- `SceneNames.cs` - Add CharacterBuilder constant
- `SolocasterGame.cs` - Register scene, set as initial
- `PlayScene.cs` - Read from GameState.CurrentCharacter

## Visual Style

- Solid dark background (matching UI theme)
- Main panel centered, approximately 600x500 pixels
- Uses existing UITheme for colors/borders
- Artwork to be added later as polish

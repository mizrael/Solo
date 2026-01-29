# Human Skill Customization Design

## Overview

Make humans more appealing by allowing them to distribute skill points during character creation, rather than having flat 1.0 effectiveness across all skills.

## Data Model Changes

Add two new fields to `RaceTemplate`:

```csharp
public int SkillPoints { get; set; } = 0;        // Points to distribute (0 = skip step)
public float PointBonus { get; set; } = 0.1f;    // Bonus per point (0.1 = +10%)
```

In `races.json`, Human gets:
```json
"skillPoints": 3,
"pointBonus": 0.1
```

Other races omit these fields (defaulting to 0 points, skipping the step).

`CharacterData` gets a new property:
```csharp
public Dictionary<Skills, int> SkillPointAllocations { get; set; } = new();
```

### Effectiveness Formula

```
finalBonus = raceBase * classBonus * (1 + allocatedPoints * pointBonus)
```

Example - Human Thief with 3 points in Sneaking:
- Race: 1.0 × Class: 1.3 × Allocation: (1 + 3×0.1) = 1.3
- Final: 1.0 × 1.3 × 1.3 = **1.69**

## Wizard Flow

The Skills step appears after Class, only when `race.SkillPoints > 0`.

Updated flow:
1. Race
2. Class
3. **Skills** *(conditional - only for races with skillPoints > 0)*
4. Sex
5. Avatar
6. Name
7. Summary

When a race has `SkillPoints = 0`, the Skills step is hidden from the step indicator and the flow skips directly from Class to Sex.

## UI Layout

```
┌─────────────────────────────────────────────────────┐
│  Distribute Skill Points (3 remaining)              │
├─────────────────────────────────────────────────────┤
│                                                     │
│  Sneaking       [-]  0  [+]     (+0%)              │
│  Negotiation    [-]  0  [+]     (+0%)              │
│  Persuasion     [-]  0  [+]     (+0%)              │
│  Lockpicking    [-]  0  [+]     (+0%)              │
│                                                     │
├─────────────────────────────────────────────────────┤
│              [ Back ]         [ Next ]              │
└─────────────────────────────────────────────────────┘
```

### Behavior

- Header shows remaining points to spend
- [-] button disabled when skill has 0 points
- [+] button disabled when no points remaining
- Each row shows current allocation and resulting bonus (e.g., "+20%")
- Next button enabled even with unspent points (player can skip customization)
- Keyboard: Up/Down to select skill row, Left/Right to adjust points

## File Changes

### Modified Files

1. `Character/RaceTemplate.cs` - Add `SkillPoints`, `PointBonus` properties
2. `Character/CharacterTemplateLoader.cs` - Parse new fields from JSON
3. `Character/CharacterData.cs` - Add `SkillPointAllocations` dictionary
4. `data/templates/character/races.json` - Add config for Human, update description
5. `UI/CharacterBuilder/CharacterBuilderPanel.cs` - Insert Skills step, handle conditional skip

### New Files

1. `UI/CharacterBuilder/SkillsStepPanel.cs` - The step panel with skill rows
2. `UI/Widgets/PointDistributorWidget.cs` - Reusable [-] value [+] row widget

## Human Description Update

Update from:
```
"Versatile and adaptable, humans excel at whatever path they choose."
```

To:
```
"Versatile and adaptable, humans can specialize their skills to match their chosen path."
```

# Character Builder Scene Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Implement a 6-step wizard for character creation (Race → Class → Sex → Avatar → Name → Summary) that appears before PlayScene.

**Architecture:** New scene with panel-based UI using existing widget framework. Global GameState stores character choices for use by PlayScene and future save/load. Three new reusable widgets: TextInputWidget, SelectableListWidget, StepIndicatorWidget.

**Tech Stack:** MonoGame/XNA, C#, existing Solo engine and Solocaster UI widget framework.

---

## Phase 1: Foundation (GameState & Data)

### Task 1: Create CharacterData class

**Files:**
- Create: `games/Solocaster/Character/CharacterData.cs`

**Step 1: Create the data class**

```csharp
namespace Solocaster.Character;

public class CharacterData
{
    public string RaceId { get; set; } = "human";
    public string ClassId { get; set; } = "warrior";
    public Sex Sex { get; set; } = Sex.Male;
    public string AvatarSpriteName { get; set; } = "human_warrior_male";
    public string Name { get; set; } = "Adventurer";
}
```

**Step 2: Build to verify compilation**

Run: `dotnet build games/Solocaster/Solocaster.csproj`
Expected: Build succeeded

**Step 3: Commit**

```bash
git add games/Solocaster/Character/CharacterData.cs
git commit -m "feat: add CharacterData class for character creation choices"
```

---

### Task 2: Create GameState static class

**Files:**
- Create: `games/Solocaster/State/GameState.cs`

**Step 1: Create the static state class**

```csharp
using Solocaster.Character;

namespace Solocaster.State;

public static class GameState
{
    public static CharacterData? CurrentCharacter { get; set; }

    public static void Clear()
    {
        CurrentCharacter = null;
    }

    public static void EnsureCharacter()
    {
        CurrentCharacter ??= new CharacterData();
    }
}
```

**Step 2: Build to verify compilation**

Run: `dotnet build games/Solocaster/Solocaster.csproj`
Expected: Build succeeded

**Step 3: Commit**

```bash
git add games/Solocaster/State/GameState.cs
git commit -m "feat: add GameState for global character data storage"
```

---

### Task 3: Update PlayScene to use GameState

**Files:**
- Modify: `games/Solocaster/Scenes/PlayScene.cs`

**Step 1: Add using statement and update SetCharacter call**

Replace the hardcoded character setup:
```csharp
// Old:
statsComponent.SetCharacter("human", "warrior", Sex.Male);

// New:
using Solocaster.State;
// ...
GameState.EnsureCharacter();
var character = GameState.CurrentCharacter!;
statsComponent.SetCharacter(character.RaceId, character.ClassId, character.Sex);
statsComponent.Name = character.Name;
```

**Step 2: Build and run to verify game still works**

Run: `dotnet build games/Solocaster/Solocaster.csproj`
Then run the game manually to verify it loads with default character.
Expected: Game runs normally with default Human Warrior Male named "Adventurer"

**Step 3: Commit**

```bash
git add games/Solocaster/Scenes/PlayScene.cs
git commit -m "feat: PlayScene now reads character from GameState"
```

---

## Phase 2: New Widgets

### Task 4: Create StepIndicatorWidget

**Files:**
- Create: `games/Solocaster/UI/Widgets/StepIndicatorWidget.cs`

**Step 1: Create the widget**

```csharp
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Solocaster.UI.Widgets;

public class StepIndicatorWidget : Widget
{
    private static Texture2D? _pixelTexture;

    public StepIndicatorWidget()
    {
    }

    public List<string> Steps { get; set; } = new();
    public int CurrentStep { get; set; }
    public HashSet<int> CompletedSteps { get; set; } = new();
    public SpriteFont? Font { get; set; }
    public Color ActiveColor { get; set; } = new Color(200, 180, 140);
    public Color CompletedColor { get; set; } = new Color(150, 150, 150);
    public Color InactiveColor { get; set; } = new Color(80, 80, 80);
    public Color SeparatorColor { get; set; } = new Color(100, 100, 100);

    public event Action<int>? OnStepClicked;

    private static Texture2D GetPixelTexture(GraphicsDevice graphicsDevice)
    {
        if (_pixelTexture == null)
        {
            _pixelTexture = new Texture2D(graphicsDevice, 1, 1);
            _pixelTexture.SetData(new[] { Color.White });
        }
        return _pixelTexture;
    }

    protected override void RenderCore(SpriteBatch spriteBatch)
    {
        if (Font == null || Steps.Count == 0)
            return;

        float totalWidth = 0;
        var stepWidths = new List<float>();

        foreach (var step in Steps)
        {
            var width = Font.MeasureString(step).X;
            stepWidths.Add(width);
            totalWidth += width;
        }

        float separatorWidth = 30;
        totalWidth += (Steps.Count - 1) * separatorWidth;

        float startX = ScreenPosition.X + (Size.X - totalWidth) / 2;
        float y = ScreenPosition.Y + (Size.Y - Font.LineSpacing) / 2;

        for (int i = 0; i < Steps.Count; i++)
        {
            Color color;
            if (i == CurrentStep)
                color = ActiveColor;
            else if (CompletedSteps.Contains(i))
                color = CompletedColor;
            else
                color = InactiveColor;

            spriteBatch.DrawString(Font, Steps[i], new Vector2(startX, y), color);
            startX += stepWidths[i];

            if (i < Steps.Count - 1)
            {
                var dotX = startX + separatorWidth / 2 - 2;
                var dotY = y + Font.LineSpacing / 2 - 2;
                var pixel = GetPixelTexture(spriteBatch.GraphicsDevice);
                spriteBatch.Draw(pixel, new Rectangle((int)dotX, (int)dotY, 4, 4), SeparatorColor);
                startX += separatorWidth;
            }
        }
    }

    protected override void OnMouseClick(Point mousePosition)
    {
        if (Font == null || Steps.Count == 0)
            return;

        float totalWidth = 0;
        var stepWidths = new List<float>();

        foreach (var step in Steps)
        {
            var width = Font.MeasureString(step).X;
            stepWidths.Add(width);
            totalWidth += width;
        }

        float separatorWidth = 30;
        totalWidth += (Steps.Count - 1) * separatorWidth;

        float startX = ScreenPosition.X + (Size.X - totalWidth) / 2;
        float y = ScreenPosition.Y;

        for (int i = 0; i < Steps.Count; i++)
        {
            var stepBounds = new Rectangle((int)startX, (int)y, (int)stepWidths[i], (int)Size.Y);
            if (stepBounds.Contains(mousePosition) && CompletedSteps.Contains(i))
            {
                OnStepClicked?.Invoke(i);
                return;
            }
            startX += stepWidths[i] + separatorWidth;
        }
    }
}
```

**Step 2: Build to verify compilation**

Run: `dotnet build games/Solocaster/Solocaster.csproj`
Expected: Build succeeded

**Step 3: Commit**

```bash
git add games/Solocaster/UI/Widgets/StepIndicatorWidget.cs
git commit -m "feat: add StepIndicatorWidget for wizard progress display"
```

---

### Task 5: Create SelectableListWidget

**Files:**
- Create: `games/Solocaster/UI/Widgets/SelectableListWidget.cs`

**Step 1: Create the widget**

```csharp
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Solocaster.UI.Widgets;

public class SelectableListWidget : PanelWidget
{
    private static Texture2D? _pixelTexture;
    private int _hoveredIndex = -1;

    public SelectableListWidget()
    {
        ShowCloseButton = false;
        Scrollable = true;
        ContentPadding = 0;
    }

    public List<string> Items { get; set; } = new();
    public int SelectedIndex { get; set; } = -1;
    public SpriteFont? Font { get; set; }
    public int ItemHeight { get; set; } = 30;
    public int ItemPadding { get; set; } = 8;
    public Color SelectedColor { get; set; } = new Color(80, 70, 50);
    public Color HoverColor { get; set; } = new Color(60, 60, 60);
    public Color TextColor { get; set; } = Color.White;
    public Color SelectedTextColor { get; set; } = new Color(220, 200, 160);

    public event Action<int>? OnSelectionChanged;

    private static Texture2D GetPixelTexture(GraphicsDevice graphicsDevice)
    {
        if (_pixelTexture == null)
        {
            _pixelTexture = new Texture2D(graphicsDevice, 1, 1);
            _pixelTexture.SetData(new[] { Color.White });
        }
        return _pixelTexture;
    }

    public void SelectNext()
    {
        if (Items.Count == 0) return;
        SelectedIndex = (SelectedIndex + 1) % Items.Count;
        OnSelectionChanged?.Invoke(SelectedIndex);
        EnsureSelectedVisible();
    }

    public void SelectPrevious()
    {
        if (Items.Count == 0) return;
        SelectedIndex = SelectedIndex <= 0 ? Items.Count - 1 : SelectedIndex - 1;
        OnSelectionChanged?.Invoke(SelectedIndex);
        EnsureSelectedVisible();
    }

    private void EnsureSelectedVisible()
    {
        if (SelectedIndex < 0) return;

        float itemY = SelectedIndex * ItemHeight;
        float visibleHeight = Size.Y - BorderWidth * 2;

        if (itemY < ScrollOffset)
            ScrollOffset = itemY;
        else if (itemY + ItemHeight > ScrollOffset + visibleHeight)
            ScrollOffset = itemY + ItemHeight - visibleHeight;
    }

    protected override void UpdateCore(GameTime gameTime, MouseState mouseState, MouseState previousMouseState)
    {
        base.UpdateCore(gameTime, mouseState, previousMouseState);

        _hoveredIndex = -1;
        var mousePoint = new Point(mouseState.X, mouseState.Y);

        if (Bounds.Contains(mousePoint))
        {
            float relativeY = mouseState.Y - ScreenPosition.Y - BorderWidth + ScrollOffset;
            int index = (int)(relativeY / ItemHeight);
            if (index >= 0 && index < Items.Count)
                _hoveredIndex = index;
        }

        if (mouseState.LeftButton == ButtonState.Pressed &&
            previousMouseState.LeftButton == ButtonState.Released &&
            _hoveredIndex >= 0)
        {
            SelectedIndex = _hoveredIndex;
            OnSelectionChanged?.Invoke(SelectedIndex);
        }
    }

    public override void Render(SpriteBatch spriteBatch)
    {
        if (!Visible)
            return;

        // Render panel background/border
        var pixel = GetPixelTexture(spriteBatch.GraphicsDevice);
        spriteBatch.Draw(pixel, Bounds, BackgroundColor);

        if (BorderWidth > 0)
        {
            var bounds = Bounds;
            spriteBatch.Draw(pixel, new Rectangle(bounds.X, bounds.Y, bounds.Width, BorderWidth), BorderColor);
            spriteBatch.Draw(pixel, new Rectangle(bounds.X, bounds.Bottom - BorderWidth, bounds.Width, BorderWidth), BorderColor);
            spriteBatch.Draw(pixel, new Rectangle(bounds.X, bounds.Y, BorderWidth, bounds.Height), BorderColor);
            spriteBatch.Draw(pixel, new Rectangle(bounds.Right - BorderWidth, bounds.Y, BorderWidth, bounds.Height), BorderColor);
        }

        if (Font == null || Items.Count == 0)
            return;

        // Set up scissor for clipping
        var originalScissor = spriteBatch.GraphicsDevice.ScissorRectangle;
        var originalRasterizer = spriteBatch.GraphicsDevice.RasterizerState;

        spriteBatch.End();

        var contentBounds = new Rectangle(
            Bounds.X + BorderWidth,
            Bounds.Y + BorderWidth,
            Bounds.Width - BorderWidth * 2,
            Bounds.Height - BorderWidth * 2
        );

        var rasterizerState = new RasterizerState { ScissorTestEnable = true };
        spriteBatch.GraphicsDevice.ScissorRectangle = contentBounds;

        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, rasterizerState);

        float y = ScreenPosition.Y + BorderWidth - ScrollOffset;

        for (int i = 0; i < Items.Count; i++)
        {
            var itemBounds = new Rectangle(
                (int)ScreenPosition.X + BorderWidth,
                (int)y,
                (int)Size.X - BorderWidth * 2,
                ItemHeight
            );

            // Draw selection/hover background
            if (i == SelectedIndex)
                spriteBatch.Draw(pixel, itemBounds, SelectedColor);
            else if (i == _hoveredIndex)
                spriteBatch.Draw(pixel, itemBounds, HoverColor);

            // Draw text
            var textColor = i == SelectedIndex ? SelectedTextColor : TextColor;
            var textPos = new Vector2(itemBounds.X + ItemPadding, itemBounds.Y + (ItemHeight - Font.LineSpacing) / 2);
            spriteBatch.DrawString(Font, Items[i], textPos, textColor);

            y += ItemHeight;
        }

        spriteBatch.End();

        spriteBatch.GraphicsDevice.ScissorRectangle = originalScissor;
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, originalRasterizer);
    }
}
```

**Step 2: Build to verify compilation**

Run: `dotnet build games/Solocaster/Solocaster.csproj`
Expected: Build succeeded

**Step 3: Commit**

```bash
git add games/Solocaster/UI/Widgets/SelectableListWidget.cs
git commit -m "feat: add SelectableListWidget for wizard selection lists"
```

---

### Task 6: Create TextInputWidget

**Files:**
- Create: `games/Solocaster/UI/Widgets/TextInputWidget.cs`

**Step 1: Create the widget**

```csharp
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Solocaster.UI.Widgets;

public class TextInputWidget : PanelWidget
{
    private static Texture2D? _pixelTexture;
    private double _cursorBlinkTimer;
    private bool _cursorVisible = true;
    private const double CursorBlinkRate = 0.5;
    private KeyboardState _previousKeyboardState;

    public TextInputWidget()
    {
        ShowCloseButton = false;
        ContentPadding = 0;
        BackgroundColor = UITheme.ItemSlot.BackgroundColor;
        BorderColor = UITheme.ItemSlot.BorderColor;
        BorderWidth = UITheme.ItemSlot.BorderWidth;
    }

    public string Text { get; set; } = string.Empty;
    public int MaxLength { get; set; } = 20;
    public SpriteFont? Font { get; set; }
    public Color TextColor { get; set; } = Color.White;
    public Color PlaceholderColor { get; set; } = new Color(100, 100, 100);
    public Color CursorColor { get; set; } = new Color(200, 180, 140);
    public string PlaceholderText { get; set; } = "Enter name...";
    public bool IsFocused { get; set; } = true;
    public int Padding { get; set; } = 8;

    public event Action<string>? OnTextChanged;

    private static Texture2D GetPixelTexture(GraphicsDevice graphicsDevice)
    {
        if (_pixelTexture == null)
        {
            _pixelTexture = new Texture2D(graphicsDevice, 1, 1);
            _pixelTexture.SetData(new[] { Color.White });
        }
        return _pixelTexture;
    }

    protected override void UpdateCore(GameTime gameTime, MouseState mouseState, MouseState previousMouseState)
    {
        base.UpdateCore(gameTime, mouseState, previousMouseState);

        if (!IsFocused)
            return;

        // Cursor blink
        _cursorBlinkTimer += gameTime.ElapsedGameTime.TotalSeconds;
        if (_cursorBlinkTimer >= CursorBlinkRate)
        {
            _cursorBlinkTimer = 0;
            _cursorVisible = !_cursorVisible;
        }

        // Handle keyboard input
        var keyboardState = Keyboard.GetState();
        var pressedKeys = keyboardState.GetPressedKeys();

        foreach (var key in pressedKeys)
        {
            if (!_previousKeyboardState.IsKeyDown(key))
            {
                HandleKeyPress(key, keyboardState);
            }
        }

        _previousKeyboardState = keyboardState;
    }

    private void HandleKeyPress(Keys key, KeyboardState keyboardState)
    {
        bool shift = keyboardState.IsKeyDown(Keys.LeftShift) || keyboardState.IsKeyDown(Keys.RightShift);

        if (key == Keys.Back && Text.Length > 0)
        {
            Text = Text[..^1];
            OnTextChanged?.Invoke(Text);
            _cursorVisible = true;
            _cursorBlinkTimer = 0;
        }
        else if (key == Keys.Delete)
        {
            Text = string.Empty;
            OnTextChanged?.Invoke(Text);
        }
        else if (Text.Length < MaxLength)
        {
            char? c = KeyToChar(key, shift);
            if (c.HasValue)
            {
                Text += c.Value;
                OnTextChanged?.Invoke(Text);
                _cursorVisible = true;
                _cursorBlinkTimer = 0;
            }
        }
    }

    private static char? KeyToChar(Keys key, bool shift)
    {
        // Letters
        if (key >= Keys.A && key <= Keys.Z)
        {
            char c = (char)('a' + (key - Keys.A));
            return shift ? char.ToUpper(c) : c;
        }

        // Numbers
        if (key >= Keys.D0 && key <= Keys.D9 && !shift)
            return (char)('0' + (key - Keys.D0));

        // Space
        if (key == Keys.Space)
            return ' ';

        // Common punctuation
        if (key == Keys.OemMinus)
            return shift ? '_' : '-';
        if (key == Keys.OemPeriod)
            return '.';

        return null;
    }

    protected override void RenderCore(SpriteBatch spriteBatch)
    {
        base.RenderCore(spriteBatch);

        if (Font == null)
            return;

        var pixel = GetPixelTexture(spriteBatch.GraphicsDevice);
        var textY = ScreenPosition.Y + (Size.Y - Font.LineSpacing) / 2;
        var textX = ScreenPosition.X + Padding;

        if (string.IsNullOrEmpty(Text))
        {
            // Draw placeholder
            spriteBatch.DrawString(Font, PlaceholderText, new Vector2(textX, textY), PlaceholderColor);
        }
        else
        {
            // Draw text
            spriteBatch.DrawString(Font, Text, new Vector2(textX, textY), TextColor);
        }

        // Draw cursor
        if (IsFocused && _cursorVisible)
        {
            var textWidth = string.IsNullOrEmpty(Text) ? 0 : Font.MeasureString(Text).X;
            var cursorX = textX + textWidth + 2;
            spriteBatch.Draw(pixel, new Rectangle((int)cursorX, (int)textY, 2, Font.LineSpacing), CursorColor);
        }
    }
}
```

**Step 2: Build to verify compilation**

Run: `dotnet build games/Solocaster/Solocaster.csproj`
Expected: Build succeeded

**Step 3: Commit**

```bash
git add games/Solocaster/UI/Widgets/TextInputWidget.cs
git commit -m "feat: add TextInputWidget for name input"
```

---

### Task 7: Create NameGenerator utility

**Files:**
- Create: `games/Solocaster/Utilities/NameGenerator.cs`

**Step 1: Create the name generator**

```csharp
using System;
using Solocaster.Character;

namespace Solocaster.Utilities;

public static class NameGenerator
{
    private static readonly Random _random = new();

    private static readonly string[] MalePrefixes =
    {
        "Gor", "Thar", "Mor", "Kael", "Vor", "Drak", "Bran", "Arn", "Fen", "Grim",
        "Hal", "Jor", "Kern", "Lor", "Mag", "Nar", "Orm", "Rath", "Sven", "Tor"
    };

    private static readonly string[] FemalePrefixes =
    {
        "Ael", "Bri", "Cyr", "Ela", "Fae", "Gwen", "Ivy", "Kira", "Lyr", "Mira",
        "Nyx", "Ora", "Ria", "Sera", "Tia", "Val", "Wren", "Yara", "Zara", "Luna"
    };

    private static readonly string[] MaleSuffixes =
    {
        "ian", "ius", "ak", "en", "or", "us", "ax", "rim", "don", "ric",
        "mar", "gar", "dan", "ven", "ron", "thos", "mir", "nar", "zan", "dur"
    };

    private static readonly string[] FemaleSuffixes =
    {
        "ara", "ena", "ia", "wyn", "is", "elle", "ina", "ora", "ess", "ani",
        "lia", "rie", "dra", "tha", "va", "na", "ra", "sa", "la", "ryn"
    };

    public static string Generate(Sex sex)
    {
        var prefixes = sex == Sex.Male ? MalePrefixes : FemalePrefixes;
        var suffixes = sex == Sex.Male ? MaleSuffixes : FemaleSuffixes;

        var prefix = prefixes[_random.Next(prefixes.Length)];
        var suffix = suffixes[_random.Next(suffixes.Length)];

        return prefix + suffix;
    }
}
```

**Step 2: Build to verify compilation**

Run: `dotnet build games/Solocaster/Solocaster.csproj`
Expected: Build succeeded

**Step 3: Commit**

```bash
git add games/Solocaster/Utilities/NameGenerator.cs
git commit -m "feat: add NameGenerator for random fantasy names"
```

---

## Phase 3: Character Builder Scene

### Task 8: Add scene name constant

**Files:**
- Modify: `games/Solocaster/Scenes/SceneNames.cs`

**Step 1: Add CharacterBuilder constant**

```csharp
namespace Solocaster.Scenes;

public static class SceneNames
{
    public const string CharacterBuilder = "CharacterBuilder";
    public const string Play = "Play";
}
```

**Step 2: Build to verify compilation**

Run: `dotnet build games/Solocaster/Solocaster.csproj`
Expected: Build succeeded

**Step 3: Commit**

```bash
git add games/Solocaster/Scenes/SceneNames.cs
git commit -m "feat: add CharacterBuilder scene name constant"
```

---

### Task 9: Create CharacterBuilderScene (skeleton)

**Files:**
- Create: `games/Solocaster/Scenes/CharacterBuilderScene.cs`

**Step 1: Create the scene skeleton**

```csharp
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Solo;
using Solo.Services;
using Solocaster.Character;
using Solocaster.State;
using Solocaster.UI;

namespace Solocaster.Scenes;

public class CharacterBuilderScene : Scene
{
    private UIService? _uiService;
    private SpriteFont? _font;

    public CharacterBuilderScene(Game game) : base(game)
    {
    }

    protected override void EnterCore()
    {
        _uiService = GameServicesManager.Instance.GetRequired<UIService>();
        _uiService.ClearWidgets();

        _font = Game.Content.Load<SpriteFont>("Font");

        UITheme.Load("./data/ui/theme.json");
        CharacterTemplateLoader.LoadAll("./data/templates/character/");

        GameState.EnsureCharacter();

        // TODO: Create CharacterBuilderPanel and add to UIService
    }
}
```

**Step 2: Build to verify compilation**

Run: `dotnet build games/Solocaster/Solocaster.csproj`
Expected: Build succeeded

**Step 3: Commit**

```bash
git add games/Solocaster/Scenes/CharacterBuilderScene.cs
git commit -m "feat: add CharacterBuilderScene skeleton"
```

---

### Task 10: Register scene and set as initial

**Files:**
- Modify: `games/Solocaster/SolocasterGame.cs`

**Step 1: Find and update LoadContent to register CharacterBuilderScene and set it as initial**

Add the scene registration and change the initial scene. Look for where PlayScene is registered and add CharacterBuilderScene before it, then change `SetScene` to use CharacterBuilder.

```csharp
// Add using:
using Solocaster.Scenes;

// In LoadContent, add:
sceneManager.AddScene(SceneNames.CharacterBuilder, new CharacterBuilderScene(this));

// Change initial scene:
sceneManager.SetScene(SceneNames.CharacterBuilder);
```

**Step 2: Build and run to verify scene loads**

Run: `dotnet build games/Solocaster/Solocaster.csproj`
Then run the game to verify it shows a blank dark screen (CharacterBuilderScene with no UI yet).
Expected: Game launches to empty CharacterBuilder scene

**Step 3: Commit**

```bash
git add games/Solocaster/SolocasterGame.cs
git commit -m "feat: register CharacterBuilderScene as initial scene"
```

---

### Task 11: Create CharacterBuilderPanel (main container)

**Files:**
- Create: `games/Solocaster/UI/CharacterBuilder/CharacterBuilderPanel.cs`

**Step 1: Create the main panel that manages wizard steps**

```csharp
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Solo.Services;
using Solocaster.Character;
using Solocaster.State;
using Solocaster.UI.Widgets;

namespace Solocaster.UI.CharacterBuilder;

public class CharacterBuilderPanel : PanelWidget
{
    private const int PanelWidth = 600;
    private const int PanelHeight = 500;

    private readonly SpriteFont _font;
    private readonly Game _game;

    private readonly StepIndicatorWidget _stepIndicator;
    private readonly ButtonWidget _backButton;
    private readonly ButtonWidget _nextButton;
    private readonly PanelWidget _contentPanel;

    private readonly List<string> _stepNames = new() { "Race", "Class", "Sex", "Avatar", "Name", "Summary" };
    private int _currentStep;
    private readonly HashSet<int> _completedSteps = new();

    private Widget? _currentStepContent;
    private KeyboardState _previousKeyboardState;

    public event Action? OnStartGame;

    public CharacterBuilderPanel(SpriteFont font, Game game)
    {
        _font = font;
        _game = game;

        ShowCloseButton = false;
        BackgroundColor = UITheme.Panel.BackgroundColor;
        BorderColor = UITheme.Panel.BorderColor;
        BorderWidth = UITheme.Panel.BorderWidth;
        ContentPadding = 0;

        Size = new Vector2(PanelWidth, PanelHeight);

        // Step indicator at top
        _stepIndicator = new StepIndicatorWidget
        {
            Steps = _stepNames,
            CurrentStep = 0,
            Font = font,
            Position = new Vector2(0, 16),
            Size = new Vector2(PanelWidth, 30)
        };
        _stepIndicator.OnStepClicked += OnStepIndicatorClicked;
        AddChild(_stepIndicator);

        // Content panel in middle
        _contentPanel = new PanelWidget
        {
            ShowCloseButton = false,
            BackgroundColor = Color.Transparent,
            BorderWidth = 0,
            ContentPadding = 0,
            Position = new Vector2(20, 60),
            Size = new Vector2(PanelWidth - 40, PanelHeight - 130)
        };
        AddChild(_contentPanel);

        // Navigation buttons at bottom
        _backButton = new ButtonWidget
        {
            Text = "Back",
            Font = font,
            Position = new Vector2(PanelWidth / 2 - 120, PanelHeight - 50),
            Size = new Vector2(100, 35),
            Enabled = false
        };
        _backButton.OnClick += OnBackClicked;
        AddChild(_backButton);

        _nextButton = new ButtonWidget
        {
            Text = "Next",
            Font = font,
            Position = new Vector2(PanelWidth / 2 + 20, PanelHeight - 50),
            Size = new Vector2(100, 35)
        };
        _nextButton.OnClick += OnNextClicked;
        AddChild(_nextButton);

        LoadStep(0);
    }

    public void CenterOnScreen(int screenWidth, int screenHeight)
    {
        Position = new Vector2(
            (screenWidth - Size.X) / 2,
            (screenHeight - Size.Y) / 2
        );
    }

    private void OnStepIndicatorClicked(int step)
    {
        if (_completedSteps.Contains(step))
            LoadStep(step);
    }

    private void OnBackClicked()
    {
        if (_currentStep > 0)
            LoadStep(_currentStep - 1);
    }

    private void OnNextClicked()
    {
        if (_currentStep == 5) // Summary step
        {
            OnStartGame?.Invoke();
            return;
        }

        _completedSteps.Add(_currentStep);
        LoadStep(_currentStep + 1);
    }

    private void LoadStep(int step)
    {
        _currentStep = step;
        _stepIndicator.CurrentStep = step;
        _stepIndicator.CompletedSteps = _completedSteps;

        _backButton.Enabled = step > 0;
        _nextButton.Text = step == 5 ? "Start Game" : "Next";

        // Clear previous content
        if (_currentStepContent != null)
        {
            _contentPanel.RemoveChild(_currentStepContent);
            _currentStepContent = null;
        }

        // Load new step content
        _currentStepContent = CreateStepContent(step);
        if (_currentStepContent != null)
        {
            _contentPanel.AddChild(_currentStepContent);
        }

        UpdateNextButtonState();
    }

    private Widget? CreateStepContent(int step)
    {
        var contentSize = _contentPanel.Size;

        return step switch
        {
            0 => new RaceStepPanel(_font, contentSize, OnSelectionChanged),
            1 => new ClassStepPanel(_font, contentSize, OnSelectionChanged),
            2 => new SexStepPanel(_font, _game, contentSize, OnSelectionChanged),
            3 => new AvatarStepPanel(_font, _game, contentSize, OnSelectionChanged),
            4 => new NameStepPanel(_font, contentSize, OnSelectionChanged),
            5 => new SummaryStepPanel(_font, _game, contentSize),
            _ => null
        };
    }

    private void OnSelectionChanged()
    {
        UpdateNextButtonState();
    }

    private void UpdateNextButtonState()
    {
        var character = GameState.CurrentCharacter!;

        bool canProceed = _currentStep switch
        {
            0 => !string.IsNullOrEmpty(character.RaceId),
            1 => !string.IsNullOrEmpty(character.ClassId),
            2 => true, // Sex always has a value
            3 => !string.IsNullOrEmpty(character.AvatarSpriteName),
            4 => !string.IsNullOrEmpty(character.Name) && character.Name.Length >= 2,
            5 => true, // Summary always allows proceeding
            _ => false
        };

        _nextButton.Enabled = canProceed;
    }

    protected override void UpdateCore(GameTime gameTime, MouseState mouseState, MouseState previousMouseState)
    {
        base.UpdateCore(gameTime, mouseState, previousMouseState);

        var keyboardState = Keyboard.GetState();

        // Enter to proceed
        if (keyboardState.IsKeyDown(Keys.Enter) && !_previousKeyboardState.IsKeyDown(Keys.Enter))
        {
            if (_nextButton.Enabled)
                OnNextClicked();
        }

        // Escape to go back
        if (keyboardState.IsKeyDown(Keys.Escape) && !_previousKeyboardState.IsKeyDown(Keys.Escape))
        {
            if (_currentStep > 0)
                OnBackClicked();
        }

        _previousKeyboardState = keyboardState;
    }
}
```

**Step 2: Build to verify compilation (will fail - step panels don't exist yet)**

Run: `dotnet build games/Solocaster/Solocaster.csproj`
Expected: Build errors for missing step panel classes - this is expected, we'll create them next

**Step 3: Commit (partial, with TODOs)**

We'll commit after creating the step panels.

---

### Task 12: Create RaceStepPanel

**Files:**
- Create: `games/Solocaster/UI/CharacterBuilder/RaceStepPanel.cs`

**Step 1: Create the race selection panel**

```csharp
using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Solocaster.Character;
using Solocaster.Inventory;
using Solocaster.State;
using Solocaster.UI.Widgets;

namespace Solocaster.UI.CharacterBuilder;

public class RaceStepPanel : Widget
{
    private readonly SelectableListWidget _raceList;
    private readonly PanelWidget _detailPanel;
    private readonly LabelWidget _nameLabel;
    private readonly LabelWidget _descriptionLabel;
    private readonly LabelWidget _bonusesLabel;
    private readonly Action _onSelectionChanged;

    private readonly RaceTemplate[] _races;

    public RaceStepPanel(SpriteFont font, Vector2 size, Action onSelectionChanged)
    {
        _onSelectionChanged = onSelectionChanged;
        Size = size;

        _races = CharacterTemplateLoader.GetAllRaces().ToArray();

        // Race list on left
        _raceList = new SelectableListWidget
        {
            Items = _races.Select(r => r.Name).ToList(),
            Font = font,
            Position = new Vector2(0, 0),
            Size = new Vector2(150, size.Y),
            BackgroundColor = new Color(30, 30, 35, 200),
            BorderColor = UITheme.Panel.BorderColor,
            BorderWidth = 2
        };
        _raceList.OnSelectionChanged += OnRaceSelected;
        AddChild(_raceList);

        // Detail panel on right
        _detailPanel = new PanelWidget
        {
            ShowCloseButton = false,
            BackgroundColor = new Color(30, 30, 35, 200),
            BorderColor = UITheme.Panel.BorderColor,
            BorderWidth = 2,
            ContentPadding = 0,
            Position = new Vector2(170, 0),
            Size = new Vector2(size.X - 170, size.Y)
        };
        AddChild(_detailPanel);

        int padding = 16;
        int y = padding;

        _nameLabel = new LabelWidget
        {
            Font = font,
            TextColor = new Color(220, 200, 160),
            Position = new Vector2(padding, y),
            Size = new Vector2(_detailPanel.Size.X - padding * 2, 30)
        };
        _detailPanel.AddChild(_nameLabel);
        y += 35;

        _descriptionLabel = new LabelWidget
        {
            Font = font,
            TextColor = Color.LightGray,
            Position = new Vector2(padding, y),
            Size = new Vector2(_detailPanel.Size.X - padding * 2, 80)
        };
        _detailPanel.AddChild(_descriptionLabel);
        y += 90;

        _bonusesLabel = new LabelWidget
        {
            Font = font,
            TextColor = Color.White,
            Position = new Vector2(padding, y),
            Size = new Vector2(_detailPanel.Size.X - padding * 2, 150)
        };
        _detailPanel.AddChild(_bonusesLabel);

        // Select current race or default to first
        var character = GameState.CurrentCharacter!;
        int index = Array.FindIndex(_races, r => r.Id == character.RaceId);
        _raceList.SelectedIndex = index >= 0 ? index : 0;
        UpdateDetails(_raceList.SelectedIndex);
    }

    private void OnRaceSelected(int index)
    {
        if (index >= 0 && index < _races.Length)
        {
            GameState.CurrentCharacter!.RaceId = _races[index].Id;
            UpdateDetails(index);
            _onSelectionChanged();
        }
    }

    private void UpdateDetails(int index)
    {
        if (index < 0 || index >= _races.Length)
            return;

        var race = _races[index];
        _nameLabel.Text = race.Name;
        _descriptionLabel.Text = race.Description;

        var bonuses = race.StatBonuses
            .Where(kv => kv.Value != 0)
            .Select(kv => $"{kv.Key}: {(kv.Value >= 0 ? "+" : "")}{kv.Value}")
            .ToList();

        _bonusesLabel.Text = bonuses.Count > 0
            ? "Stat Bonuses:\n" + string.Join("\n", bonuses)
            : "Stat Bonuses: None";
    }

    protected override void UpdateCore(GameTime gameTime, MouseState mouseState, MouseState previousMouseState)
    {
        base.UpdateCore(gameTime, mouseState, previousMouseState);

        var keyboardState = Keyboard.GetState();
        var prevKeyboardState = _previousKeyboardState;

        if (keyboardState.IsKeyDown(Keys.Up) && !prevKeyboardState.IsKeyDown(Keys.Up))
            _raceList.SelectPrevious();
        if (keyboardState.IsKeyDown(Keys.Down) && !prevKeyboardState.IsKeyDown(Keys.Down))
            _raceList.SelectNext();

        _previousKeyboardState = keyboardState;
    }

    private KeyboardState _previousKeyboardState;
}
```

**Step 2: Build to verify compilation (will still fail - other panels missing)**

Run: `dotnet build games/Solocaster/Solocaster.csproj`
Expected: Build errors for missing step panel classes

---

### Task 13: Create ClassStepPanel

**Files:**
- Create: `games/Solocaster/UI/CharacterBuilder/ClassStepPanel.cs`

**Step 1: Create the class selection panel (similar structure to RaceStepPanel)**

```csharp
using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Solocaster.Character;
using Solocaster.Inventory;
using Solocaster.State;
using Solocaster.UI.Widgets;

namespace Solocaster.UI.CharacterBuilder;

public class ClassStepPanel : Widget
{
    private readonly SelectableListWidget _classList;
    private readonly PanelWidget _detailPanel;
    private readonly LabelWidget _nameLabel;
    private readonly LabelWidget _descriptionLabel;
    private readonly LabelWidget _bonusesLabel;
    private readonly Action _onSelectionChanged;

    private readonly ClassTemplate[] _classes;
    private KeyboardState _previousKeyboardState;

    public ClassStepPanel(SpriteFont font, Vector2 size, Action onSelectionChanged)
    {
        _onSelectionChanged = onSelectionChanged;
        Size = size;

        _classes = CharacterTemplateLoader.GetAllClasses().ToArray();

        _classList = new SelectableListWidget
        {
            Items = _classes.Select(c => c.Name).ToList(),
            Font = font,
            Position = new Vector2(0, 0),
            Size = new Vector2(150, size.Y),
            BackgroundColor = new Color(30, 30, 35, 200),
            BorderColor = UITheme.Panel.BorderColor,
            BorderWidth = 2
        };
        _classList.OnSelectionChanged += OnClassSelected;
        AddChild(_classList);

        _detailPanel = new PanelWidget
        {
            ShowCloseButton = false,
            BackgroundColor = new Color(30, 30, 35, 200),
            BorderColor = UITheme.Panel.BorderColor,
            BorderWidth = 2,
            ContentPadding = 0,
            Position = new Vector2(170, 0),
            Size = new Vector2(size.X - 170, size.Y)
        };
        AddChild(_detailPanel);

        int padding = 16;
        int y = padding;

        _nameLabel = new LabelWidget
        {
            Font = font,
            TextColor = new Color(220, 200, 160),
            Position = new Vector2(padding, y),
            Size = new Vector2(_detailPanel.Size.X - padding * 2, 30)
        };
        _detailPanel.AddChild(_nameLabel);
        y += 35;

        _descriptionLabel = new LabelWidget
        {
            Font = font,
            TextColor = Color.LightGray,
            Position = new Vector2(padding, y),
            Size = new Vector2(_detailPanel.Size.X - padding * 2, 80)
        };
        _detailPanel.AddChild(_descriptionLabel);
        y += 90;

        _bonusesLabel = new LabelWidget
        {
            Font = font,
            TextColor = Color.White,
            Position = new Vector2(padding, y),
            Size = new Vector2(_detailPanel.Size.X - padding * 2, 150)
        };
        _detailPanel.AddChild(_bonusesLabel);

        var character = GameState.CurrentCharacter!;
        int index = Array.FindIndex(_classes, c => c.Id == character.ClassId);
        _classList.SelectedIndex = index >= 0 ? index : 0;
        UpdateDetails(_classList.SelectedIndex);
    }

    private void OnClassSelected(int index)
    {
        if (index >= 0 && index < _classes.Length)
        {
            GameState.CurrentCharacter!.ClassId = _classes[index].Id;
            UpdateDetails(index);
            _onSelectionChanged();
        }
    }

    private void UpdateDetails(int index)
    {
        if (index < 0 || index >= _classes.Length)
            return;

        var cls = _classes[index];
        _nameLabel.Text = cls.Name;
        _descriptionLabel.Text = cls.Description;

        var bonuses = cls.StatBonuses
            .Where(kv => kv.Value != 0)
            .Select(kv => $"{kv.Key}: {(kv.Value >= 0 ? "+" : "")}{kv.Value}")
            .ToList();

        _bonusesLabel.Text = bonuses.Count > 0
            ? "Stat Bonuses:\n" + string.Join("\n", bonuses)
            : "Stat Bonuses: None";
    }

    protected override void UpdateCore(GameTime gameTime, MouseState mouseState, MouseState previousMouseState)
    {
        base.UpdateCore(gameTime, mouseState, previousMouseState);

        var keyboardState = Keyboard.GetState();

        if (keyboardState.IsKeyDown(Keys.Up) && !_previousKeyboardState.IsKeyDown(Keys.Up))
            _classList.SelectPrevious();
        if (keyboardState.IsKeyDown(Keys.Down) && !_previousKeyboardState.IsKeyDown(Keys.Down))
            _classList.SelectNext();

        _previousKeyboardState = keyboardState;
    }
}
```

---

### Task 14: Create SexStepPanel

**Files:**
- Create: `games/Solocaster/UI/CharacterBuilder/SexStepPanel.cs`

**Step 1: Create the sex selection panel**

```csharp
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Solocaster.Character;
using Solocaster.State;
using Solocaster.UI.Widgets;

namespace Solocaster.UI.CharacterBuilder;

public class SexStepPanel : Widget
{
    private readonly ButtonWidget _maleButton;
    private readonly ButtonWidget _femaleButton;
    private readonly Action _onSelectionChanged;
    private KeyboardState _previousKeyboardState;

    private readonly Color _selectedColor = new Color(80, 70, 50);
    private readonly Color _normalColor;

    public SexStepPanel(SpriteFont font, Game game, Vector2 size, Action onSelectionChanged)
    {
        _onSelectionChanged = onSelectionChanged;
        Size = size;

        _normalColor = UITheme.Button.BackgroundColor;

        float buttonWidth = 150;
        float buttonHeight = 60;
        float spacing = 40;
        float totalWidth = buttonWidth * 2 + spacing;
        float startX = (size.X - totalWidth) / 2;
        float y = (size.Y - buttonHeight) / 2;

        _maleButton = new ButtonWidget
        {
            Text = "Male",
            Font = font,
            Position = new Vector2(startX, y),
            Size = new Vector2(buttonWidth, buttonHeight)
        };
        _maleButton.OnClick += () => SelectSex(Sex.Male);
        AddChild(_maleButton);

        _femaleButton = new ButtonWidget
        {
            Text = "Female",
            Font = font,
            Position = new Vector2(startX + buttonWidth + spacing, y),
            Size = new Vector2(buttonWidth, buttonHeight)
        };
        _femaleButton.OnClick += () => SelectSex(Sex.Female);
        AddChild(_femaleButton);

        UpdateButtonStates();
    }

    private void SelectSex(Sex sex)
    {
        GameState.CurrentCharacter!.Sex = sex;
        UpdateButtonStates();
        _onSelectionChanged();
    }

    private void UpdateButtonStates()
    {
        var currentSex = GameState.CurrentCharacter!.Sex;
        _maleButton.BackgroundColor = currentSex == Sex.Male ? _selectedColor : _normalColor;
        _femaleButton.BackgroundColor = currentSex == Sex.Female ? _selectedColor : _normalColor;
    }

    protected override void UpdateCore(GameTime gameTime, MouseState mouseState, MouseState previousMouseState)
    {
        base.UpdateCore(gameTime, mouseState, previousMouseState);

        var keyboardState = Keyboard.GetState();

        if ((keyboardState.IsKeyDown(Keys.Left) && !_previousKeyboardState.IsKeyDown(Keys.Left)) ||
            (keyboardState.IsKeyDown(Keys.Right) && !_previousKeyboardState.IsKeyDown(Keys.Right)))
        {
            var currentSex = GameState.CurrentCharacter!.Sex;
            SelectSex(currentSex == Sex.Male ? Sex.Female : Sex.Male);
        }

        _previousKeyboardState = keyboardState;
    }
}
```

---

### Task 15: Create AvatarStepPanel

**Files:**
- Create: `games/Solocaster/UI/CharacterBuilder/AvatarStepPanel.cs`

**Step 1: Create the avatar selection panel**

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Solo.Assets.Loaders;
using Solocaster.State;
using Solocaster.UI.Widgets;

namespace Solocaster.UI.CharacterBuilder;

public class AvatarStepPanel : Widget
{
    private readonly SpriteFont _font;
    private readonly Game _game;
    private readonly Action _onSelectionChanged;
    private readonly List<ImageWidget> _avatarWidgets = new();
    private readonly List<string> _avatarNames = new();
    private int _selectedIndex;
    private KeyboardState _previousKeyboardState;

    private const int AvatarSize = 80;
    private const int AvatarPadding = 10;
    private const int Columns = 4;

    public AvatarStepPanel(SpriteFont font, Game game, Vector2 size, Action onSelectionChanged)
    {
        _font = font;
        _game = game;
        _onSelectionChanged = onSelectionChanged;
        Size = size;

        LoadAvatars();
    }

    private void LoadAvatars()
    {
        // Clear previous
        foreach (var widget in _avatarWidgets)
            RemoveChild(widget);
        _avatarWidgets.Clear();
        _avatarNames.Clear();

        var character = GameState.CurrentCharacter!;
        var prefix = $"{character.RaceId}_{character.ClassId}_{character.Sex.ToString().ToLower()}";

        try
        {
            var spriteSheet = SpriteSheetLoader.Get("avatars", _game);
            var matchingSprites = spriteSheet.GetAllSpriteNames()
                .Where(name => name.StartsWith(prefix))
                .ToList();

            if (matchingSprites.Count == 0)
            {
                // Fallback: try to find any avatar for this race/class
                var fallbackPrefix = $"{character.RaceId}_{character.ClassId}";
                matchingSprites = spriteSheet.GetAllSpriteNames()
                    .Where(name => name.StartsWith(fallbackPrefix))
                    .ToList();
            }

            float totalWidth = Columns * (AvatarSize + AvatarPadding) - AvatarPadding;
            float startX = (Size.X - totalWidth) / 2;
            float startY = 20;

            for (int i = 0; i < matchingSprites.Count; i++)
            {
                var spriteName = matchingSprites[i];
                var sprite = spriteSheet.Get(spriteName);

                int col = i % Columns;
                int row = i / Columns;

                var avatarWidget = new ImageWidget
                {
                    Texture = sprite.Texture,
                    SourceRectangle = sprite.Bounds,
                    ScaleToFit = true,
                    Position = new Vector2(
                        startX + col * (AvatarSize + AvatarPadding),
                        startY + row * (AvatarSize + AvatarPadding)
                    ),
                    Size = new Vector2(AvatarSize, AvatarSize)
                };

                _avatarWidgets.Add(avatarWidget);
                _avatarNames.Add(spriteName);
                AddChild(avatarWidget);
            }

            // Select current avatar or first one
            _selectedIndex = _avatarNames.IndexOf(character.AvatarSpriteName);
            if (_selectedIndex < 0 && _avatarNames.Count > 0)
            {
                _selectedIndex = 0;
                character.AvatarSpriteName = _avatarNames[0];
            }
        }
        catch
        {
            // Handle missing spritesheet gracefully
        }
    }

    private void SelectAvatar(int index)
    {
        if (index >= 0 && index < _avatarNames.Count)
        {
            _selectedIndex = index;
            GameState.CurrentCharacter!.AvatarSpriteName = _avatarNames[index];
            _onSelectionChanged();
        }
    }

    protected override void UpdateCore(GameTime gameTime, MouseState mouseState, MouseState previousMouseState)
    {
        base.UpdateCore(gameTime, mouseState, previousMouseState);

        // Handle click on avatar
        if (mouseState.LeftButton == ButtonState.Pressed &&
            previousMouseState.LeftButton == ButtonState.Released)
        {
            var mousePoint = new Point(mouseState.X, mouseState.Y);
            for (int i = 0; i < _avatarWidgets.Count; i++)
            {
                if (_avatarWidgets[i].Bounds.Contains(mousePoint))
                {
                    SelectAvatar(i);
                    break;
                }
            }
        }

        // Keyboard navigation
        var keyboardState = Keyboard.GetState();

        if (keyboardState.IsKeyDown(Keys.Left) && !_previousKeyboardState.IsKeyDown(Keys.Left))
            SelectAvatar(Math.Max(0, _selectedIndex - 1));
        if (keyboardState.IsKeyDown(Keys.Right) && !_previousKeyboardState.IsKeyDown(Keys.Right))
            SelectAvatar(Math.Min(_avatarNames.Count - 1, _selectedIndex + 1));
        if (keyboardState.IsKeyDown(Keys.Up) && !_previousKeyboardState.IsKeyDown(Keys.Up))
            SelectAvatar(Math.Max(0, _selectedIndex - Columns));
        if (keyboardState.IsKeyDown(Keys.Down) && !_previousKeyboardState.IsKeyDown(Keys.Down))
            SelectAvatar(Math.Min(_avatarNames.Count - 1, _selectedIndex + Columns));

        _previousKeyboardState = keyboardState;
    }

    protected override void RenderCore(SpriteBatch spriteBatch)
    {
        base.RenderCore(spriteBatch);

        // Draw selection highlight
        if (_selectedIndex >= 0 && _selectedIndex < _avatarWidgets.Count)
        {
            var selected = _avatarWidgets[_selectedIndex];
            var bounds = selected.Bounds;
            bounds.Inflate(3, 3);

            var pixel = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
            pixel.SetData(new[] { Color.White });

            var highlightColor = new Color(200, 180, 140);
            // Draw border
            spriteBatch.Draw(pixel, new Rectangle(bounds.X, bounds.Y, bounds.Width, 2), highlightColor);
            spriteBatch.Draw(pixel, new Rectangle(bounds.X, bounds.Bottom - 2, bounds.Width, 2), highlightColor);
            spriteBatch.Draw(pixel, new Rectangle(bounds.X, bounds.Y, 2, bounds.Height), highlightColor);
            spriteBatch.Draw(pixel, new Rectangle(bounds.Right - 2, bounds.Y, 2, bounds.Height), highlightColor);
        }
    }
}
```

---

### Task 16: Create NameStepPanel

**Files:**
- Create: `games/Solocaster/UI/CharacterBuilder/NameStepPanel.cs`

**Step 1: Create the name input panel**

```csharp
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Solocaster.State;
using Solocaster.UI.Widgets;
using Solocaster.Utilities;

namespace Solocaster.UI.CharacterBuilder;

public class NameStepPanel : Widget
{
    private readonly TextInputWidget _nameInput;
    private readonly ButtonWidget _randomButton;
    private readonly Action _onSelectionChanged;

    public NameStepPanel(SpriteFont font, Vector2 size, Action onSelectionChanged)
    {
        _onSelectionChanged = onSelectionChanged;
        Size = size;

        float inputWidth = 300;
        float inputHeight = 40;
        float buttonWidth = 100;
        float spacing = 20;
        float totalWidth = inputWidth + spacing + buttonWidth;
        float startX = (size.X - totalWidth) / 2;
        float y = (size.Y - inputHeight) / 2;

        _nameInput = new TextInputWidget
        {
            Font = font,
            Text = GameState.CurrentCharacter!.Name,
            MaxLength = 20,
            PlaceholderText = "Enter your name...",
            Position = new Vector2(startX, y),
            Size = new Vector2(inputWidth, inputHeight)
        };
        _nameInput.OnTextChanged += OnNameChanged;
        AddChild(_nameInput);

        _randomButton = new ButtonWidget
        {
            Text = "Random",
            Font = font,
            Position = new Vector2(startX + inputWidth + spacing, y),
            Size = new Vector2(buttonWidth, inputHeight)
        };
        _randomButton.OnClick += OnRandomClicked;
        AddChild(_randomButton);

        var labelY = y - 40;
        var label = new LabelWidget
        {
            Text = "Choose your name:",
            Font = font,
            TextColor = new Color(200, 180, 140),
            Position = new Vector2(0, labelY),
            Size = new Vector2(size.X, 30),
            CenterHorizontally = true
        };
        AddChild(label);
    }

    private void OnNameChanged(string name)
    {
        GameState.CurrentCharacter!.Name = name;
        _onSelectionChanged();
    }

    private void OnRandomClicked()
    {
        var character = GameState.CurrentCharacter!;
        var name = NameGenerator.Generate(character.Sex);
        _nameInput.Text = name;
        character.Name = name;
        _onSelectionChanged();
    }
}
```

---

### Task 17: Create SummaryStepPanel

**Files:**
- Create: `games/Solocaster/UI/CharacterBuilder/SummaryStepPanel.cs`

**Step 1: Create the summary panel**

```csharp
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Solo.Assets.Loaders;
using Solocaster.Character;
using Solocaster.Inventory;
using Solocaster.State;
using Solocaster.UI.Widgets;

namespace Solocaster.UI.CharacterBuilder;

public class SummaryStepPanel : Widget
{
    public SummaryStepPanel(SpriteFont font, Game game, Vector2 size)
    {
        Size = size;

        var character = GameState.CurrentCharacter!;
        var race = CharacterTemplateLoader.GetRace(character.RaceId);
        var cls = CharacterTemplateLoader.GetClass(character.ClassId);

        int leftX = 20;
        int rightX = 200;
        int y = 20;
        int lineHeight = 28;

        // Avatar
        try
        {
            var spriteSheet = SpriteSheetLoader.Get("avatars", game);
            var sprite = spriteSheet.Get(character.AvatarSpriteName);

            var avatarWidget = new ImageWidget
            {
                Texture = sprite.Texture,
                SourceRectangle = sprite.Bounds,
                ScaleToFit = true,
                Position = new Vector2(leftX, y),
                Size = new Vector2(120, 120)
            };
            AddChild(avatarWidget);
        }
        catch { }

        // Character info on right
        int infoY = y;

        var nameLabel = new LabelWidget
        {
            Text = character.Name,
            Font = font,
            TextColor = new Color(220, 200, 160),
            Position = new Vector2(rightX, infoY),
            Size = new Vector2(size.X - rightX - 20, lineHeight)
        };
        AddChild(nameLabel);
        infoY += lineHeight;

        var raceClassLabel = new LabelWidget
        {
            Text = $"{race?.Name ?? character.RaceId} {cls?.Name ?? character.ClassId}",
            Font = font,
            TextColor = Color.LightGray,
            Position = new Vector2(rightX, infoY),
            Size = new Vector2(size.X - rightX - 20, lineHeight)
        };
        AddChild(raceClassLabel);
        infoY += lineHeight;

        var sexLabel = new LabelWidget
        {
            Text = character.Sex.ToString(),
            Font = font,
            TextColor = Color.LightGray,
            Position = new Vector2(rightX, infoY),
            Size = new Vector2(size.X - rightX - 20, lineHeight)
        };
        AddChild(sexLabel);

        // Stats section
        y = 160;

        var statsHeader = new LabelWidget
        {
            Text = "Starting Stats:",
            Font = font,
            TextColor = new Color(200, 180, 140),
            Position = new Vector2(leftX, y),
            Size = new Vector2(size.X - 40, lineHeight)
        };
        AddChild(statsHeader);
        y += lineHeight + 5;

        // Calculate combined stats
        var stats = new[] { StatType.Strength, StatType.Agility, StatType.Vitality, StatType.Intelligence, StatType.Wisdom };

        foreach (var stat in stats)
        {
            int baseValue = 10;
            int raceBonus = race?.StatBonuses.GetValueOrDefault(stat) ?? 0;
            int classBonus = cls?.StatBonuses.GetValueOrDefault(stat) ?? 0;
            int total = baseValue + raceBonus + classBonus;

            string bonusText = "";
            if (raceBonus != 0 || classBonus != 0)
            {
                var parts = new System.Collections.Generic.List<string>();
                if (raceBonus != 0) parts.Add($"{(raceBonus >= 0 ? "+" : "")}{raceBonus} race");
                if (classBonus != 0) parts.Add($"{(classBonus >= 0 ? "+" : "")}{classBonus} class");
                bonusText = $" ({string.Join(", ", parts)})";
            }

            var statLabel = new LabelWidget
            {
                Text = $"{stat}: {total}{bonusText}",
                Font = font,
                TextColor = Color.White,
                Position = new Vector2(leftX + 20, y),
                Size = new Vector2(size.X - 60, lineHeight)
            };
            AddChild(statLabel);
            y += lineHeight;
        }
    }
}
```

---

### Task 18: Build and test all step panels

**Step 1: Build everything**

Run: `dotnet build games/Solocaster/Solocaster.csproj`
Expected: Build succeeded

**Step 2: Commit all step panels**

```bash
git add games/Solocaster/UI/CharacterBuilder/
git commit -m "feat: add all CharacterBuilder step panels"
```

---

### Task 19: Wire up CharacterBuilderScene

**Files:**
- Modify: `games/Solocaster/Scenes/CharacterBuilderScene.cs`

**Step 1: Complete the scene by adding the panel and navigation to PlayScene**

```csharp
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Solo;
using Solo.Services;
using Solocaster.Character;
using Solocaster.State;
using Solocaster.UI;
using Solocaster.UI.CharacterBuilder;

namespace Solocaster.Scenes;

public class CharacterBuilderScene : Scene
{
    private UIService? _uiService;
    private SpriteFont? _font;
    private CharacterBuilderPanel? _builderPanel;

    public CharacterBuilderScene(Game game) : base(game)
    {
    }

    protected override void EnterCore()
    {
        _uiService = GameServicesManager.Instance.GetRequired<UIService>();
        _uiService.ClearWidgets();

        var renderService = GameServicesManager.Instance.GetRequired<RenderService>();

        _font = Game.Content.Load<SpriteFont>("Font");

        UITheme.Load("./data/ui/theme.json");
        CharacterTemplateLoader.LoadAll("./data/templates/character/");

        GameState.EnsureCharacter();

        _builderPanel = new CharacterBuilderPanel(_font, Game);
        _builderPanel.CenterOnScreen(
            renderService.Graphics.GraphicsDevice.Viewport.Width,
            renderService.Graphics.GraphicsDevice.Viewport.Height
        );
        _builderPanel.OnStartGame += OnStartGame;
        _uiService.AddWidget(_builderPanel);
    }

    private void OnStartGame()
    {
        var sceneManager = GameServicesManager.Instance.GetRequired<SceneManager>();
        sceneManager.SetScene(SceneNames.Play);
    }
}
```

**Step 2: Build and run**

Run: `dotnet build games/Solocaster/Solocaster.csproj`
Then run the game and test the full character creation flow.
Expected: Character builder wizard appears, all steps work, clicking "Start Game" transitions to PlayScene

**Step 3: Commit**

```bash
git add games/Solocaster/Scenes/CharacterBuilderScene.cs
git commit -m "feat: complete CharacterBuilderScene with full wizard flow"
```

---

### Task 20: Add helper method to CharacterTemplateLoader

**Files:**
- Modify: `games/Solocaster/Character/CharacterTemplateLoader.cs`

**Step 1: Check if GetAllRaces, GetAllClasses, GetRace, GetClass methods exist. If not, add them:**

```csharp
public static IEnumerable<RaceTemplate> GetAllRaces() => _races.Values;
public static IEnumerable<ClassTemplate> GetAllClasses() => _classes.Values;
public static RaceTemplate? GetRace(string id) => _races.GetValueOrDefault(id);
public static ClassTemplate? GetClass(string id) => _classes.GetValueOrDefault(id);
```

**Step 2: Build to verify**

Run: `dotnet build games/Solocaster/Solocaster.csproj`
Expected: Build succeeded

**Step 3: Commit if changes were needed**

```bash
git add games/Solocaster/Character/CharacterTemplateLoader.cs
git commit -m "feat: add helper methods to CharacterTemplateLoader"
```

---

### Task 21: Add GetAllSpriteNames to SpriteSheetLoader (if needed)

**Files:**
- Check/Modify: `Solo/Assets/Loaders/SpriteSheetLoader.cs` or the SpriteSheet class

**Step 1: Check if a method exists to get all sprite names from a spritesheet. If not, add it to the SpriteSheet class:**

```csharp
public IEnumerable<string> GetAllSpriteNames() => _sprites.Keys;
```

**Step 2: Build to verify**

Run: `dotnet build games/Solocaster/Solocaster.csproj`
Expected: Build succeeded

**Step 3: Commit if changes were needed**

```bash
git add Solo/Assets/Loaders/SpriteSheetLoader.cs
git commit -m "feat: add GetAllSpriteNames to SpriteSheet"
```

---

## Phase 4: Final Integration & Polish

### Task 22: Final build and test

**Step 1: Clean build**

Run: `dotnet clean games/Solocaster/Solocaster.csproj && dotnet build games/Solocaster/Solocaster.csproj`
Expected: Build succeeded

**Step 2: Full test of character creation flow**

1. Launch game - should see Character Builder
2. Select race - detail panel updates
3. Click Next or press Enter
4. Select class - detail panel updates
5. Click Next
6. Select sex with buttons or arrows
7. Click Next
8. Select avatar from grid
9. Click Next
10. Enter name or click Random
11. Click Next
12. Review summary
13. Click "Start Game"
14. Verify PlayScene loads with selected character

**Step 3: Final commit**

```bash
git add -A
git commit -m "feat: complete Character Builder Scene implementation"
```

---

## Summary

**Files created:**
- `Character/CharacterData.cs`
- `State/GameState.cs`
- `UI/Widgets/StepIndicatorWidget.cs`
- `UI/Widgets/SelectableListWidget.cs`
- `UI/Widgets/TextInputWidget.cs`
- `UI/CharacterBuilder/CharacterBuilderPanel.cs`
- `UI/CharacterBuilder/RaceStepPanel.cs`
- `UI/CharacterBuilder/ClassStepPanel.cs`
- `UI/CharacterBuilder/SexStepPanel.cs`
- `UI/CharacterBuilder/AvatarStepPanel.cs`
- `UI/CharacterBuilder/NameStepPanel.cs`
- `UI/CharacterBuilder/SummaryStepPanel.cs`
- `Utilities/NameGenerator.cs`
- `Scenes/CharacterBuilderScene.cs`

**Files modified:**
- `Scenes/SceneNames.cs`
- `Scenes/PlayScene.cs`
- `SolocasterGame.cs`
- `Character/CharacterTemplateLoader.cs` (if needed)
- `Solo/Assets/Loaders/SpriteSheetLoader.cs` (if needed)

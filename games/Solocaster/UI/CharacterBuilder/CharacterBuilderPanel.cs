using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
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

    private enum StepType { Race, Class, Skills, Sex, Avatar, Name, Summary }
    private readonly List<StepType> _activeSteps = new();
    private int _currentStepIndex;
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

        // Build initial step list (Skills step will be added/removed based on race)
        RebuildStepList();

        // Step indicator at top
        _stepIndicator = new StepIndicatorWidget
        {
            Steps = GetStepNames(),
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

    private void RebuildStepList()
    {
        _activeSteps.Clear();
        _activeSteps.Add(StepType.Race);
        _activeSteps.Add(StepType.Class);

        // Add Skills step if current race has skill points
        var character = GameState.CurrentCharacter;
        if (character != null && CharacterTemplateLoader.TryGetRace(character.RaceId, out var race) && race?.SkillPoints > 0)
        {
            _activeSteps.Add(StepType.Skills);
        }

        _activeSteps.Add(StepType.Sex);
        _activeSteps.Add(StepType.Avatar);
        _activeSteps.Add(StepType.Name);
        _activeSteps.Add(StepType.Summary);
    }

    private List<string> GetStepNames()
    {
        return _activeSteps.Select(s => s.ToString()).ToList();
    }

    private void UpdateStepIndicator()
    {
        _stepIndicator.Steps = GetStepNames();
        _stepIndicator.CurrentStep = _currentStepIndex;
        _stepIndicator.CompletedSteps = _completedSteps;
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
        if (_currentStepIndex > 0)
            LoadStep(_currentStepIndex - 1);
    }

    private void OnNextClicked()
    {
        var currentStepType = _activeSteps[_currentStepIndex];

        if (currentStepType == StepType.Summary)
        {
            OnStartGame?.Invoke();
            return;
        }

        _completedSteps.Add(_currentStepIndex);

        // After Race step, rebuild step list in case Skills step needs to be added/removed
        if (currentStepType == StepType.Race)
        {
            var oldHasSkills = _activeSteps.Contains(StepType.Skills);
            RebuildStepList();
            var newHasSkills = _activeSteps.Contains(StepType.Skills);

            // Clear completed steps after Race if Skills step changed
            if (oldHasSkills != newHasSkills)
            {
                _completedSteps.Clear();
                _completedSteps.Add(0); // Race is still completed
            }

            UpdateStepIndicator();
        }

        LoadStep(_currentStepIndex + 1);
    }

    private void LoadStep(int stepIndex)
    {
        _currentStepIndex = stepIndex;
        UpdateStepIndicator();

        var isLastStep = _activeSteps[stepIndex] == StepType.Summary;
        _backButton.Enabled = stepIndex > 0;
        _nextButton.Text = isLastStep ? "Start Game" : "Next";

        // Clear previous content
        if (_currentStepContent != null)
        {
            _contentPanel.RemoveChild(_currentStepContent);
            _currentStepContent = null;
        }

        // Load new step content
        _currentStepContent = CreateStepContent(_activeSteps[stepIndex]);
        if (_currentStepContent != null)
        {
            _contentPanel.AddChild(_currentStepContent);
        }

        UpdateNextButtonState();
    }

    private Widget? CreateStepContent(StepType stepType)
    {
        var contentSize = _contentPanel.Size;

        return stepType switch
        {
            StepType.Race => new RaceStepPanel(_font, contentSize, OnSelectionChanged),
            StepType.Class => new ClassStepPanel(_font, contentSize, OnSelectionChanged),
            StepType.Skills => CreateSkillsPanel(contentSize),
            StepType.Sex => new SexStepPanel(_font, _game, contentSize, OnSelectionChanged),
            StepType.Avatar => new AvatarStepPanel(_font, _game, contentSize, OnSelectionChanged),
            StepType.Name => new NameStepPanel(_font, contentSize, OnSelectionChanged),
            StepType.Summary => new SummaryStepPanel(_font, _game, contentSize),
            _ => null
        };
    }

    private Widget? CreateSkillsPanel(Vector2 contentSize)
    {
        var character = GameState.CurrentCharacter!;
        if (!CharacterTemplateLoader.TryGetRace(character.RaceId, out var race) || race == null)
            return null;

        return new SkillsStepPanel(_font, contentSize, OnSelectionChanged, race.SkillPoints, race.PointBonus);
    }

    private void OnSelectionChanged()
    {
        UpdateNextButtonState();
    }

    private bool AreAllSkillPointsSpent()
    {
        var character = GameState.CurrentCharacter!;
        if (!CharacterTemplateLoader.TryGetRace(character.RaceId, out var race) || race == null)
            return true;

        int totalPoints = race.SkillPoints;
        int spentPoints = 0;
        foreach (var kvp in character.SkillPointAllocations)
        {
            spentPoints += kvp.Value;
        }

        return spentPoints >= totalPoints;
    }

    private void UpdateNextButtonState()
    {
        var character = GameState.CurrentCharacter!;
        var currentStepType = _activeSteps[_currentStepIndex];

        bool canProceed = currentStepType switch
        {
            StepType.Race => !string.IsNullOrEmpty(character.RaceId),
            StepType.Class => !string.IsNullOrEmpty(character.ClassId),
            StepType.Skills => AreAllSkillPointsSpent(),
            StepType.Sex => true, // Sex always has a value
            StepType.Avatar => !string.IsNullOrEmpty(character.AvatarSpriteName),
            StepType.Name => !string.IsNullOrEmpty(character.Name) && character.Name.Length >= 2,
            StepType.Summary => true, // Summary always allows proceeding
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

        // Backspace to go back (not Escape as that exits the game)
        if (keyboardState.IsKeyDown(Keys.Back) && !_previousKeyboardState.IsKeyDown(Keys.Back))
        {
            // Only go back if we're not on the name step (Backspace is used for text input there)
            var currentStepType = _activeSteps[_currentStepIndex];
            if (currentStepType != StepType.Name && _currentStepIndex > 0)
                OnBackClicked();
        }

        _previousKeyboardState = keyboardState;
    }
}

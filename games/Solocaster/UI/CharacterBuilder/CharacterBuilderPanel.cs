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

        // Backspace to go back (not Escape as that exits the game)
        if (keyboardState.IsKeyDown(Keys.Back) && !_previousKeyboardState.IsKeyDown(Keys.Back))
        {
            // Only go back if we're not on the name step (Backspace is used for text input there)
            if (_currentStep != 4 && _currentStep > 0)
                OnBackClicked();
        }

        _previousKeyboardState = keyboardState;
    }
}

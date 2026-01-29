using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Solocaster.UI.Widgets;

public class PointDistributorWidget : Widget
{
    private readonly ButtonWidget _minusButton;
    private readonly ButtonWidget _plusButton;
    private readonly LabelWidget _nameLabel;
    private readonly LabelWidget _valueLabel;
    private readonly LabelWidget _bonusLabel;

    private int _value;
    private readonly float _bonusPerPoint;

    public string SkillName { get; }
    public int Value
    {
        get => _value;
        set
        {
            _value = value;
            UpdateDisplay();
        }
    }

    public bool CanDecrease
    {
        get => _minusButton.Enabled;
        set => _minusButton.Enabled = value;
    }

    public bool CanIncrease
    {
        get => _plusButton.Enabled;
        set => _plusButton.Enabled = value;
    }

    public event Action? OnValueChanged;

    public PointDistributorWidget(string skillName, SpriteFont font, float bonusPerPoint)
    {
        SkillName = skillName;
        _bonusPerPoint = bonusPerPoint;

        Size = new Vector2(400, 35);

        _nameLabel = new LabelWidget
        {
            Text = skillName,
            Font = font,
            TextColor = UITheme.Text.Primary,
            Position = new Vector2(0, 5),
            Size = new Vector2(120, 25)
        };
        AddChild(_nameLabel);

        _minusButton = new ButtonWidget
        {
            Text = "-",
            Font = font,
            Position = new Vector2(140, 0),
            Size = new Vector2(35, 30),
            Enabled = false
        };
        _minusButton.OnClick += OnMinusClicked;
        AddChild(_minusButton);

        _valueLabel = new LabelWidget
        {
            Text = "0",
            Font = font,
            TextColor = UITheme.Text.Title,
            Position = new Vector2(185, 5),
            Size = new Vector2(30, 25),
            CenterHorizontally = true
        };
        AddChild(_valueLabel);

        _plusButton = new ButtonWidget
        {
            Text = "+",
            Font = font,
            Position = new Vector2(225, 0),
            Size = new Vector2(35, 30)
        };
        _plusButton.OnClick += OnPlusClicked;
        AddChild(_plusButton);

        _bonusLabel = new LabelWidget
        {
            Text = "(+0%)",
            Font = font,
            TextColor = UITheme.Text.Secondary,
            Position = new Vector2(275, 5),
            Size = new Vector2(80, 25)
        };
        AddChild(_bonusLabel);

        UpdateDisplay();
    }

    private void OnMinusClicked()
    {
        if (_value > 0)
        {
            _value--;
            UpdateDisplay();
            OnValueChanged?.Invoke();
        }
    }

    private void OnPlusClicked()
    {
        _value++;
        UpdateDisplay();
        OnValueChanged?.Invoke();
    }

    private void UpdateDisplay()
    {
        _valueLabel.Text = _value.ToString();

        int bonusPercent = (int)(_value * _bonusPerPoint * 100);
        _bonusLabel.Text = $"(+{bonusPercent}%)";
        _bonusLabel.TextColor = bonusPercent > 0 ? UITheme.StatusBar.ProgressFill : UITheme.Text.Secondary;

        _minusButton.Enabled = _value > 0;
    }

    public void HandleKeyboardInput(bool leftPressed, bool rightPressed)
    {
        if (leftPressed && CanDecrease && _value > 0)
        {
            _value--;
            UpdateDisplay();
            OnValueChanged?.Invoke();
        }
        else if (rightPressed && CanIncrease)
        {
            _value++;
            UpdateDisplay();
            OnValueChanged?.Invoke();
        }
    }
}

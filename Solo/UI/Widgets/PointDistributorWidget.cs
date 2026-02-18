using Microsoft.Xna.Framework;
using System;

namespace Solo.UI.Widgets;

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

    public PointDistributorWidget(string skillName, float bonusPerPoint)
    {
        SkillName = skillName;
        _bonusPerPoint = bonusPerPoint;

        _nameLabel = new LabelWidget
        {
            Text = skillName,
            TextColor = UITheme.Text.Primary
        };
        AddChild(_nameLabel);

        _minusButton = new ButtonWidget
        {
            AutoSize = false,
            Text = "-",
            Enabled = false
        };
        _minusButton.OnClick += OnMinusClicked;
        AddChild(_minusButton);

        _valueLabel = new LabelWidget
        {
            Text = "0",
            TextColor = UITheme.Text.Title,
            CenterHorizontally = true
        };
        AddChild(_valueLabel);

        _plusButton = new ButtonWidget
        {
            AutoSize = false,
            Text = "+"
        };
        _plusButton.OnClick += OnPlusClicked;
        AddChild(_plusButton);

        _bonusLabel = new LabelWidget
        {
            Text = "(+0%)",
            TextColor = UITheme.Text.Secondary
        };
        AddChild(_bonusLabel);

        UpdateDisplay();
    }

    protected override Vector2 MeasureCore(float availableWidth, float availableHeight)
    {
        float maxRight = 0;
        float maxBottom = 0;

        foreach (var child in Children)
        {
            if (!child.Visible)
                continue;

            child.Measure(availableWidth, availableHeight);

            float childRight = child.Position.X + child.DesiredSize.X;
            float childBottom = child.Position.Y + child.DesiredSize.Y;

            if (childRight > maxRight)
                maxRight = childRight;
            if (childBottom > maxBottom)
                maxBottom = childBottom;
        }

        return new Vector2(maxRight, maxBottom);
    }

    protected override void ArrangeCore(Vector2 finalSize)
    {
        int lh = UITheme.LineHeight;
        int btnSize = lh + 8;
        int labelVPad = (int)(finalSize.Y - lh) / 2;
        int nameWidth = Math.Max(120, lh * 6);
        int gap = lh / 2;
        int valueWidth = Math.Max(30, lh * 2);
        int bonusWidth = Math.Max(80, lh * 4);

        int x = 0;
        _nameLabel.Position = new Vector2(x, labelVPad);
        _nameLabel.Arrange(new Vector2(nameWidth, lh));
        x += nameWidth + gap;

        _minusButton.Position = new Vector2(x, 0);
        _minusButton.Arrange(new Vector2(btnSize, finalSize.Y));
        x += btnSize + gap / 2;

        _valueLabel.Position = new Vector2(x, labelVPad);
        _valueLabel.Arrange(new Vector2(valueWidth, lh));
        x += valueWidth + gap / 2;

        _plusButton.Position = new Vector2(x, 0);
        _plusButton.Arrange(new Vector2(btnSize, finalSize.Y));
        x += btnSize + gap;

        _bonusLabel.Position = new Vector2(x, labelVPad);
        _bonusLabel.Arrange(new Vector2(bonusWidth, lh));
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

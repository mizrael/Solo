using SkiaSharp;
using SpriteSheetEditor.Filters;
using SpriteSheetEditor.Utils;

namespace SpriteSheetEditor.Controls;

public partial class FilterPanel : ContentView
{
    public static readonly BindableProperty TargetColorProperty =
        BindableProperty.Create(nameof(TargetColor), typeof(SKColor), typeof(FilterPanel),
            SKColors.Magenta, propertyChanged: OnTargetColorChanged);

    public static readonly BindableProperty ToleranceProperty =
        BindableProperty.Create(nameof(Tolerance), typeof(float), typeof(FilterPanel),
            0f, propertyChanged: OnToleranceChanged);

    public static readonly BindableProperty IsPickingColorProperty =
        BindableProperty.Create(nameof(IsPickingColor), typeof(bool), typeof(FilterPanel),
            false);

    public static readonly BindableProperty ModeProperty =
        BindableProperty.Create(nameof(Mode), typeof(BackgroundRemovalMode), typeof(FilterPanel),
            BackgroundRemovalMode.SoftAlpha, propertyChanged: OnModeChanged);

    public SKColor TargetColor
    {
        get => (SKColor)GetValue(TargetColorProperty);
        set => SetValue(TargetColorProperty, value);
    }

    public float Tolerance
    {
        get => (float)GetValue(ToleranceProperty);
        set => SetValue(ToleranceProperty, value);
    }

    public bool IsPickingColor
    {
        get => (bool)GetValue(IsPickingColorProperty);
        set => SetValue(IsPickingColorProperty, value);
    }

    public BackgroundRemovalMode Mode
    {
        get => (BackgroundRemovalMode)GetValue(ModeProperty);
        set => SetValue(ModeProperty, value);
    }

    public event EventHandler? ApplyClicked;
    public event EventHandler? CancelClicked;
    public event EventHandler? PickColorClicked;
    public event EventHandler? SettingsChanged;

    private bool _isUpdatingFromCode;

    public FilterPanel()
    {
        InitializeComponent();
        ModePicker.SelectedIndex = 1; // Default to Soft Alpha
        UpdateColorSwatch();
    }

    private static void OnModeChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is FilterPanel panel)
        {
            panel.SettingsChanged?.Invoke(panel, EventArgs.Empty);
        }
    }

    private void OnModeChanged(object? sender, EventArgs e)
    {
        if (_isUpdatingFromCode) return;

        Mode = ModePicker.SelectedIndex switch
        {
            0 => BackgroundRemovalMode.Hard,
            1 => BackgroundRemovalMode.SoftAlpha,
            2 => BackgroundRemovalMode.ChromaKey,
            _ => BackgroundRemovalMode.SoftAlpha
        };
    }

    private static void OnTargetColorChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is FilterPanel panel)
        {
            panel.UpdateColorSwatch();
            panel.UpdateHexEntry();
            panel.SettingsChanged?.Invoke(panel, EventArgs.Empty);
        }
    }

    private static void OnToleranceChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is FilterPanel panel)
        {
            panel.UpdateToleranceSlider();
            panel.SettingsChanged?.Invoke(panel, EventArgs.Empty);
        }
    }

    private void OnHexEntryTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (_isUpdatingFromCode) return;

        var hexText = e.NewTextValue;
        if (SKColorUtils.TryParseHex(hexText, out var color))
        {
            _isUpdatingFromCode = true;
            TargetColor = color;
            _isUpdatingFromCode = false;
        }
    }

    private void OnToleranceSliderValueChanged(object? sender, ValueChangedEventArgs e)
    {
        if (_isUpdatingFromCode) return;

        var percentage = (int)e.NewValue;
        ToleranceValueLabel.Text = $"{percentage}%";

        _isUpdatingFromCode = true;
        Tolerance = (float)(e.NewValue / 100.0);
        _isUpdatingFromCode = false;

        SettingsChanged?.Invoke(this, EventArgs.Empty);
    }

    private void OnPickButtonClicked(object? sender, EventArgs e)
    {
        IsPickingColor = !IsPickingColor;
        PickButton.BackgroundColor = IsPickingColor ? Color.FromArgb("#0078d4") : null;
        PickColorClicked?.Invoke(this, EventArgs.Empty);
    }

    private void OnApplyButtonClicked(object? sender, EventArgs e)
    {
        ApplyClicked?.Invoke(this, EventArgs.Empty);
    }

    private void OnCancelButtonClicked(object? sender, EventArgs e)
    {
        CancelClicked?.Invoke(this, EventArgs.Empty);
    }

    private void UpdateColorSwatch()
    {
        ColorSwatch.Color = Color.FromRgba(TargetColor.Red, TargetColor.Green, TargetColor.Blue, TargetColor.Alpha);
    }

    private void UpdateHexEntry()
    {
        if (_isUpdatingFromCode) return;

        _isUpdatingFromCode = true;
        HexEntry.Text = $"#{TargetColor.Red:X2}{TargetColor.Green:X2}{TargetColor.Blue:X2}";
        _isUpdatingFromCode = false;
    }

    private void UpdateToleranceSlider()
    {
        if (_isUpdatingFromCode) return;

        _isUpdatingFromCode = true;
        ToleranceSlider.Value = Tolerance * 100;
        ToleranceValueLabel.Text = $"{(int)(Tolerance * 100)}%";
        _isUpdatingFromCode = false;
    }

    public void SetPickedColor(SKColor color)
    {
        IsPickingColor = false;
        PickButton.BackgroundColor = null;
        TargetColor = color;
    }
}

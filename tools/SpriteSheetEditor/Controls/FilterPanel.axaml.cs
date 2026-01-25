using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Media;
using SkiaSharp;
using SpriteSheetEditor.Filters;
using SpriteSheetEditor.Utils;

namespace SpriteSheetEditor.Controls;

public partial class FilterPanel : UserControl
{
    public static readonly StyledProperty<SKColor> TargetColorProperty =
        AvaloniaProperty.Register<FilterPanel, SKColor>(nameof(TargetColor), SKColors.Magenta);

    public static readonly StyledProperty<float> ToleranceProperty =
        AvaloniaProperty.Register<FilterPanel, float>(nameof(Tolerance), 0f);

    public static readonly StyledProperty<bool> IsPickingColorProperty =
        AvaloniaProperty.Register<FilterPanel, bool>(nameof(IsPickingColor), false);

    public static readonly StyledProperty<BackgroundRemovalMode> ModeProperty =
        AvaloniaProperty.Register<FilterPanel, BackgroundRemovalMode>(nameof(Mode), BackgroundRemovalMode.SoftAlpha);

    public SKColor TargetColor
    {
        get => GetValue(TargetColorProperty);
        set => SetValue(TargetColorProperty, value);
    }

    public float Tolerance
    {
        get => GetValue(ToleranceProperty);
        set => SetValue(ToleranceProperty, value);
    }

    public bool IsPickingColor
    {
        get => GetValue(IsPickingColorProperty);
        set => SetValue(IsPickingColorProperty, value);
    }

    public BackgroundRemovalMode Mode
    {
        get => GetValue(ModeProperty);
        set => SetValue(ModeProperty, value);
    }

    public event EventHandler? ApplyClicked;
    public event EventHandler? CancelClicked;
    public event EventHandler? PickColorClicked;
    public event EventHandler? SettingsChanged;

    private bool _isUpdatingFromCode;

    public FilterPanel()
    {
        _isUpdatingFromCode = true;
        InitializeComponent();
        _isUpdatingFromCode = false;
        UpdateColorSwatch();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == TargetColorProperty)
        {
            UpdateColorSwatch();
            UpdateHexEntry();
            SettingsChanged?.Invoke(this, EventArgs.Empty);
        }
        else if (change.Property == ToleranceProperty)
        {
            UpdateToleranceSlider();
            SettingsChanged?.Invoke(this, EventArgs.Empty);
        }
        else if (change.Property == ModeProperty)
        {
            SettingsChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private void OnModeChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_isUpdatingFromCode || ModePicker is null) return;

        Mode = ModePicker.SelectedIndex switch
        {
            0 => BackgroundRemovalMode.Hard,
            1 => BackgroundRemovalMode.SoftAlpha,
            2 => BackgroundRemovalMode.ChromaKey,
            _ => BackgroundRemovalMode.SoftAlpha
        };
    }

    private void OnHexEntryTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (_isUpdatingFromCode) return;

        var hexText = HexEntry.Text;
        if (SKColorUtils.TryParseHex(hexText, out var color))
        {
            _isUpdatingFromCode = true;
            TargetColor = color;
            _isUpdatingFromCode = false;
        }
    }

    private void OnToleranceSliderValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
        if (_isUpdatingFromCode) return;

        var percentage = (int)e.NewValue;
        ToleranceValueLabel.Text = $"{percentage}%";

        _isUpdatingFromCode = true;
        Tolerance = (float)(e.NewValue / 100.0);
        _isUpdatingFromCode = false;

        SettingsChanged?.Invoke(this, EventArgs.Empty);
    }

    private void OnPickButtonClicked(object? sender, RoutedEventArgs e)
    {
        IsPickingColor = !IsPickingColor;
        PickButton.Background = IsPickingColor ? new SolidColorBrush(Color.Parse("#0078d4")) : null;
        PickColorClicked?.Invoke(this, EventArgs.Empty);
    }

    private void OnApplyButtonClicked(object? sender, RoutedEventArgs e)
    {
        ApplyClicked?.Invoke(this, EventArgs.Empty);
    }

    private void OnCancelButtonClicked(object? sender, RoutedEventArgs e)
    {
        CancelClicked?.Invoke(this, EventArgs.Empty);
    }

    private void UpdateColorSwatch()
    {
        ColorSwatch.Background = new SolidColorBrush(Color.FromRgb(TargetColor.Red, TargetColor.Green, TargetColor.Blue));
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
        PickButton.Background = null;
        TargetColor = color;
    }
}

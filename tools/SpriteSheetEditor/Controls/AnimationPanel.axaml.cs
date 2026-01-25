using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using SkiaSharp;
using SpriteSheetEditor.Models;
using SpriteSheetEditor.ViewModels;

namespace SpriteSheetEditor.Controls;

public partial class AnimationPanel : UserControl
{
    public static readonly StyledProperty<MainViewModel?> ViewModelProperty =
        AvaloniaProperty.Register<AnimationPanel, MainViewModel?>(nameof(ViewModel));

    public static readonly StyledProperty<SKBitmap?> SourceImageProperty =
        AvaloniaProperty.Register<AnimationPanel, SKBitmap?>(nameof(SourceImage));

    public MainViewModel? ViewModel
    {
        get => GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    public SKBitmap? SourceImage
    {
        get => GetValue(SourceImageProperty);
        set => SetValue(SourceImageProperty, value);
    }

    public event EventHandler? NewAnimationClicked;
    public event EventHandler? DeleteAnimationClicked;
    public event EventHandler? AddFramesClicked;
    public event EventHandler? RemoveFrameClicked;
    public event EventHandler? ExportAnimationClicked;
    public event EventHandler<AnimationDefinition>? AnimationSelectionChanged;
    public event EventHandler<AnimationFrame?>? FrameSelectionChanged;
    public event EventHandler<AnimationPropertyChangedEventArgs>? AnimationPropertyChanged;
    public event EventHandler<FrameReorderedEventArgs>? FrameReordered;

    private bool _isUpdatingFromCode;

    public AnimationPanel()
    {
        InitializeComponent();
        PreviewCanvas.FrameChanged += OnPreviewFrameChanged;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == ViewModelProperty)
        {
            if (change.OldValue is MainViewModel oldVm)
            {
                oldVm.PropertyChanged -= OnViewModelPropertyChanged;
            }

            if (change.NewValue is MainViewModel newVm)
            {
                newVm.PropertyChanged += OnViewModelPropertyChanged;
                UpdateAnimationList();
            }
        }
        else if (change.Property == SourceImageProperty)
        {
            PreviewCanvas.SourceImage = SourceImage;
        }
    }

    private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainViewModel.SelectedAnimation))
        {
            UpdateSelectedAnimation();
        }
        else if (e.PropertyName == nameof(MainViewModel.AnimationCount))
        {
            UpdateAnimationList();
        }
    }

    private void UpdateAnimationList()
    {
        if (ViewModel is null) return;

        _isUpdatingFromCode = true;
        AnimationList.ItemsSource = ViewModel.Document.Animations;
        AnimationList.SelectedItem = ViewModel.SelectedAnimation;
        _isUpdatingFromCode = false;
    }

    private void UpdateSelectedAnimation()
    {
        if (ViewModel is null) return;

        _isUpdatingFromCode = true;

        var animation = ViewModel.SelectedAnimation;
        AnimationList.SelectedItem = animation;

        var hasAnimation = animation is not null;
        AnimationPropertiesPanel.IsVisible = hasAnimation;
        FrameListHeader.IsVisible = hasAnimation;
        FrameList.IsVisible = hasAnimation;
        PreviewPanel.IsVisible = hasAnimation;
        ExportButton.IsVisible = hasAnimation;

        if (animation is not null)
        {
            AnimationNameEntry.Text = animation.Name;
            FpsSpinner.Value = animation.Fps;
            LoopCheckBox.IsChecked = animation.Loop;
            FrameList.ItemsSource = animation.Frames;
            PreviewCanvas.Animation = animation;
            UpdateFrameCounter();
        }
        else
        {
            AnimationNameEntry.Text = string.Empty;
            FpsSpinner.Value = 10;
            LoopCheckBox.IsChecked = true;
            FrameList.ItemsSource = null;
            PreviewCanvas.Animation = null;
        }

        _isUpdatingFromCode = false;
    }

    private void OnAnimationSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_isUpdatingFromCode || ViewModel is null) return;

        if (AnimationList.SelectedItem is AnimationDefinition animation)
        {
            ViewModel.SelectedAnimation = animation;
            AnimationSelectionChanged?.Invoke(this, animation);
        }
    }

    private void OnFrameSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_isUpdatingFromCode || ViewModel is null) return;

        var frame = FrameList.SelectedItem as AnimationFrame;
        ViewModel.SelectedFrame = frame;
        FrameSelectionChanged?.Invoke(this, frame);
    }

    private void OnNewAnimationClicked(object? sender, RoutedEventArgs e)
    {
        NewAnimationClicked?.Invoke(this, EventArgs.Empty);
    }

    private void OnDeleteAnimationClicked(object? sender, RoutedEventArgs e)
    {
        DeleteAnimationClicked?.Invoke(this, EventArgs.Empty);
    }

    private void OnAddFramesClicked(object? sender, RoutedEventArgs e)
    {
        AddFramesClicked?.Invoke(this, EventArgs.Empty);
        UpdateFrameCounter();
    }

    private void OnRemoveFrameClicked(object? sender, RoutedEventArgs e)
    {
        RemoveFrameClicked?.Invoke(this, EventArgs.Empty);
        UpdateFrameCounter();
    }

    private void OnExportClicked(object? sender, RoutedEventArgs e)
    {
        ExportAnimationClicked?.Invoke(this, EventArgs.Empty);
    }

    private void OnAnimationNameChanged(object? sender, TextChangedEventArgs e)
    {
        if (_isUpdatingFromCode || ViewModel?.SelectedAnimation is null) return;

        var oldName = ViewModel.SelectedAnimation.Name;
        var newName = AnimationNameEntry.Text ?? string.Empty;

        if (oldName != newName)
        {
            AnimationPropertyChanged?.Invoke(this, new AnimationPropertyChangedEventArgs(
                ViewModel.SelectedAnimation, nameof(AnimationDefinition.Name), oldName, newName));
        }
    }

    private void OnFpsChanged(object? sender, NumericUpDownValueChangedEventArgs e)
    {
        if (_isUpdatingFromCode || ViewModel?.SelectedAnimation is null) return;

        var oldFps = ViewModel.SelectedAnimation.Fps;
        var newFps = (int)(e.NewValue ?? 10);

        if (oldFps != newFps)
        {
            AnimationPropertyChanged?.Invoke(this, new AnimationPropertyChangedEventArgs(
                ViewModel.SelectedAnimation, nameof(AnimationDefinition.Fps), oldFps, newFps));
        }
    }

    private void OnLoopChanged(object? sender, RoutedEventArgs e)
    {
        if (_isUpdatingFromCode || ViewModel?.SelectedAnimation is null) return;

        var oldLoop = ViewModel.SelectedAnimation.Loop;
        var newLoop = LoopCheckBox.IsChecked ?? true;

        if (oldLoop != newLoop)
        {
            AnimationPropertyChanged?.Invoke(this, new AnimationPropertyChangedEventArgs(
                ViewModel.SelectedAnimation, nameof(AnimationDefinition.Loop), oldLoop, newLoop));
        }
    }

    private void OnPlayPauseClicked(object? sender, RoutedEventArgs e)
    {
        PreviewCanvas.TogglePlayPause();
        PlayPauseButton.Content = PreviewCanvas.IsPlaying ? "Pause" : "Play";
    }

    private void OnStopClicked(object? sender, RoutedEventArgs e)
    {
        PreviewCanvas.Stop();
        PlayPauseButton.Content = "Play";
        UpdateFrameCounter();
    }

    private void OnPreviewFrameChanged(object? sender, int frameIndex)
    {
        UpdateFrameCounter();
    }

    private void UpdateFrameCounter()
    {
        var animation = ViewModel?.SelectedAnimation;
        if (animation is null)
        {
            FrameCounterLabel.Text = "Frame 0/0";
            return;
        }

        var currentFrame = PreviewCanvas.CurrentFrameIndex + 1;
        var totalFrames = animation.Frames.Count;
        FrameCounterLabel.Text = $"Frame {currentFrame}/{totalFrames}";
    }

    public void RefreshFrameList()
    {
        if (ViewModel?.SelectedAnimation is null) return;

        _isUpdatingFromCode = true;
        FrameList.ItemsSource = null;
        FrameList.ItemsSource = ViewModel.SelectedAnimation.Frames;
        _isUpdatingFromCode = false;

        UpdateFrameCounter();
    }
}

public class AnimationPropertyChangedEventArgs : EventArgs
{
    public AnimationDefinition Animation { get; }
    public string PropertyName { get; }
    public object? OldValue { get; }
    public object? NewValue { get; }

    public AnimationPropertyChangedEventArgs(AnimationDefinition animation, string propertyName, object? oldValue, object? newValue)
    {
        Animation = animation;
        PropertyName = propertyName;
        OldValue = oldValue;
        NewValue = newValue;
    }
}

public class FrameReorderedEventArgs : EventArgs
{
    public AnimationDefinition Animation { get; }
    public int OldIndex { get; }
    public int NewIndex { get; }

    public FrameReorderedEventArgs(AnimationDefinition animation, int oldIndex, int newIndex)
    {
        Animation = animation;
        OldIndex = oldIndex;
        NewIndex = newIndex;
    }
}

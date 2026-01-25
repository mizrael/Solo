using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using SkiaSharp;
using SpriteSheetEditor.Controls;
using SpriteSheetEditor.Filters;
using SpriteSheetEditor.Models;
using SpriteSheetEditor.Services;
using SpriteSheetEditor.UndoRedo.Commands;
using SpriteSheetEditor.ViewModels;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace SpriteSheetEditor;

public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel = new();
    private string? _nameBeforeEdit;

    public MainWindow()
    {
        InitializeComponent();
        DataContext = _viewModel;
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;
        UpdateStatusBar();
        UpdateMenuState();
        UpdateDocumentLabel();
        Canvas.ColorPicked += OnCanvasColorPicked;
        Canvas.SpriteDrawn += OnSpriteDrawn;
        Canvas.SpriteModified += OnSpriteModified;
        Canvas.ParentScrollViewer = CanvasScrollViewer;
        Canvas.ViewModel = _viewModel;

        SpriteListView.ItemsSource = _viewModel.Document.Sprites;

        KeyDown += OnWindowKeyDown;
    }

    private void OnNameEntryGotFocus(object? sender, GotFocusEventArgs e)
    {
        _nameBeforeEdit = _viewModel.SelectedSprite?.Name;
        NameErrorLabel.IsVisible = false;
    }

    private void OnNameEntryLostFocus(object? sender, RoutedEventArgs e)
    {
        if (_viewModel.SelectedSprite is null || _nameBeforeEdit is null) return;

        var newName = NameEntry.Text;

        if (string.IsNullOrWhiteSpace(newName))
        {
            _viewModel.SelectedSprite.Name = _nameBeforeEdit;
            NameErrorLabel.Text = "Name cannot be empty";
            NameErrorLabel.IsVisible = true;
            return;
        }

        var isDuplicate = _viewModel.Document.Sprites
            .Any(s => s != _viewModel.SelectedSprite &&
                      s.Name.Equals(newName, StringComparison.OrdinalIgnoreCase));

        if (isDuplicate)
        {
            _viewModel.SelectedSprite.Name = _nameBeforeEdit;
            NameErrorLabel.Text = $"Name '{newName}' is already used";
            NameErrorLabel.IsVisible = true;
            return;
        }

        _viewModel.SelectedSprite.Name = newName;
        NameErrorLabel.IsVisible = false;
    }

    private void OnSpriteDrawn(SpriteDefinition sprite)
    {
        var command = new AddSpriteCommand(_viewModel.Document, sprite);
        _viewModel.UndoRedo.Execute(command);
        _viewModel.SelectedSprite = sprite;
        _viewModel.NotifySpriteCountChanged();
        UpdateSpriteSelection();
    }

    private void OnSpriteModified(SpriteDefinition sprite, SpriteState oldState, string description)
    {
        var command = ModifySpriteCommand.Create(sprite, oldState, description);
        _viewModel.UndoRedo.Execute(command);
    }

    private void OnCanvasColorPicked(SKColor color)
    {
        SetPickedColor(color);
    }

    private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            UpdateStatusBar();
            UpdateMenuState();
        });
    }

    private void OnSpriteSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (SpriteListView.SelectedItem is SpriteDefinition sprite)
        {
            _viewModel.SelectedSprite = sprite;
            UpdatePropertyPanel();
        }
    }

    private void UpdateSpriteSelection()
    {
        SpriteListView.SelectedItem = _viewModel.SelectedSprite;
        UpdatePropertyPanel();
    }

    private void UpdatePropertyPanel()
    {
        if (_viewModel.SelectedSprite is { } sprite)
        {
            NameEntry.Text = sprite.Name;
            XEntry.Text = sprite.X.ToString();
            YEntry.Text = sprite.Y.ToString();
            WidthEntry.Text = sprite.Width.ToString();
            HeightEntry.Text = sprite.Height.ToString();
        }
        else
        {
            NameEntry.Text = string.Empty;
            XEntry.Text = string.Empty;
            YEntry.Text = string.Empty;
            WidthEntry.Text = string.Empty;
            HeightEntry.Text = string.Empty;
        }
    }

    private void UpdateStatusBar()
    {
        ZoomLabel.Text = $"Zoom: {_viewModel.ZoomLevel * 100:F0}%";
        ImageSizeLabel.Text = _viewModel.ImageWidth > 0
            ? $"Image: {_viewModel.ImageWidth}x{_viewModel.ImageHeight}"
            : "Image: --";
        SpriteCountLabel.Text = $"Sprites: {_viewModel.SpriteCount}";
        UpdateCanvasSize();
    }

    private void UpdateMenuState()
    {
        var hasDocument = _viewModel.Document.LoadedImage is not null;
        var hasSprites = _viewModel.Document.Sprites.Count > 0;

        SaveImageMenuItem.IsEnabled = hasDocument;
        ImportImagesMenuItem.IsEnabled = hasDocument;
        SaveJsonMenuItem.IsEnabled = hasDocument;
        CloseProjectMenuItem.IsEnabled = hasDocument || hasSprites;
        FiltersMenu.IsEnabled = hasDocument;
        FitToWindowMenuItem.IsEnabled = hasDocument;
        RearrangeMenuItem.IsEnabled = hasDocument && _viewModel.Document.Sprites.Count > 1;

        UndoMenuItem.IsEnabled = _viewModel.UndoRedo.CanUndo;
        RedoMenuItem.IsEnabled = _viewModel.UndoRedo.CanRedo;

        UndoMenuItem.Header = _viewModel.UndoRedo.CanUndo
            ? $"Undo {_viewModel.UndoRedo.UndoDescription}"
            : "Undo";
        RedoMenuItem.Header = _viewModel.UndoRedo.CanRedo
            ? $"Redo {_viewModel.UndoRedo.RedoDescription}"
            : "Redo";

        SelectToolMenuItem.Header = _viewModel.CurrentTool == EditorTool.Select ? "✓ Select" : "Select";
        DrawToolMenuItem.Header = _viewModel.CurrentTool == EditorTool.Draw ? "✓ Draw" : "Draw";
    }

    private void UpdateCanvasSize()
    {
        if (_viewModel.ImageWidth <= 0) return;

        var scaledWidth = _viewModel.ImageWidth * _viewModel.ZoomLevel;
        var scaledHeight = _viewModel.ImageHeight * _viewModel.ZoomLevel;

        Canvas.Width = scaledWidth;
        Canvas.Height = scaledHeight;
    }

    private void OnCanvasPanChanged()
    {
        UpdateCanvasSize();
    }

    private void UpdateDocumentLabel()
    {
        var imageName = !string.IsNullOrEmpty(_viewModel.Document.ImageFilePath)
            ? Path.GetFileName(_viewModel.Document.ImageFilePath)
            : null;
        var sheetName = _viewModel.Document.SpriteSheetName;

        if (imageName is not null && sheetName != "untitled")
        {
            DocumentLabel.Text = $"{imageName} - {sheetName}";
        }
        else if (imageName is not null)
        {
            DocumentLabel.Text = imageName;
        }
        else if (sheetName != "untitled")
        {
            DocumentLabel.Text = sheetName;
        }
        else
        {
            DocumentLabel.Text = "No document loaded";
        }
    }

    private async void OnLoadImagesClicked(object? sender, RoutedEventArgs e)
    {
        var hasUnsavedChanges = _viewModel.UndoRedo.CanUndo ||
                                _viewModel.Document.LoadedImage is not null ||
                                _viewModel.Document.Sprites.Count > 0;

        if (hasUnsavedChanges)
        {
            var result = await MessageBoxManager.GetMessageBoxStandard(
                "Load Images",
                "Loading images will replace the current document. You may have unsaved changes. Continue?",
                ButtonEnum.YesNo).ShowWindowDialogAsync(this);

            if (result != ButtonResult.Yes) return;
        }

        var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Select Images",
            AllowMultiple = true,
            FileTypeFilter = new[]
            {
                new FilePickerFileType("Images") { Patterns = new[] { "*.png", "*.jpg", "*.jpeg", "*.bmp" } }
            }
        });

        if (files.Count == 0) return;

        var filePaths = files.Select(f => f.Path.LocalPath).ToList();
        await LoadImagesDirect(filePaths, PackingLayout.Grid);
    }

    private async Task LoadImagesDirect(IReadOnlyList<string> filePaths, PackingLayout layout)
    {
        try
        {
            var result = await ImageImporter.LoadImagesAsync(filePaths, layout);

            var command = new LoadImagesCommand(
                _viewModel.Document,
                result.CompositeImage,
                result.Document.Sprites.ToList(),
                result.Document.SpriteSheetName);

            _viewModel.UndoRedo.Execute(command);
            _viewModel.SelectedSprite = null;
            _viewModel.NotifyImageChanged();
            _viewModel.NotifySpriteCountChanged();
            SpriteListView.ItemsSource = _viewModel.Document.Sprites;
            UpdateDocumentLabel();
            UpdateStatusBar();
            Canvas.FitToWindow();
        }
        catch (Exception ex)
        {
            await MessageBoxManager.GetMessageBoxStandard(
                "Load Error",
                $"Failed to load images: {ex.Message}",
                ButtonEnum.Ok).ShowWindowDialogAsync(this);
        }
    }

    private async void OnSaveImageClicked(object? sender, RoutedEventArgs e)
    {
        if (_viewModel.Document.LoadedImage is null)
        {
            await MessageBoxManager.GetMessageBoxStandard(
                "No Image",
                "Please load an image first.",
                ButtonEnum.Ok).ShowWindowDialogAsync(this);
            return;
        }

        var fileName = !string.IsNullOrEmpty(_viewModel.Document.ImageFilePath)
            ? Path.GetFileNameWithoutExtension(_viewModel.Document.ImageFilePath) + "_edited.png"
            : "image.png";

        var file = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Save Image",
            SuggestedFileName = fileName,
            FileTypeChoices = new[]
            {
                new FilePickerFileType("PNG Image") { Patterns = new[] { "*.png" } }
            }
        });

        if (file is not null)
        {
            await using var stream = await file.OpenWriteAsync();
            _viewModel.Document.LoadedImage.Encode(stream, SKEncodedImageFormat.Png, 100);

            await MessageBoxManager.GetMessageBoxStandard(
                "Saved",
                $"Image saved to {file.Path.LocalPath}",
                ButtonEnum.Ok).ShowWindowDialogAsync(this);
        }
    }

    private async void OnOpenJsonClicked(object? sender, RoutedEventArgs e)
    {
        var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Select Spritesheet JSON",
            AllowMultiple = false,
            FileTypeFilter = new[]
            {
                new FilePickerFileType("JSON") { Patterns = new[] { "*.json" } }
            }
        });

        if (files.Count == 0) return;

        var doc = await JsonExporter.LoadAsync(files[0].Path.LocalPath);
        doc.LoadedImage = _viewModel.Document.LoadedImage;
        doc.ImageFilePath = _viewModel.Document.ImageFilePath;
        _viewModel.Document = doc;
        SpriteListView.ItemsSource = _viewModel.Document.Sprites;
        UpdateDocumentLabel();

        var result = await MessageBoxManager.GetMessageBoxStandard(
            "Load Image?",
            "Would you like to load an image file for this spritesheet?",
            ButtonEnum.YesNo).ShowWindowDialogAsync(this);

        if (result == ButtonResult.Yes)
        {
            OnLoadImagesClicked(sender, e);
        }
    }

    private async void OnSaveJsonClicked(object? sender, RoutedEventArgs e)
    {
        var fileName = $"{_viewModel.Document.SpriteSheetName}.json";

        var file = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Save JSON",
            SuggestedFileName = fileName,
            FileTypeChoices = new[]
            {
                new FilePickerFileType("JSON") { Patterns = new[] { "*.json" } }
            }
        });

        if (file is not null)
        {
            await JsonExporter.SaveAsync(_viewModel.Document, file.Path.LocalPath);

            await MessageBoxManager.GetMessageBoxStandard(
                "Saved",
                $"Spritesheet saved to {file.Path.LocalPath}",
                ButtonEnum.Ok).ShowWindowDialogAsync(this);
        }
    }

    private async void OnCloseProjectClicked(object? sender, RoutedEventArgs e)
    {
        if (_viewModel.Document.LoadedImage is null && _viewModel.Document.Sprites.Count == 0)
        {
            return;
        }

        var result = await MessageBoxManager.GetMessageBoxStandard(
            "Close Project",
            "Are you sure you want to close the current project? Any unsaved changes will be lost.",
            ButtonEnum.YesNo).ShowWindowDialogAsync(this);

        if (result != ButtonResult.Yes) return;

        _viewModel.Document.LoadedImage?.Dispose();
        _viewModel.Document.LoadedImage = null;
        _viewModel.Document.ImageFilePath = null;
        _viewModel.Document.Sprites.Clear();
        _viewModel.Document.SpriteSheetName = "untitled";
        _viewModel.SelectedSprite = null;
        _viewModel.UndoRedo.Clear();
        _viewModel.NotifyImageChanged();
        SpriteListView.ItemsSource = _viewModel.Document.Sprites;
        UpdateDocumentLabel();
        UpdateStatusBar();
        Canvas.InvalidateVisual();
    }

    private void OnUndoClicked(object? sender, RoutedEventArgs e)
    {
        if (_viewModel.UndoRedo.CanUndo)
        {
            _viewModel.UndoRedo.Undo();
            _viewModel.NotifySpriteCountChanged();
            _viewModel.NotifyImageChanged();
            Canvas.InvalidateVisual();
        }
    }

    private void OnRedoClicked(object? sender, RoutedEventArgs e)
    {
        if (_viewModel.UndoRedo.CanRedo)
        {
            _viewModel.UndoRedo.Redo();
            _viewModel.NotifySpriteCountChanged();
            _viewModel.NotifyImageChanged();
            Canvas.InvalidateVisual();
        }
    }

    private void OnSelectToolClicked(object? sender, RoutedEventArgs e)
    {
        _viewModel.CurrentTool = EditorTool.Select;
        UpdateMenuState();
    }

    private void OnDrawToolClicked(object? sender, RoutedEventArgs e)
    {
        _viewModel.CurrentTool = EditorTool.Draw;
        UpdateMenuState();
    }

    private void OnFitToWindowClicked(object? sender, RoutedEventArgs e)
    {
        Canvas.FitToWindow();
    }

    private async void OnGridClicked(object? sender, RoutedEventArgs e)
    {
        if (_viewModel.ImageWidth == 0)
        {
            await MessageBoxManager.GetMessageBoxStandard(
                "No Image",
                "Please load an image first.",
                ButtonEnum.Ok).ShowWindowDialogAsync(this);
            return;
        }

        var dialog = new GridSettingsDialog();
        dialog.Show(_viewModel.ImageWidth, _viewModel.ImageHeight, _viewModel.SpriteCount);
        var result = await dialog.ShowDialog<GridSettingsEventArgs?>(this);

        if (result is not null)
        {
            var newSprites = GridGenerator.GenerateSprites(
                _viewModel.Document.SpriteSheetName,
                _viewModel.ImageWidth,
                _viewModel.ImageHeight,
                result.Columns,
                result.Rows);

            var command = new GenerateGridCommand(_viewModel.Document, newSprites);
            _viewModel.UndoRedo.Execute(command);
            _viewModel.SelectedSprite = null;
            _viewModel.NotifySpriteCountChanged();
            SpriteListView.ItemsSource = _viewModel.Document.Sprites;
            UpdateDocumentLabel();
        }
    }

    private void OnDeleteSpriteClicked(object? sender, RoutedEventArgs e)
    {
        if (_viewModel.SelectedSprite is null) return;

        var command = new RemoveSpriteCommand(_viewModel.Document, _viewModel.SelectedSprite);
        _viewModel.UndoRedo.Execute(command);
        _viewModel.SelectedSprite = null;
        _viewModel.NotifySpriteCountChanged();
        UpdateSpriteSelection();
    }

    private async void OnColorToTransparentClicked(object? sender, RoutedEventArgs e)
    {
        if (_viewModel.ImageWidth == 0)
        {
            await MessageBoxManager.GetMessageBoxStandard(
                "No Image",
                "Please load an image first.",
                ButtonEnum.Ok).ShowWindowDialogAsync(this);
            return;
        }

        _viewModel.BeginFilterPreview();
        FilterPanel.IsVisible = true;
        ApplyFilterPreview();
    }

    private void OnFilterApplyClicked(object? sender, EventArgs e)
    {
        if (_viewModel.OriginalImage is not null && _viewModel.Document.LoadedImage is not null)
        {
            var filterName = FilterPanel.Mode switch
            {
                BackgroundRemovalMode.Hard => "Hard Color Removal",
                BackgroundRemovalMode.SoftAlpha => "Soft Alpha",
                BackgroundRemovalMode.ChromaKey => "Chroma Key",
                _ => "Color Filter"
            };
            var command = new ApplyFilterCommand(_viewModel.Document, _viewModel.OriginalImage, _viewModel.Document.LoadedImage, filterName);
            _viewModel.UndoRedo.Execute(command);
        }
        _viewModel.ApplyFilter();
        FilterPanel.IsVisible = false;
        FilterPanel.IsPickingColor = false;
        UpdateDocumentLabel();
    }

    private void OnFilterCancelClicked(object? sender, EventArgs e)
    {
        _viewModel.CancelFilter();
        FilterPanel.IsVisible = false;
        FilterPanel.IsPickingColor = false;
        Canvas.InvalidateVisual();
    }

    private void OnFilterPickColorClicked(object? sender, EventArgs e)
    {
        Canvas.IsEyedropperMode = FilterPanel.IsPickingColor;
    }

    private void OnFilterSettingsChanged(object? sender, EventArgs e)
    {
        ApplyFilterPreview();
    }

    private void ApplyFilterPreview()
    {
        if (_viewModel.OriginalImage is null) return;

        var filtered = FilterPanel.Mode switch
        {
            BackgroundRemovalMode.Hard => ColorFilter.ApplyColorToTransparent(
                _viewModel.OriginalImage,
                FilterPanel.TargetColor,
                FilterPanel.Tolerance),
            BackgroundRemovalMode.SoftAlpha => ColorFilter.ApplyColorToTransparentSoft(
                _viewModel.OriginalImage,
                FilterPanel.TargetColor,
                FilterPanel.Tolerance),
            BackgroundRemovalMode.ChromaKey => ColorFilter.ApplyChromaKey(
                _viewModel.OriginalImage,
                FilterPanel.TargetColor,
                FilterPanel.Tolerance),
            _ => ColorFilter.ApplyColorToTransparentSoft(
                _viewModel.OriginalImage,
                FilterPanel.TargetColor,
                FilterPanel.Tolerance)
        };

        _viewModel.Document.LoadedImage?.Dispose();
        _viewModel.Document.LoadedImage = filtered;
        _viewModel.NotifyImageChanged();
        Canvas.InvalidateVisual();
    }

    public void SetPickedColor(SKColor color)
    {
        if (FilterPanel.IsVisible && FilterPanel.IsPickingColor)
        {
            FilterPanel.SetPickedColor(color);
            FilterPanel.IsPickingColor = false;
        }
    }

    private async void OnImportImagesClicked(object? sender, RoutedEventArgs e)
    {
        if (_viewModel.Document.LoadedImage is null)
        {
            await MessageBoxManager.GetMessageBoxStandard(
                "No Image",
                "Please load images first before importing additional images.",
                ButtonEnum.Ok).ShowWindowDialogAsync(this);
            return;
        }

        var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Import Images",
            AllowMultiple = true,
            FileTypeFilter = new[]
            {
                new FilePickerFileType("Images") { Patterns = new[] { "*.png", "*.jpg", "*.jpeg", "*.bmp" } }
            }
        });

        if (files.Count == 0) return;

        var filePaths = files.Select(f => f.Path.LocalPath).ToList();

        try
        {
            var result = await ImageImporter.AppendImagesAsync(
                filePaths,
                _viewModel.Document.LoadedImage!,
                _viewModel.Document.Sprites);

            var command = new ImportImagesCommand(
                _viewModel.Document,
                result.Image,
                result.AllSprites);

            _viewModel.UndoRedo.Execute(command);
            _viewModel.SelectedSprite = null;
            _viewModel.NotifyImageChanged();
            _viewModel.NotifySpriteCountChanged();
            SpriteListView.ItemsSource = _viewModel.Document.Sprites;
            UpdateDocumentLabel();
            UpdateStatusBar();
            Canvas.FitToWindow();
        }
        catch (Exception ex)
        {
            await MessageBoxManager.GetMessageBoxStandard(
                "Import Error",
                $"Failed to import images: {ex.Message}",
                ButtonEnum.Ok).ShowWindowDialogAsync(this);
        }
    }

    private async void OnRearrangeLayoutClicked(object? sender, RoutedEventArgs e)
    {
        if (_viewModel.Document.LoadedImage is null || _viewModel.Document.Sprites.Count < 2)
            return;

        var dialog = new ImportImagesDialog();
        dialog.ShowForRearrange(_viewModel.Document.Sprites.Count);
        var result = await dialog.ShowDialog<ImportImagesEventArgs?>(this);

        if (result is not null)
        {
            try
            {
                var rearrangeResult = ImageImporter.RearrangeLayout(
                    _viewModel.Document.LoadedImage,
                    _viewModel.Document.Sprites,
                    result.Layout);

                var command = new RearrangeLayoutCommand(_viewModel.Document, rearrangeResult.Image, rearrangeResult.Sprites);
                _viewModel.UndoRedo.Execute(command);

                _viewModel.SelectedSprite = null;
                _viewModel.NotifyImageChanged();
                SpriteListView.ItemsSource = _viewModel.Document.Sprites;
                UpdateDocumentLabel();
                UpdateStatusBar();
                Canvas.InvalidateVisual();
            }
            catch (Exception ex)
            {
                await MessageBoxManager.GetMessageBoxStandard(
                    "Rearrange Error",
                    $"Failed to rearrange layout: {ex.Message}",
                    ButtonEnum.Ok).ShowWindowDialogAsync(this);
            }
        }
    }

    private void OnWindowKeyDown(object? sender, KeyEventArgs e)
    {
        var ctrl = e.KeyModifiers.HasFlag(KeyModifiers.Control);
        var shift = e.KeyModifiers.HasFlag(KeyModifiers.Shift);

        switch (e.Key)
        {
            case Key.Z when ctrl && !shift:
                OnUndoClicked(null, new RoutedEventArgs());
                e.Handled = true;
                break;
            case Key.Y when ctrl:
            case Key.Z when ctrl && shift:
                OnRedoClicked(null, new RoutedEventArgs());
                e.Handled = true;
                break;
            case Key.O when ctrl && shift:
                OnOpenJsonClicked(null, new RoutedEventArgs());
                e.Handled = true;
                break;
            case Key.O when ctrl:
                OnLoadImagesClicked(null, new RoutedEventArgs());
                e.Handled = true;
                break;
            case Key.S when ctrl:
                OnSaveJsonClicked(null, new RoutedEventArgs());
                e.Handled = true;
                break;
            case Key.G when ctrl:
                OnGridClicked(null, new RoutedEventArgs());
                e.Handled = true;
                break;
            case Key.D1:
                _viewModel.CurrentTool = EditorTool.Select;
                UpdateMenuState();
                e.Handled = true;
                break;
            case Key.D2:
                _viewModel.CurrentTool = EditorTool.Draw;
                UpdateMenuState();
                e.Handled = true;
                break;
            case Key.Delete:
                _viewModel.DeleteSelectedSprite();
                _viewModel.NotifySpriteCountChanged();
                UpdateSpriteSelection();
                e.Handled = true;
                break;
            case Key.Escape:
                _viewModel.SelectedSprite = null;
                UpdateSpriteSelection();
                e.Handled = true;
                break;
        }
    }
}

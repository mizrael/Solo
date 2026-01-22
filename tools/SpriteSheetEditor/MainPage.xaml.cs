using SkiaSharp;
using SpriteSheetEditor.Controls;
using SpriteSheetEditor.Filters;
using SpriteSheetEditor.Models;
using SpriteSheetEditor.Services;
using SpriteSheetEditor.UndoRedo.Commands;
using SpriteSheetEditor.ViewModels;

namespace SpriteSheetEditor;

public partial class MainPage : ContentPage
{
    private readonly MainViewModel _viewModel = new();
    private string? _nameBeforeEdit;

    public MainPage()
    {
        InitializeComponent();
        BindingContext = _viewModel;
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;
        UpdateStatusBar();
        UpdateToolButtons();
        UpdateDocumentLabel();
        this.Focused += (s, e) => Focus();
        Canvas.ColorPicked += OnCanvasColorPicked;
        Canvas.SpriteDrawn += OnSpriteDrawn;
        Canvas.SpriteModified += OnSpriteModified;
        NameEntry.Focused += OnNameEntryFocused;
    }

    private void OnNameEntryFocused(object? sender, FocusEventArgs e)
    {
        _nameBeforeEdit = _viewModel.SelectedSprite?.Name;
        NameErrorLabel.IsVisible = false;
    }

    private void OnNameEntryUnfocused(object? sender, FocusEventArgs e)
    {
        if (_viewModel.SelectedSprite is null || _nameBeforeEdit is null) return;

        var newName = _viewModel.SelectedSprite.Name;

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

        NameErrorLabel.IsVisible = false;
    }

    private void OnSpriteDrawn(SpriteDefinition sprite)
    {
        var command = new AddSpriteCommand(_viewModel.Document, sprite);
        _viewModel.UndoRedo.Execute(command);
        _viewModel.SelectedSprite = sprite;
        _viewModel.NotifySpriteCountChanged();
    }

    private void OnSpriteModified(SpriteDefinition sprite, SpriteState oldState, string description)
    {
        var command = ModifySpriteCommand.Create(sprite, oldState, description);
        _viewModel.UndoRedo.Execute(command);
    }

    private void OnCanvasColorPicked(SkiaSharp.SKColor color)
    {
        SetPickedColor(color);
    }

    private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        UpdateStatusBar();
    }

    private void UpdateStatusBar()
    {
        ZoomLabel.Text = $"Zoom: {_viewModel.ZoomLevel * 100:F0}%";
        ImageSizeLabel.Text = _viewModel.ImageWidth > 0
            ? $"Image: {_viewModel.ImageWidth}x{_viewModel.ImageHeight}"
            : "Image: --";
        SpriteCountLabel.Text = $"Sprites: {_viewModel.SpriteCount}";
        FiltersButton.IsEnabled = _viewModel.ImageWidth > 0;
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

    private void UpdateToolButtons()
    {
        // Tool selection moved to Sprites menu - no toolbar buttons to update
    }

    private async void OnLoadImagesClicked(object? sender, EventArgs e)
    {
        var hasUnsavedChanges = _viewModel.UndoRedo.CanUndo ||
                                _viewModel.Document.LoadedImage is not null ||
                                _viewModel.Document.Sprites.Count > 0;

        if (hasUnsavedChanges)
        {
            var confirm = await DisplayAlertAsync("Load Images",
                "Loading images will replace the current document. You may have unsaved changes. Continue?",
                "Continue", "Cancel");

            if (!confirm) return;
        }

#if WINDOWS
        var hwnd = ((MauiWinUIWindow)Application.Current!.Windows[0].Handler!.PlatformView!).WindowHandle;
        var picker = new Windows.Storage.Pickers.FileOpenPicker
        {
            SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary,
            ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail
        };
        picker.FileTypeFilter.Add(".png");
        picker.FileTypeFilter.Add(".jpg");
        picker.FileTypeFilter.Add(".jpeg");
        picker.FileTypeFilter.Add(".bmp");

        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
        var files = await picker.PickMultipleFilesAsync();

        if (files is null || files.Count == 0) return;

        var filePaths = files.Select(f => f.Path).ToList();

        if (filePaths.Count == 1)
        {
            await LoadImagesDirect(filePaths, PackingLayout.Grid);
        }
        else
        {
            LoadImagesDialog.Show(filePaths, "Load Images", "Load", isImportMode: false);
        }
#endif
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
            UpdateDocumentLabel();
            UpdateStatusBar();
            Canvas.FitToWindow();
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Load Error", $"Failed to load images: {ex.Message}", "OK");
        }
    }

    private async void OnLoadImagesDialogConfirm(object? sender, ImportImagesEventArgs e)
    {
        LoadImagesDialog.Hide();

        await LoadImagesDirect(e.FilePaths, e.Layout);
    }

    private void OnLoadImagesDialogCancel(object? sender, EventArgs e)
    {
        LoadImagesDialog.Hide();
    }

    private async void OnSaveImageClicked(object? sender, EventArgs e)
    {
        if (_viewModel.Document.LoadedImage is null)
        {
            await DisplayAlertAsync("No Image", "Please load an image first.", "OK");
            return;
        }

#if WINDOWS
        var fileName = !string.IsNullOrEmpty(_viewModel.Document.ImageFilePath)
            ? Path.GetFileNameWithoutExtension(_viewModel.Document.ImageFilePath) + "_edited.png"
            : "image.png";

        var hwnd = ((MauiWinUIWindow)Application.Current!.Windows[0].Handler!.PlatformView!).WindowHandle;
        var savePicker = new Windows.Storage.Pickers.FileSavePicker
        {
            SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary,
            SuggestedFileName = fileName
        };
        savePicker.FileTypeChoices.Add("PNG Image", [".png"]);

        WinRT.Interop.InitializeWithWindow.Initialize(savePicker, hwnd);
        var file = await savePicker.PickSaveFileAsync();

        if (file is not null)
        {
            using var stream = await file.OpenStreamForWriteAsync();
            _viewModel.Document.LoadedImage.Encode(stream, SKEncodedImageFormat.Png, 100);
            await DisplayAlertAsync("Saved", $"Image saved to {file.Path}", "OK");
        }
#endif
    }

    private async void OnOpenJsonClicked(object? sender, EventArgs e)
    {
        var result = await FilePicker.PickAsync(new PickOptions
        {
            PickerTitle = "Select Spritesheet JSON",
            FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.WinUI, [".json"] }
            })
        });

        if (result is null) return;

        var doc = await JsonExporter.LoadAsync(result.FullPath);
        doc.LoadedImage = _viewModel.Document.LoadedImage;
        doc.ImageFilePath = _viewModel.Document.ImageFilePath;
        _viewModel.Document = doc;
        UpdateDocumentLabel();

        var loadImage = await DisplayAlertAsync("Load Image?",
            "Would you like to load an image file for this spritesheet?", "Yes", "No");

        if (loadImage)
        {
            OnLoadImagesClicked(sender, e);
        }
    }

    private async void OnSaveJsonClicked(object? sender, EventArgs e)
    {
        var fileName = $"{_viewModel.Document.SpriteSheetName}.json";

#if WINDOWS
        var hwnd = ((MauiWinUIWindow)Application.Current!.Windows[0].Handler!.PlatformView!).WindowHandle;
        var savePicker = new Windows.Storage.Pickers.FileSavePicker
        {
            SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary,
            SuggestedFileName = fileName
        };
        savePicker.FileTypeChoices.Add("JSON", [".json"]);

        WinRT.Interop.InitializeWithWindow.Initialize(savePicker, hwnd);
        var file = await savePicker.PickSaveFileAsync();

        if (file is not null)
        {
            await JsonExporter.SaveAsync(_viewModel.Document, file.Path);
            await DisplayAlertAsync("Saved", $"Spritesheet saved to {file.Path}", "OK");
        }
#endif
    }

    private async void OnCloseProjectClicked(object? sender, EventArgs e)
    {
        if (_viewModel.Document.LoadedImage is null && _viewModel.Document.Sprites.Count == 0)
        {
            return; // Nothing to close
        }

        var confirm = await DisplayAlertAsync("Close Project",
            "Are you sure you want to close the current project? Any unsaved changes will be lost.",
            "Close", "Cancel");

        if (!confirm) return;

        _viewModel.Document.LoadedImage?.Dispose();
        _viewModel.Document.LoadedImage = null;
        _viewModel.Document.ImageFilePath = null;
        _viewModel.Document.Sprites.Clear();
        _viewModel.Document.SpriteSheetName = "untitled";
        _viewModel.SelectedSprite = null;
        _viewModel.UndoRedo.Clear();
        _viewModel.NotifyImageChanged();
        UpdateDocumentLabel();
        UpdateStatusBar();
        Canvas.InvalidateSurface();
    }

    private void OnUndoClicked(object? sender, EventArgs e)
    {
        if (_viewModel.UndoRedo.CanUndo)
        {
            _viewModel.UndoRedo.Undo();
            _viewModel.NotifySpriteCountChanged();
            _viewModel.NotifyImageChanged();
            Canvas.InvalidateSurface();
        }
    }

    private void OnRedoClicked(object? sender, EventArgs e)
    {
        if (_viewModel.UndoRedo.CanRedo)
        {
            _viewModel.UndoRedo.Redo();
            _viewModel.NotifySpriteCountChanged();
            _viewModel.NotifyImageChanged();
            Canvas.InvalidateSurface();
        }
    }

    private void OnSelectToolClicked(object? sender, EventArgs e)
    {
        _viewModel.CurrentTool = EditorTool.Select;
        UpdateToolButtons();
    }

    private void OnDrawToolClicked(object? sender, EventArgs e)
    {
        _viewModel.CurrentTool = EditorTool.Draw;
        UpdateToolButtons();
    }

    private async void OnGridClicked(object? sender, EventArgs e)
    {
        if (_viewModel.ImageWidth == 0)
        {
            await DisplayAlertAsync("No Image", "Please load an image first.", "OK");
            return;
        }

        GridDialog.Show(_viewModel.ImageWidth, _viewModel.ImageHeight, _viewModel.SpriteCount);
    }

    private void OnGridDialogGenerate(object? sender, Controls.GridSettingsEventArgs e)
    {
        GridDialog.Hide();

        var newSprites = GridGenerator.GenerateSprites(
            _viewModel.Document.SpriteSheetName,
            _viewModel.ImageWidth,
            _viewModel.ImageHeight,
            e.Columns,
            e.Rows);

        var command = new GenerateGridCommand(_viewModel.Document, newSprites);
        _viewModel.UndoRedo.Execute(command);
        _viewModel.SelectedSprite = null;
        _viewModel.NotifySpriteCountChanged();
        UpdateDocumentLabel();
    }

    private void OnGridDialogCancel(object? sender, EventArgs e)
    {
        GridDialog.Hide();
    }

    private void OnDeleteSpriteClicked(object? sender, EventArgs e)
    {
        if (_viewModel.SelectedSprite is null) return;

        var command = new RemoveSpriteCommand(_viewModel.Document, _viewModel.SelectedSprite);
        _viewModel.UndoRedo.Execute(command);
        _viewModel.SelectedSprite = null;
        _viewModel.NotifySpriteCountChanged();
    }

    private void OnFileClicked(object? sender, EventArgs e)
    {
#if WINDOWS
        if (FileButton.Handler?.PlatformView is Microsoft.UI.Xaml.Controls.Button nativeButton)
        {
            var flyout = new Microsoft.UI.Xaml.Controls.MenuFlyout();

            var loadImages = new Microsoft.UI.Xaml.Controls.MenuFlyoutItem { Text = "Load Images..." };
            loadImages.Click += (s, args) => OnLoadImagesClicked(s, EventArgs.Empty);
            flyout.Items.Add(loadImages);

            var hasDocument = _viewModel.Document.LoadedImage is not null;
            var saveImage = new Microsoft.UI.Xaml.Controls.MenuFlyoutItem
            {
                Text = "Save Image...",
                IsEnabled = hasDocument
            };
            saveImage.Click += (s, args) => OnSaveImageClicked(s, EventArgs.Empty);
            flyout.Items.Add(saveImage);

            flyout.Items.Add(new Microsoft.UI.Xaml.Controls.MenuFlyoutSeparator());

            var importImages = new Microsoft.UI.Xaml.Controls.MenuFlyoutItem
            {
                Text = "Import Images...",
                IsEnabled = hasDocument
            };
            importImages.Click += (s, args) => OnImportImagesClicked(s, EventArgs.Empty);
            flyout.Items.Add(importImages);

            flyout.Items.Add(new Microsoft.UI.Xaml.Controls.MenuFlyoutSeparator());

            var openJson = new Microsoft.UI.Xaml.Controls.MenuFlyoutItem { Text = "Open JSON..." };
            openJson.Click += (s, args) => OnOpenJsonClicked(s, EventArgs.Empty);
            flyout.Items.Add(openJson);

            var saveJson = new Microsoft.UI.Xaml.Controls.MenuFlyoutItem
            {
                Text = "Save JSON...",
                IsEnabled = hasDocument
            };
            saveJson.Click += (s, args) => OnSaveJsonClicked(s, EventArgs.Empty);
            flyout.Items.Add(saveJson);

            flyout.Items.Add(new Microsoft.UI.Xaml.Controls.MenuFlyoutSeparator());

            var hasProject = _viewModel.Document.LoadedImage is not null || _viewModel.Document.Sprites.Count > 0;
            var closeProject = new Microsoft.UI.Xaml.Controls.MenuFlyoutItem
            {
                Text = "Close Project",
                IsEnabled = hasProject
            };
            closeProject.Click += (s, args) => OnCloseProjectClicked(s, EventArgs.Empty);
            flyout.Items.Add(closeProject);

            flyout.ShowAt(nativeButton);
        }
#endif
    }

    private void OnEditClicked(object? sender, EventArgs e)
    {
#if WINDOWS
        if (EditButton.Handler?.PlatformView is Microsoft.UI.Xaml.Controls.Button nativeButton)
        {
            var flyout = new Microsoft.UI.Xaml.Controls.MenuFlyout();

            var undoItem = new Microsoft.UI.Xaml.Controls.MenuFlyoutItem
            {
                Text = _viewModel.UndoRedo.CanUndo
                    ? $"Undo {_viewModel.UndoRedo.UndoDescription}"
                    : "Undo",
                IsEnabled = _viewModel.UndoRedo.CanUndo,
                KeyboardAcceleratorTextOverride = "Ctrl+Z"
            };
            undoItem.Click += (s, args) => OnUndoClicked(s, EventArgs.Empty);
            flyout.Items.Add(undoItem);

            var redoItem = new Microsoft.UI.Xaml.Controls.MenuFlyoutItem
            {
                Text = _viewModel.UndoRedo.CanRedo
                    ? $"Redo {_viewModel.UndoRedo.RedoDescription}"
                    : "Redo",
                IsEnabled = _viewModel.UndoRedo.CanRedo,
                KeyboardAcceleratorTextOverride = "Ctrl+Y"
            };
            redoItem.Click += (s, args) => OnRedoClicked(s, EventArgs.Empty);
            flyout.Items.Add(redoItem);

            flyout.Items.Add(new Microsoft.UI.Xaml.Controls.MenuFlyoutSeparator());

            var fitToWindow = new Microsoft.UI.Xaml.Controls.MenuFlyoutItem
            {
                Text = "Fit to window",
                IsEnabled = _viewModel.Document.LoadedImage is not null
            };
            fitToWindow.Click += (s, args) => Canvas.FitToWindow();
            flyout.Items.Add(fitToWindow);

            flyout.Items.Add(new Microsoft.UI.Xaml.Controls.MenuFlyoutSeparator());

            var rearrangeLayout = new Microsoft.UI.Xaml.Controls.MenuFlyoutItem
            {
                Text = "Rearrange layout...",
                IsEnabled = _viewModel.Document.LoadedImage is not null && _viewModel.Document.Sprites.Count > 1
            };
            rearrangeLayout.Click += (s, args) => OnRearrangeLayoutClicked(s, EventArgs.Empty);
            flyout.Items.Add(rearrangeLayout);

            flyout.ShowAt(nativeButton);
        }
#endif
    }

    private void OnSpritesClicked(object? sender, EventArgs e)
    {
#if WINDOWS
        if (SpritesButton.Handler?.PlatformView is Microsoft.UI.Xaml.Controls.Button nativeButton)
        {
            var flyout = new Microsoft.UI.Xaml.Controls.MenuFlyout();
            var hasDocument = _viewModel.Document.LoadedImage is not null;

            var selectItem = new Microsoft.UI.Xaml.Controls.MenuFlyoutItem
            {
                Text = _viewModel.CurrentTool == EditorTool.Select ? "✓ Select" : "   Select",
                IsEnabled = hasDocument,
                KeyboardAcceleratorTextOverride = "1"
            };
            selectItem.Click += (s, args) => OnSelectToolClicked(s, EventArgs.Empty);
            flyout.Items.Add(selectItem);

            var drawItem = new Microsoft.UI.Xaml.Controls.MenuFlyoutItem
            {
                Text = _viewModel.CurrentTool == EditorTool.Draw ? "✓ Draw" : "   Draw",
                IsEnabled = hasDocument,
                KeyboardAcceleratorTextOverride = "2"
            };
            drawItem.Click += (s, args) => OnDrawToolClicked(s, EventArgs.Empty);
            flyout.Items.Add(drawItem);

            flyout.ShowAt(nativeButton);
        }
#endif
    }

    private void OnFiltersClicked(object? sender, EventArgs e)
    {
#if WINDOWS
        if (FiltersButton.Handler?.PlatformView is Microsoft.UI.Xaml.Controls.Button nativeButton)
        {
            var flyout = new Microsoft.UI.Xaml.Controls.MenuFlyout();
            var item = new Microsoft.UI.Xaml.Controls.MenuFlyoutItem { Text = "Color to Transparent" };
            item.Click += (s, args) => OnColorToTransparentClicked(s, EventArgs.Empty);
            flyout.Items.Add(item);
            flyout.ShowAt(nativeButton);
        }
#endif
    }

    private async void OnColorToTransparentClicked(object? sender, EventArgs e)
    {
        if (_viewModel.ImageWidth == 0)
        {
            await DisplayAlertAsync("No Image", "Please load an image first.", "OK");
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
        Canvas.InvalidateSurface();
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
        Canvas.InvalidateSurface();
    }

    public void SetPickedColor(SKColor color)
    {
        if (FilterPanel.IsVisible && FilterPanel.IsPickingColor)
        {
            FilterPanel.SetPickedColor(color);
            FilterPanel.IsPickingColor = false;
        }
    }

    private async void OnImportImagesClicked(object? sender, EventArgs e)
    {
        if (_viewModel.Document.LoadedImage is null)
        {
            await DisplayAlertAsync("No Image", "Please load images first before importing additional images.", "OK");
            return;
        }

#if WINDOWS
        var hwnd = ((MauiWinUIWindow)Application.Current!.Windows[0].Handler!.PlatformView!).WindowHandle;
        var picker = new Windows.Storage.Pickers.FileOpenPicker
        {
            SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary,
            ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail
        };
        picker.FileTypeFilter.Add(".png");
        picker.FileTypeFilter.Add(".jpg");
        picker.FileTypeFilter.Add(".jpeg");
        picker.FileTypeFilter.Add(".bmp");

        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
        var files = await picker.PickMultipleFilesAsync();

        if (files is null || files.Count == 0) return;

        var filePaths = files.Select(f => f.Path).ToList();

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
            UpdateDocumentLabel();
            UpdateStatusBar();
            Canvas.FitToWindow();
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Import Error", $"Failed to import images: {ex.Message}", "OK");
        }
#endif
    }

    private void OnRearrangeLayoutClicked(object? sender, EventArgs e)
    {
        RearrangeDialog.ShowForRearrange(_viewModel.Document.Sprites.Count);
    }

    private async void OnRearrangeDialogConfirm(object? sender, ImportImagesEventArgs e)
    {
        RearrangeDialog.Hide();

        if (_viewModel.Document.LoadedImage is null || _viewModel.Document.Sprites.Count < 2)
            return;

        try
        {
            var result = ImageImporter.RearrangeLayout(
                _viewModel.Document.LoadedImage,
                _viewModel.Document.Sprites,
                e.Layout);

            var command = new RearrangeLayoutCommand(_viewModel.Document, result.Image, result.Sprites);
            _viewModel.UndoRedo.Execute(command);

            _viewModel.SelectedSprite = null;
            _viewModel.NotifyImageChanged();
            UpdateDocumentLabel();
            UpdateStatusBar();
            Canvas.InvalidateSurface();
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Rearrange Error", $"Failed to rearrange layout: {ex.Message}", "OK");
        }
    }

    private void OnRearrangeDialogCancel(object? sender, EventArgs e)
    {
        RearrangeDialog.Hide();
    }

    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();

#if WINDOWS
        var nativeWindow = (Handler?.PlatformView as Microsoft.UI.Xaml.FrameworkElement)?.XamlRoot?.Content;
        if (nativeWindow is Microsoft.UI.Xaml.UIElement element)
        {
            element.KeyDown += OnKeyDown;
        }
#endif
    }

#if WINDOWS
    private void OnKeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
    {
        var ctrl = Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread(Windows.System.VirtualKey.Control)
            .HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down);
        var shift = Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread(Windows.System.VirtualKey.Shift)
            .HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down);

        switch (e.Key)
        {
            case Windows.System.VirtualKey.Z when ctrl && !shift:
                OnUndoClicked(null, EventArgs.Empty);
                e.Handled = true;
                break;
            case Windows.System.VirtualKey.Y when ctrl:
            case Windows.System.VirtualKey.Z when ctrl && shift:
                OnRedoClicked(null, EventArgs.Empty);
                e.Handled = true;
                break;
            case Windows.System.VirtualKey.O when ctrl && shift:
                OnOpenJsonClicked(null, EventArgs.Empty);
                e.Handled = true;
                break;
            case Windows.System.VirtualKey.O when ctrl:
                OnLoadImagesClicked(null, EventArgs.Empty);
                e.Handled = true;
                break;
            case Windows.System.VirtualKey.S when ctrl:
                OnSaveJsonClicked(null, EventArgs.Empty);
                e.Handled = true;
                break;
            case Windows.System.VirtualKey.G when ctrl:
                OnGridClicked(null, EventArgs.Empty);
                e.Handled = true;
                break;
            case Windows.System.VirtualKey.Number1:
                _viewModel.CurrentTool = EditorTool.Select;
                UpdateToolButtons();
                e.Handled = true;
                break;
            case Windows.System.VirtualKey.Number2:
                _viewModel.CurrentTool = EditorTool.Draw;
                UpdateToolButtons();
                e.Handled = true;
                break;
            case Windows.System.VirtualKey.Delete:
                _viewModel.DeleteSelectedSprite();
                e.Handled = true;
                break;
            case Windows.System.VirtualKey.Escape:
                _viewModel.SelectedSprite = null;
                e.Handled = true;
                break;
        }
    }
#endif
}

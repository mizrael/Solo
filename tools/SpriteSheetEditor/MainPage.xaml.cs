using SkiaSharp;
using SpriteSheetEditor.Filters;
using SpriteSheetEditor.Services;
using SpriteSheetEditor.ViewModels;

namespace SpriteSheetEditor;

public partial class MainPage : ContentPage
{
    private readonly MainViewModel _viewModel = new();

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
        SelectToolButton.BackgroundColor = _viewModel.CurrentTool == EditorTool.Select
            ? Color.FromArgb("#0078d4") : Color.FromArgb("#3e3e42");
        DrawToolButton.BackgroundColor = _viewModel.CurrentTool == EditorTool.Draw
            ? Color.FromArgb("#0078d4") : Color.FromArgb("#3e3e42");
    }

    private async void OnOpenImageClicked(object? sender, EventArgs e)
    {
        var result = await FilePicker.PickAsync(new PickOptions
        {
            PickerTitle = "Select Image",
            FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.WinUI, [".png", ".jpg", ".jpeg", ".bmp"] }
            })
        });

        if (result is null) return;

        using var stream = await result.OpenReadAsync();
        _viewModel.Document.LoadedImage = SKBitmap.Decode(stream);
        _viewModel.Document.ImageFilePath = result.FullPath;
        _viewModel.Document.SpriteSheetName = Path.GetFileNameWithoutExtension(result.FileName);
        _viewModel.NotifyImageChanged();
        UpdateDocumentLabel();
        Canvas.FitToWindow();
    }

    private async void OnSaveImageClicked(object? sender, EventArgs e)
    {
        if (_viewModel.Document.LoadedImage is null)
        {
            await DisplayAlert("No Image", "Please load an image first.", "OK");
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
            await DisplayAlert("Saved", $"Image saved to {file.Path}", "OK");
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

        var loadImage = await DisplayAlert("Load Image?",
            "Would you like to load an image file for this spritesheet?", "Yes", "No");

        if (loadImage)
        {
            OnOpenImageClicked(sender, e);
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
            await DisplayAlert("Saved", $"Spritesheet saved to {file.Path}", "OK");
        }
#endif
    }

    private async void OnCloseProjectClicked(object? sender, EventArgs e)
    {
        if (_viewModel.Document.LoadedImage is null && _viewModel.Document.Sprites.Count == 0)
        {
            return; // Nothing to close
        }

        var confirm = await DisplayAlert("Close Project",
            "Are you sure you want to close the current project? Any unsaved changes will be lost.",
            "Close", "Cancel");

        if (!confirm) return;

        _viewModel.Document.LoadedImage?.Dispose();
        _viewModel.Document.LoadedImage = null;
        _viewModel.Document.ImageFilePath = null;
        _viewModel.Document.Sprites.Clear();
        _viewModel.Document.SpriteSheetName = "untitled";
        _viewModel.SelectedSprite = null;
        _viewModel.NotifyImageChanged();
        UpdateDocumentLabel();
        UpdateStatusBar();
        Canvas.InvalidateSurface();
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
            await DisplayAlert("No Image", "Please load an image first.", "OK");
            return;
        }

        var columnsStr = await DisplayPromptAsync("Grid Generation", "Number of columns:", initialValue: "4", keyboard: Keyboard.Numeric);
        if (string.IsNullOrEmpty(columnsStr) || !int.TryParse(columnsStr, out var columns) || columns < 1) return;

        var rowsStr = await DisplayPromptAsync("Grid Generation", "Number of rows:", initialValue: "4", keyboard: Keyboard.Numeric);
        if (string.IsNullOrEmpty(rowsStr) || !int.TryParse(rowsStr, out var rows) || rows < 1) return;

        var (tileWidth, tileHeight) = GridGenerator.CalculateTileSize(_viewModel.ImageWidth, _viewModel.ImageHeight, columns, rows);
        var totalSprites = columns * rows;

        var message = $"Tile size: {tileWidth}x{tileHeight}\nWill create {totalSprites} sprites.";

        if (GridGenerator.HasUncoveredPixels(_viewModel.ImageWidth, _viewModel.ImageHeight, columns, rows))
        {
            var (uncoveredX, uncoveredY) = GridGenerator.GetUncoveredPixels(_viewModel.ImageWidth, _viewModel.ImageHeight, columns, rows);
            message += $"\n\nWarning: {uncoveredX} pixel(s) horizontally and {uncoveredY} pixel(s) vertically will be uncovered.";
        }

        if (_viewModel.SpriteCount > 0)
        {
            message += $"\n\nThis will replace all {_viewModel.SpriteCount} existing sprites.";
        }

        var proceed = await DisplayAlert("Generate Grid", message, "Generate", "Cancel");
        if (!proceed) return;

        var newDoc = GridGenerator.Generate(_viewModel.Document.SpriteSheetName, _viewModel.ImageWidth, _viewModel.ImageHeight, columns, rows);
        newDoc.LoadedImage = _viewModel.Document.LoadedImage;
        newDoc.ImageFilePath = _viewModel.Document.ImageFilePath;
        _viewModel.Document = newDoc;
        _viewModel.SelectedSprite = null;
        UpdateDocumentLabel();
    }

    private void OnFitClicked(object? sender, EventArgs e)
    {
        Canvas.FitToWindow();
    }

    private void OnDeleteSpriteClicked(object? sender, EventArgs e)
    {
        _viewModel.DeleteSelectedSprite();
    }

    private void OnFileClicked(object? sender, EventArgs e)
    {
#if WINDOWS
        if (FileButton.Handler?.PlatformView is Microsoft.UI.Xaml.Controls.Button nativeButton)
        {
            var flyout = new Microsoft.UI.Xaml.Controls.MenuFlyout();

            var openImage = new Microsoft.UI.Xaml.Controls.MenuFlyoutItem { Text = "Open Image..." };
            openImage.Click += (s, args) => OnOpenImageClicked(s, EventArgs.Empty);
            flyout.Items.Add(openImage);

            var saveImage = new Microsoft.UI.Xaml.Controls.MenuFlyoutItem { Text = "Save Image..." };
            saveImage.Click += (s, args) => OnSaveImageClicked(s, EventArgs.Empty);
            flyout.Items.Add(saveImage);

            flyout.Items.Add(new Microsoft.UI.Xaml.Controls.MenuFlyoutSeparator());

            var openJson = new Microsoft.UI.Xaml.Controls.MenuFlyoutItem { Text = "Open JSON..." };
            openJson.Click += (s, args) => OnOpenJsonClicked(s, EventArgs.Empty);
            flyout.Items.Add(openJson);

            var saveJson = new Microsoft.UI.Xaml.Controls.MenuFlyoutItem { Text = "Save JSON..." };
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
            await DisplayAlert("No Image", "Please load an image first.", "OK");
            return;
        }

        _viewModel.BeginFilterPreview();
        FilterPanel.IsVisible = true;
        ApplyFilterPreview();
    }

    private void OnFilterApplyClicked(object? sender, EventArgs e)
    {
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
            case Windows.System.VirtualKey.O when ctrl && shift:
                OnOpenJsonClicked(null, EventArgs.Empty);
                e.Handled = true;
                break;
            case Windows.System.VirtualKey.O when ctrl:
                OnOpenImageClicked(null, EventArgs.Empty);
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
            case Windows.System.VirtualKey.Number0 when ctrl:
                Canvas.FitToWindow();
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
            case Windows.System.VirtualKey.Add:
            case (Windows.System.VirtualKey)187:
                Canvas.ZoomIn();
                e.Handled = true;
                break;
            case Windows.System.VirtualKey.Subtract:
            case (Windows.System.VirtualKey)189:
                Canvas.ZoomOut();
                e.Handled = true;
                break;
        }
    }
#endif
}

using SpriteSheetEditor.Services;

namespace SpriteSheetEditor.Controls;

public partial class ImportImagesDialog : ContentView
{
    private const string DefaultTitle = "Import Images";
    private const string DefaultButtonText = "Import";

    private IReadOnlyList<string> _filePaths = [];
    private bool _isImportMode;

    public event EventHandler<ImportImagesEventArgs>? ImportClicked;
    public event EventHandler? CancelClicked;

    public ImportImagesDialog()
    {
        InitializeComponent();
    }

    public void Show(IReadOnlyList<string> filePaths, string? title = null, string? buttonText = null, bool isImportMode = false)
    {
        _filePaths = filePaths;
        _isImportMode = isImportMode;

        TitleLabel.Text = title ?? DefaultTitle;
        ImportButton.Text = buttonText ?? DefaultButtonText;
        FileCountLabel.Text = $"{filePaths.Count} image{(filePaths.Count == 1 ? "" : "s")} selected";
        PaddingEntry.Text = "0";
        InfoLabel.Text = string.Empty;
        ImportButton.IsEnabled = filePaths.Count > 0;

        // Reset layout options
        GridRadio.IsChecked = true;
        ImportGridRadio.IsChecked = true;
        ChangeLayoutCheckBox.IsChecked = false;
        ImportLayoutOptions.IsVisible = false;

        // Show appropriate layout panel based on mode
        LayoutPanel.IsVisible = !isImportMode;
        ChangeLayoutPanel.IsVisible = isImportMode;

        IsVisible = true;
    }

    public void Hide()
    {
        IsVisible = false;
    }

    public void ShowForRearrange(int spriteCount)
    {
        _filePaths = [];
        _isImportMode = false;

        TitleLabel.Text = "Rearrange Layout";
        ImportButton.Text = "Apply";
        FileCountLabel.Text = $"{spriteCount} sprite{(spriteCount == 1 ? "" : "s")}";
        PaddingEntry.Text = "0";
        InfoLabel.Text = string.Empty;
        ImportButton.IsEnabled = spriteCount > 1;

        // Reset layout options
        GridRadio.IsChecked = true;

        // Show layout panel for rearranging
        LayoutPanel.IsVisible = true;
        ChangeLayoutPanel.IsVisible = false;

        IsVisible = true;
    }

    private void OnChangeLayoutChecked(object? sender, CheckedChangedEventArgs e)
    {
        ImportLayoutOptions.IsVisible = e.Value;
    }

    private PackingLayout GetSelectedLayout()
    {
        if (_isImportMode)
        {
            if (!ChangeLayoutCheckBox.IsChecked)
                return PackingLayout.Grid;

            if (ImportColumnRadio.IsChecked)
                return PackingLayout.SingleColumn;
            if (ImportRowRadio.IsChecked)
                return PackingLayout.SingleRow;
            return PackingLayout.Grid;
        }
        else
        {
            if (ColumnRadio.IsChecked)
                return PackingLayout.SingleColumn;
            if (RowRadio.IsChecked)
                return PackingLayout.SingleRow;
            return PackingLayout.Grid;
        }
    }

    private void OnImportClicked(object? sender, EventArgs e)
    {
        if (!int.TryParse(PaddingEntry.Text, out var padding) || padding < 0)
        {
            padding = 0;
        }

        var layout = GetSelectedLayout();
        ImportClicked?.Invoke(this, new ImportImagesEventArgs(_filePaths, padding, layout));
    }

    private void OnCancelClicked(object? sender, EventArgs e)
    {
        Hide();
        CancelClicked?.Invoke(this, EventArgs.Empty);
    }
}

public class ImportImagesEventArgs : EventArgs
{
    public IReadOnlyList<string> FilePaths { get; }
    public int Padding { get; }
    public PackingLayout Layout { get; }

    public ImportImagesEventArgs(IReadOnlyList<string> filePaths, int padding, PackingLayout layout)
    {
        FilePaths = filePaths;
        Padding = padding;
        Layout = layout;
    }
}

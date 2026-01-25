using Avalonia.Controls;
using Avalonia.Interactivity;
using SpriteSheetEditor.Services;

namespace SpriteSheetEditor.Controls;

public partial class ImportImagesDialog : Window
{
    private IReadOnlyList<string> _filePaths = [];

    public ImportImagesDialog()
    {
        InitializeComponent();
    }

    public void Show(IReadOnlyList<string> filePaths, string? title = null, string? buttonText = null)
    {
        _filePaths = filePaths;

        TitleLabel.Text = title ?? "Import Images";
        ImportButton.Content = buttonText ?? "Import";
        FileCountLabel.Text = $"{filePaths.Count} image{(filePaths.Count == 1 ? "" : "s")} selected";
        InfoLabel.Text = string.Empty;
        ImportButton.IsEnabled = filePaths.Count > 0;

        GridRadio.IsChecked = true;
    }

    public void ShowForRearrange(int spriteCount)
    {
        _filePaths = [];

        TitleLabel.Text = "Rearrange Layout";
        ImportButton.Content = "Apply";
        FileCountLabel.Text = $"{spriteCount} sprite{(spriteCount == 1 ? "" : "s")}";
        InfoLabel.Text = string.Empty;
        ImportButton.IsEnabled = spriteCount > 1;

        GridRadio.IsChecked = true;
    }

    private PackingLayout GetSelectedLayout()
    {
        if (ColumnRadio.IsChecked == true)
            return PackingLayout.SingleColumn;
        if (RowRadio.IsChecked == true)
            return PackingLayout.SingleRow;
        return PackingLayout.Grid;
    }

    private void OnImportClicked(object? sender, RoutedEventArgs e)
    {
        var layout = GetSelectedLayout();
        Close(new ImportImagesEventArgs(_filePaths, layout));
    }

    private void OnCancelClicked(object? sender, RoutedEventArgs e)
    {
        Close(null);
    }
}

public class ImportImagesEventArgs : EventArgs
{
    public IReadOnlyList<string> FilePaths { get; }
    public PackingLayout Layout { get; }

    public ImportImagesEventArgs(IReadOnlyList<string> filePaths, PackingLayout layout)
    {
        FilePaths = filePaths;
        Layout = layout;
    }
}

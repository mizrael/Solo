namespace SpriteSheetEditor.Controls;

public partial class ImportImagesDialog : ContentView
{
    private IReadOnlyList<string> _filePaths = [];

    public event EventHandler<ImportImagesEventArgs>? ImportClicked;
    public event EventHandler? CancelClicked;

    public ImportImagesDialog()
    {
        InitializeComponent();
    }

    public void Show(IReadOnlyList<string> filePaths)
    {
        _filePaths = filePaths;
        FileCountLabel.Text = $"{filePaths.Count} image{(filePaths.Count == 1 ? "" : "s")} selected";
        PaddingEntry.Text = "0";
        InfoLabel.Text = string.Empty;
        ImportButton.IsEnabled = filePaths.Count > 0;
        IsVisible = true;
    }

    public void Hide()
    {
        IsVisible = false;
    }

    private void OnImportClicked(object? sender, EventArgs e)
    {
        if (!int.TryParse(PaddingEntry.Text, out var padding) || padding < 0)
        {
            padding = 0;
        }

        ImportClicked?.Invoke(this, new ImportImagesEventArgs(_filePaths, padding));
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

    public ImportImagesEventArgs(IReadOnlyList<string> filePaths, int padding)
    {
        FilePaths = filePaths;
        Padding = padding;
    }
}

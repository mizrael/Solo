using SpriteSheetEditor.Services;

namespace SpriteSheetEditor.Controls;

public partial class GridSettingsDialog : ContentView
{
    private int _imageWidth;
    private int _imageHeight;
    private int _existingSpriteCount;

    public event EventHandler<GridSettingsEventArgs>? GenerateClicked;
    public event EventHandler? CancelClicked;

    public GridSettingsDialog()
    {
        InitializeComponent();
        ColumnsEntry.TextChanged += OnSettingsChanged;
        RowsEntry.TextChanged += OnSettingsChanged;
    }

    public void Show(int imageWidth, int imageHeight, int existingSpriteCount)
    {
        _imageWidth = imageWidth;
        _imageHeight = imageHeight;
        _existingSpriteCount = existingSpriteCount;
        IsVisible = true;
        UpdateInfo();
    }

    public void Hide()
    {
        IsVisible = false;
    }

    private void OnSettingsChanged(object? sender, TextChangedEventArgs e)
    {
        UpdateInfo();
    }

    private void UpdateInfo()
    {
        if (!int.TryParse(ColumnsEntry.Text, out var columns) || columns < 1 ||
            !int.TryParse(RowsEntry.Text, out var rows) || rows < 1)
        {
            InfoLabel.Text = string.Empty;
            WarningLabel.Text = string.Empty;
            GenerateButton.IsEnabled = false;
            return;
        }

        GenerateButton.IsEnabled = true;

        var (tileWidth, tileHeight) = GridGenerator.CalculateTileSize(_imageWidth, _imageHeight, columns, rows);
        var totalSprites = columns * rows;
        InfoLabel.Text = $"Tile size: {tileWidth}x{tileHeight} | {totalSprites} sprites";

        var warnings = new List<string>();

        if (GridGenerator.HasUncoveredPixels(_imageWidth, _imageHeight, columns, rows))
        {
            var (uncoveredX, uncoveredY) = GridGenerator.GetUncoveredPixels(_imageWidth, _imageHeight, columns, rows);
            warnings.Add($"{uncoveredX}px horizontal, {uncoveredY}px vertical uncovered");
        }

        if (_existingSpriteCount > 0)
        {
            warnings.Add($"Will replace {_existingSpriteCount} existing sprite(s)");
        }

        WarningLabel.Text = warnings.Count > 0 ? string.Join(" | ", warnings) : string.Empty;
    }

    private void OnGenerateClicked(object? sender, EventArgs e)
    {
        if (int.TryParse(ColumnsEntry.Text, out var columns) && columns >= 1 &&
            int.TryParse(RowsEntry.Text, out var rows) && rows >= 1)
        {
            GenerateClicked?.Invoke(this, new GridSettingsEventArgs(columns, rows));
        }
    }

    private void OnCancelClicked(object? sender, EventArgs e)
    {
        Hide();
        CancelClicked?.Invoke(this, EventArgs.Empty);
    }
}

public class GridSettingsEventArgs : EventArgs
{
    public int Columns { get; }
    public int Rows { get; }

    public GridSettingsEventArgs(int columns, int rows)
    {
        Columns = columns;
        Rows = rows;
    }
}

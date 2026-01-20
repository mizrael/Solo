using CommunityToolkit.Mvvm.ComponentModel;
using SpriteSheetEditor.Models;

namespace SpriteSheetEditor.ViewModels;

public enum EditorTool
{
    Select,
    Draw
}

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private SpriteSheetDocument _document = new() { SpriteSheetName = "untitled" };

    [ObservableProperty]
    private SpriteDefinition? _selectedSprite;

    [ObservableProperty]
    private EditorTool _currentTool = EditorTool.Select;

    [ObservableProperty]
    private float _zoomLevel = 1.0f;

    [ObservableProperty]
    private float _panOffsetX;

    [ObservableProperty]
    private float _panOffsetY;

    public int ImageWidth => Document.LoadedImage?.Width ?? 0;
    public int ImageHeight => Document.LoadedImage?.Height ?? 0;
    public int SpriteCount => Document.Sprites.Count;

    public void AddSprite(SpriteDefinition sprite)
    {
        Document.Sprites.Add(sprite);
        SelectedSprite = sprite;
        OnPropertyChanged(nameof(SpriteCount));
    }

    public void DeleteSelectedSprite()
    {
        if (SelectedSprite is null) return;

        Document.Sprites.Remove(SelectedSprite);
        SelectedSprite = null;
        OnPropertyChanged(nameof(SpriteCount));
    }

    public void ClearAllSprites()
    {
        Document.Sprites.Clear();
        SelectedSprite = null;
        OnPropertyChanged(nameof(SpriteCount));
    }

    public SpriteDefinition? FindSpriteAt(int x, int y)
    {
        for (int i = Document.Sprites.Count - 1; i >= 0; i--)
        {
            if (Document.Sprites[i].ContainsPoint(x, y))
                return Document.Sprites[i];
        }
        return null;
    }

    public void NotifyImageChanged()
    {
        OnPropertyChanged(nameof(ImageWidth));
        OnPropertyChanged(nameof(ImageHeight));
    }
}

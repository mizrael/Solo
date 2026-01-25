using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using SkiaSharp;
using SpriteSheetEditor.Models;
using SpriteSheetEditor.UndoRedo;

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

    [ObservableProperty]
    private bool _isFilterActive;

    [ObservableProperty]
    private SKBitmap? _originalImage;  // Backup of image before filter preview

    [ObservableProperty]
    private AnimationDefinition? _selectedAnimation;

    [ObservableProperty]
    private AnimationFrame? _selectedFrame;

    [ObservableProperty]
    private bool _isAnimationPlaying;

    [ObservableProperty]
    private int _currentPreviewFrameIndex;

    public ObservableCollection<SpriteDefinition> SelectedSprites { get; } = [];

    public UndoRedoManager UndoRedo { get; } = new();

    public int ImageWidth => Document.LoadedImage?.Width ?? 0;
    public int ImageHeight => Document.LoadedImage?.Height ?? 0;
    public int SpriteCount => Document.Sprites.Count;
    public int AnimationCount => Document.Animations.Count;

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

    public void NotifySpriteCountChanged()
    {
        OnPropertyChanged(nameof(SpriteCount));
    }

    public void BeginFilterPreview()
    {
        if (Document.LoadedImage is not null)
        {
            OriginalImage = Document.LoadedImage.Copy();
            IsFilterActive = true;
        }
    }

    public void ApplyFilter()
    {
        // Keep the current (filtered) image, discard backup
        OriginalImage?.Dispose();
        OriginalImage = null;
        IsFilterActive = false;
    }

    public void CancelFilter()
    {
        // Restore original image, discard filtered preview
        if (OriginalImage is not null)
        {
            Document.LoadedImage?.Dispose();
            Document.LoadedImage = OriginalImage;
            OriginalImage = null;
            NotifyImageChanged();
        }
        IsFilterActive = false;
    }

    public AnimationDefinition CreateNewAnimation()
    {
        var index = Document.Animations.Count;
        var animation = new AnimationDefinition
        {
            Name = $"animation_{index}",
            Fps = 10,
            Loop = true
        };
        return animation;
    }

    public void AddAnimation(AnimationDefinition animation)
    {
        Document.Animations.Add(animation);
        SelectedAnimation = animation;
        OnPropertyChanged(nameof(AnimationCount));
    }

    public void RemoveAnimation(AnimationDefinition animation)
    {
        Document.Animations.Remove(animation);
        if (SelectedAnimation == animation)
        {
            SelectedAnimation = Document.Animations.Count > 0 ? Document.Animations[0] : null;
        }
        OnPropertyChanged(nameof(AnimationCount));
    }

    public void DeleteSelectedAnimation()
    {
        if (SelectedAnimation is null) return;
        RemoveAnimation(SelectedAnimation);
    }

    public void AddSelectedSpritesToAnimation()
    {
        if (SelectedAnimation is null || SelectedSprites.Count == 0) return;

        foreach (var sprite in SelectedSprites)
        {
            var frame = new AnimationFrame
            {
                Sprite = sprite,
                SpriteName = sprite.Name
            };
            SelectedAnimation.Frames.Add(frame);
        }
    }

    public void RemoveFrameFromAnimation(AnimationFrame frame)
    {
        SelectedAnimation?.Frames.Remove(frame);
        if (SelectedFrame == frame)
        {
            SelectedFrame = null;
        }
    }

    public void NotifyAnimationCountChanged()
    {
        OnPropertyChanged(nameof(AnimationCount));
    }

    public void ClearSelection()
    {
        SelectedSprites.Clear();
        SelectedSprite = null;
    }

    public void ToggleSpriteSelection(SpriteDefinition sprite)
    {
        if (SelectedSprites.Contains(sprite))
        {
            SelectedSprites.Remove(sprite);
            if (SelectedSprite == sprite)
            {
                SelectedSprite = SelectedSprites.Count > 0 ? SelectedSprites[^1] : null;
            }
        }
        else
        {
            SelectedSprites.Add(sprite);
            SelectedSprite = sprite;
        }
    }

    public void SelectSprite(SpriteDefinition sprite)
    {
        SelectedSprites.Clear();
        SelectedSprites.Add(sprite);
        SelectedSprite = sprite;
    }
}

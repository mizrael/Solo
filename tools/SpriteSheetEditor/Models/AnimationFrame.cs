using CommunityToolkit.Mvvm.ComponentModel;

namespace SpriteSheetEditor.Models;

public partial class AnimationFrame : ObservableObject
{
    [ObservableProperty]
    private SpriteDefinition _sprite = null!;

    [ObservableProperty]
    private string _spriteName = string.Empty;
}

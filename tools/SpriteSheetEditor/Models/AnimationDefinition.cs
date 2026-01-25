using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace SpriteSheetEditor.Models;

public partial class AnimationDefinition : ObservableObject
{
    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private int _fps = 10;

    [ObservableProperty]
    private bool _loop = true;

    [JsonIgnore]
    public ObservableCollection<AnimationFrame> Frames { get; } = [];
}

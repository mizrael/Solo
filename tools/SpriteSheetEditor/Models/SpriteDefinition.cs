using System.Drawing;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace SpriteSheetEditor.Models;

public partial class SpriteDefinition : ObservableObject
{
    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private int _x;

    [ObservableProperty]
    private int _y;

    [ObservableProperty]
    private int _width;

    [ObservableProperty]
    private int _height;

    [JsonIgnore]
    public Rectangle Bounds => new(X, Y, Width, Height);

    public bool ContainsPoint(int px, int py)
    {
        return px >= X && px < X + Width && py >= Y && py < Y + Height;
    }
}

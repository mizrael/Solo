using System.Drawing;
using System.Text.Json.Serialization;

namespace SpriteSheetEditor.Models;

public class SpriteDefinition
{
    public string Name { get; set; } = string.Empty;
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }

    [JsonIgnore]
    public Rectangle Bounds => new(X, Y, Width, Height);

    public bool ContainsPoint(int px, int py)
    {
        return px >= X && px < X + Width && py >= Y && py < Y + Height;
    }
}

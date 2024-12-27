using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Solo.Assets;

public record Sprite
{
    public Sprite(string name, Texture2D texture, Rectangle bounds)
    {
        Name = name;            
        Texture = texture;

        Bounds = bounds;
    }

    public Sprite(string name, Texture2D texture) 
        : this(name, texture, new Rectangle(0, 0, texture.Width, texture.Height))
    {
    }

    public string Name { get; }
    public Texture2D Texture { get; }

    private Rectangle _bounds;
    public Rectangle Bounds
    {
        get => _bounds;
        set
        {
            _bounds = value;
            Center = new Vector2((float)value.Width * .5f, (float)value.Height * .5f);
        }
    }

    /// <summary>
    /// Sets the center of the sprite.
    /// Will be recalculated when <see cref="Sprite.Bounds"/> is set.
    /// </summary>
    public Vector2 Center { get; set; }        

    public static Sprite FromTexture(string name, ContentManager contentManager)
    {
        var texture = contentManager.Load<Texture2D>(name);
        return new Sprite(name, texture);
    }
}

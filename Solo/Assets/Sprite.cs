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
        Center = new Vector2(bounds.Width / 2, bounds.Height / 2);
    }

    public Sprite(string name, Texture2D texture) 
        : this(name, texture, new Rectangle(0, 0, texture.Width, texture.Height))
    {
    }

    public string Name { get; }
    public Texture2D Texture { get; }

    public Rectangle Bounds { get; set; }
    public Vector2 Center { get; set; }        

    public static Sprite FromTexture(string name, ContentManager contentManager)
    {
        var texture = contentManager.Load<Texture2D>(name);
        return new Sprite(name, texture);
    }
}

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Monoroids.Core.Assets
{
    public record Sprite
    {
        public Sprite(string name, Rectangle bounds, Texture2D texture)
        {
            Name = name;
            Bounds = bounds;
            Origin = new Point(bounds.Width / 2, bounds.Height / 2);
            Texture = texture;
        }

        public string Name { get; }
        public Rectangle Bounds { get; }
        public Point Origin { get; }
        public Texture2D Texture { get; }
    }
}

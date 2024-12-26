using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Solo.Assets;

// TODO: this has to go away, in favor of AnimatedSpriteSheet
public record Animation
{
    public Animation(
        Texture2D texture, string name,
        int fps, int framesCount, Point frameSize)
    {
        Name = name;
        Fps = fps;
        FrameSize = frameSize;
        FrameCenter = new Vector2((float)frameSize.X * .5f, (float)frameSize.Y * .5f); 
        FramesCount = framesCount;                        
        Texture = texture;
    }

    public string Name { get; }
    public int Fps { get; }
    public int FramesCount { get; }
    public Point FrameSize { get; }
    public Vector2 FrameCenter { get; }
    public Texture2D Texture { get; }
}
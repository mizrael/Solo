using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Solo.Assets;

public record AnimatedSpriteSheet(string Name, Texture2D Texture, int Fps, AnimatedSpriteSheet.Frame[] Frames)
{
    public record Frame
    {
        public readonly Vector2 Center;
        public readonly Rectangle Bounds;

        public Frame(Rectangle bounds)
        {
            this.Center = new(bounds.Width * 0.5f, bounds.Height * 0.5f);
            this.Bounds = bounds;
        }
    }

    private Lazy<TimeSpan> _duration = new (() =>
    {
        var totalFrames = Frames.Length;
        var duration = TimeSpan.FromSeconds(totalFrames / (float)Fps);
        return duration;
    }, isThreadSafe: true);

    public TimeSpan Duration => _duration.Value;
}

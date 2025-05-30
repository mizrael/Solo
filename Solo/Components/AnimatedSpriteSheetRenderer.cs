using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Solo.Assets;
using Solo.Services;

namespace Solo.Components;

public class AnimatedSpriteSheetRenderer : Component, IRenderable
{
    private int _currentFrame;
    private double _lastUpdate;
    private AnimatedSpriteSheet _animation;
    private TransformComponent _transform;

    public AnimatedSpriteSheetRenderer(GameObject owner) : base(owner)
    {
    }

    protected override void InitCore()
    {
        _transform = Owner.Components.Get<TransformComponent>();
    }

    protected override void UpdateCore(GameTime gameTime)
    {
        if (Animation is null) return;

        var needUpdate = gameTime.TotalGameTime.TotalMilliseconds - _lastUpdate > 1000f / Animation.Fps;
        if (!needUpdate)
            return;

        _lastUpdate = gameTime.TotalGameTime.TotalMilliseconds;

        _currentFrame++;
        if (_currentFrame >= Animation.Frames.Length)
        {
            _currentFrame = 0;
            OnAnimationComplete?.Invoke(this);
        }
    }

    public void Render(SpriteBatch spriteBatch)
    {
        if (Animation is null)
            return;

        var frame = Animation.Frames[_currentFrame];

        spriteBatch.Draw(Animation.Texture,
            position: _transform.World.Position,
            sourceRectangle: frame.Bounds,
            color: Color.White,
            rotation: _transform.World.Rotation,
            origin: frame.Center,
            scale: _transform.World.Scale,
            SpriteEffects.None,
            layerDepth: 0f);
    }

    public void Reset()
    {
        _currentFrame = 0;
        _lastUpdate = 0;
    }

    public AnimatedSpriteSheet Animation
    {
        get => _animation;
        set
        {
            if (_animation == value)
                return;
            Reset();
            _animation = value;
        }
    }

    public event OnAnimationCompleteHandler OnAnimationComplete;
    public delegate void OnAnimationCompleteHandler(AnimatedSpriteSheetRenderer renderer);

    public int LayerIndex { get; set; }
    public bool Hidden { get; set; }

    public AnimatedSpriteSheet.Frame? CurrentFrame => Animation?.Frames[_currentFrame];
}
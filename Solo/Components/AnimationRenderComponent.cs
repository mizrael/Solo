using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Solo.Assets;
using Solo.Services;

namespace Solo.Components;

public class AnimationRenderComponent : Component, IRenderable
{
    private TransformComponent _transform;

    private int _currFramePosX = 0;
    private int _currFramePosY = 0;
    private int _currFrameIndex = 0;
    private double _lastUpdate = 0;

    private Animation _animation;

    private AnimationRenderComponent(GameObject owner) : base(owner)
    {
    }

    protected override void InitCore()
    {
        _transform = Owner.Components.Get<TransformComponent>();
    }

    protected override void UpdateCore(GameTime gameTime)
    {
        if (null == Animation || !this.Owner.Enabled)
            return;

        var needUpdate = (gameTime.TotalGameTime.TotalMilliseconds - _lastUpdate > 1000f / Animation.Fps);
        if (!needUpdate)
            return;

        _lastUpdate = gameTime.TotalGameTime.TotalMilliseconds;

        _currFramePosX += Animation.FrameSize.X;
        if (_currFramePosX >= Animation.Texture.Width)
        {
            _currFramePosX = 0;
            _currFramePosY += Animation.FrameSize.Y;
        }

        if (_currFramePosY >= Animation.Texture.Height)
            _currFramePosY = 0;

        _currFrameIndex++;
        if (_currFrameIndex >= Animation.FramesCount)
        {
            this.Reset();
            this.OnAnimationComplete?.Invoke(this);            
        }
    }

    public void Render(SpriteBatch spriteBatch)
    {
        if (null == Animation || !this.Owner.Enabled)
            return;

        var sourceRectangle = new Rectangle(
            _currFramePosX, _currFramePosY, 
            Animation.FrameSize.X, Animation.FrameSize.Y);

        spriteBatch.Draw(Animation.Texture,
            position: _transform.World.Position,
            sourceRectangle: sourceRectangle,
            color: Color.White,
            rotation: _transform.World.Rotation,
            origin: Animation.FrameCenter,
            scale: _transform.World.Scale,
            SpriteEffects.None,
            layerDepth: 0f);

        //context.Translate(_transform.World.Position.X + (MirrorVertically ? Animation.FrameSize.Width : 0f), _transform.World.Position.Y);
        //context.Rotate(_transform.World.Rotation);
        //context.Scale(_transform.World.Scale.X * (MirrorVertically ? -1f : 1f), _transform.World.Scale.Y);

        //context.DrawImage(Animation.ImageRef,
        //    _currFramePosX, _currFramePosY,
        //    Animation.FrameSize.Width, Animation.FrameSize.Height,
        //    Animation.HalfFrameSize.Width, Animation.HalfFrameSize.Height,
        //    -Animation.FrameSize.Width, -Animation.FrameSize.Height);
    }

    public Animation Animation
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

    public void Reset()
    {
        _currFramePosX = 0;
        _currFramePosY = 0;
        _currFrameIndex = 0;
    }

    public event OnAnimationCompleteHandler OnAnimationComplete;
    public delegate void OnAnimationCompleteHandler(AnimationRenderComponent renderer);

    public int LayerIndex { get; set; }
    public bool Hidden { get; set; }
}
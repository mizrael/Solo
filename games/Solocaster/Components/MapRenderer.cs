using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Solo;
using Solo.Components;
using Solo.Services;
using Solocaster.Components;
using Solocaster.Entities;

namespace Solocaster;

//TODO: Handle screen resize
public class MapRenderer : Component, IRenderable
{
    private readonly GameObject _player;
    private TransformComponent _playerTransform;
    private PlayerBrain _playerBrain;

    private readonly Map _map;
    private readonly Raycaster _raycaster;
    private readonly Texture2D _frameTexture;
    
    private readonly int _screenWidth;
    private readonly int _screenHeight;
    private readonly int _frameBufferWidth;
    private readonly int _frameBufferHeight;

    private readonly Vector2 _halfScreenSize;
    private readonly Vector2 _halfFrameBufferSize;
    private readonly Vector2 _frameBufferScale;

    public MapRenderer(GameObject owner,
                      GameObject player, 
                      Map map, 
                      Raycaster raycaster, 
                      Texture2D frameTexture) : base(owner)
    {
        _player = player;
        _map = map;
        _raycaster = raycaster;

        var viewport = GraphicsDeviceManagerAccessor.Instance.GraphicsDeviceManager.GraphicsDevice.Viewport;
        _screenWidth = viewport.Width;
        _screenHeight = viewport.Height;

        _frameTexture = frameTexture;
        _frameBufferWidth = frameTexture.Width;
        _frameBufferHeight = frameTexture.Height;

        _halfScreenSize = new Vector2(_screenWidth / 2, _screenHeight / 2);
        _halfFrameBufferSize = new Vector2(_frameBufferWidth / 2, _frameBufferHeight / 2);
        _frameBufferScale = new Vector2(
            (float)_screenHeight / _frameBufferWidth,
            (float)_screenWidth / _frameBufferHeight);
    }

    protected override void InitCore()
    {
        _playerTransform = _player.Components.Get<TransformComponent>();
        _playerBrain = _player.Components.Get<PlayerBrain>();

        base.InitCore();
    }

    protected override void UpdateCore(GameTime gameTime)
    {
        _map.Update(gameTime);

        // Convert screen mouse position to framebuffer coordinates
        var mouseState = Mouse.GetState();
        var mouseFrameBufferPos = ScreenToFrameBuffer(mouseState.X, mouseState.Y);

        _raycaster.Update(_playerTransform, _playerBrain, mouseFrameBufferPos);
        _frameTexture.SetData(_raycaster.FrameBuffer);
    }

    /// <summary>
    /// Converts screen coordinates to framebuffer coordinates, accounting for the 90° CCW rotation and scaling applied
    /// during rendering.
    /// </summary>
    private Vector2 ScreenToFrameBuffer(int screenX, int screenY)
    {
        // The framebuffer is drawn with:
        // 1. Origin at framebuffer center
        // 2. 90° CCW rotation
        // 3. Scale applied
        // 4. Positioned at screen center
        //
        // Forward transform: fb(fx,fy) -> screen(sx,sy)
        //   sx = scaleX * (fbH/2 - fy) + screenW/2
        //   sy = scaleY * (fx - fbW/2) + screenH/2
        //
        // Inverse: screen(sx,sy) -> fb(fx,fy)
        //   fx = (sy - screenH/2) / scaleY + fbW/2
        //   fy = fbH/2 - (sx - screenW/2) / scaleX

        float px = screenX - _halfScreenSize.X;
        float py = screenY - _halfScreenSize.Y;

        float fbX = py / _frameBufferScale.Y + _halfFrameBufferSize.X;
        float fbY = _halfFrameBufferSize.Y - px / _frameBufferScale.X;

        return new Vector2(fbX, fbY);
    }

    public void Render(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(
            _frameTexture,
            position: _halfScreenSize,
            sourceRectangle: null,
            color: Color.White,
            rotation: MathHelper.PiOver2,
            origin: _halfFrameBufferSize,
            scale: _frameBufferScale,
            effects: SpriteEffects.None,
            layerDepth: 0);
    }

    public int LayerIndex { get; set; }
    public bool Hidden { get; set; }
}

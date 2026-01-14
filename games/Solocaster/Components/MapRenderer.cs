using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Solo;
using Solo.Components;
using Solo.Services;
using Solocaster.Components;
using Solocaster.Entities;

namespace Solocaster;

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
                      Map map, Raycaster raycaster, Texture2D frameTexture) : base(owner)
    {
        _player = player;
        _map = map;
        _raycaster = raycaster;

        var renderService = GameServicesManager.Instance.GetRequired<RenderService>();
        _screenWidth = renderService.Graphics.GraphicsDevice.Viewport.Width;
        _screenHeight = renderService.Graphics.GraphicsDevice.Viewport.Height;

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
        _raycaster.Update(_playerTransform, _playerBrain);
        _frameTexture.SetData(_raycaster.FrameBuffer);
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

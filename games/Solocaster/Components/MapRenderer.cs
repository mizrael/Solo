using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoRaycaster;
using Solo;
using Solo.Components;
using Solo.Services;
using Solocaster.Entities;

namespace Solocaster;

public class MapRenderer : Component, IRenderable
{
    private Camera _camera;
    private Map _map;
    private Raycaster _raycaster;
    private Texture2D _frameTexture;
    
    private int _screenWidth;
    private int _screenHeight;
    private int _frameBufferWidth;
    private int _frameBufferHeight;

    private Vector2 _halfScreenSize;
    private Vector2 _halfFrameBufferSize;
    private Vector2 _frameBufferScale;

    public MapRenderer(GameObject owner,
                      Camera camera, Map map, Raycaster raycaster, Texture2D frameTexture) : base(owner)
    {
        _camera = camera;
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

    protected override void UpdateCore(GameTime gameTime)
    {
        _camera.Update(gameTime);
        _map.Update(gameTime);
        _raycaster.Update(_camera);
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

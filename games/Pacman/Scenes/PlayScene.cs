using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Solo;
using Solo.Assets;
using Solo.Components;
using Solo.Services;
using System;

namespace Pacman.Scenes;

public class PlayScene : Scene
{
    public PlayScene(Game game) : base(game)
    {
    }

    protected override void EnterCore()
    {
        AddMap();
    }

    private void AddMap()
    {
        var map = new GameObject();
        
        map.Components.Add<MapBrainComponent>();
        var transform = map.Components.Add<TransformComponent>();

        var sprite = Sprite.FromTexture("map", Game.Content);
        var renderer = map.Components.Add<SpriteRenderComponent>();
        renderer.Sprite = sprite;
        renderer.LayerIndex = (int)RenderLayers.Background;

        var renderService = GameServicesManager.Instance.GetService<RenderService>();
        renderService.SetLayerConfig((int)RenderLayers.Background, new RenderLayerConfig
        {
            SamplerState = SamplerState.LinearClamp
        });
        var calculateSize = new Action(() =>
        {
            transform.Local.Position.X = renderService.Graphics.GraphicsDevice.Viewport.Width / 2;
            transform.Local.Position.Y = renderService.Graphics.GraphicsDevice.Viewport.Height / 2;

            transform.Local.Scale.X = renderService.Graphics.GraphicsDevice.Viewport.Width / (float)sprite.Bounds.Width;
            transform.Local.Scale.Y = renderService.Graphics.GraphicsDevice.Viewport.Height / (float)sprite.Bounds.Height;
        });
        calculateSize();

        renderService.Window.ClientSizeChanged += (s, e) => calculateSize();

        this.Root.AddChild(map);
    }
}


public class MapBrainComponent : Component
#if DEBUG
    , IRenderable
#endif
{
    private Texture2D _pixelTexture;
    private Point _tileSize = Point.Zero;

    private static int[,] _tiles =
    {
        { 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2 },
        { 2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2, 2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2 },
        { 2, 1, 2, 2, 2, 2, 1, 2, 2, 2, 2, 2, 1, 2, 2, 1, 2, 2, 2, 2, 2, 1, 2, 2, 2, 2, 1, 2 },
        { 2, 4, 2, 2, 2, 2, 1, 2, 2, 2, 2, 2, 1, 2, 2, 1, 2, 2, 2, 2, 2, 1, 2, 2, 2, 2, 4, 2 },
        { 2, 1, 2, 2, 2, 2, 1, 2, 2, 2, 2, 2, 1, 2, 2, 1, 2, 2, 2, 2, 2, 1, 2, 2, 2, 2, 1, 2 },
        { 2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2 },
        { 2, 1, 2, 2, 2, 2, 1, 2, 2, 1, 2, 2, 2, 2, 2, 2, 2, 2, 1, 2, 2, 1, 2, 2, 2, 2, 1, 2 },
        { 2, 1, 2, 2, 2, 2, 1, 2, 2, 1, 2, 2, 2, 2, 2, 2, 2, 2, 1, 2, 2, 1, 2, 2, 2, 2, 1, 2 },
        { 2, 1, 1, 1, 1, 1, 1, 2, 2, 1, 1, 1, 1, 2, 2, 1, 1, 1, 1, 2, 2, 1, 1, 1, 1, 1, 1, 2 },
        { 2, 2, 2, 2, 2, 2, 1, 2, 2, 2, 2, 2, 0, 2, 2, 0, 2, 2, 2, 2, 2, 1, 2, 2, 2, 2, 2, 2 },
        { 2, 2, 2, 2, 2, 2, 1, 2, 2, 2, 2, 2, 0, 2, 2, 0, 2, 2, 2, 2, 2, 1, 2, 2, 2, 2, 2, 2 },
        { 2, 2, 2, 2, 2, 2, 1, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 2, 1, 2, 2, 2, 2, 2, 2 },
        { 2, 2, 2, 2, 2, 2, 1, 2, 2, 0, 2, 2, 2, 0, 0, 2, 2, 2, 0, 2, 2, 1, 2, 2, 2, 2, 2, 2 },
        { 2, 2, 2, 2, 2, 2, 1, 2, 2, 0, 2, 0, 0, 0, 0, 0, 0, 2, 0, 2, 2, 1, 2, 2, 2, 2, 2, 2 },
        { 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 2, 0, 0, 0, 0, 0, 0, 2, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0 },
        { 2, 2, 2, 2, 2, 2, 1, 2, 2, 0, 2, 0, 0, 0, 0, 0, 0, 2, 0, 2, 2, 1, 2, 2, 2, 2, 2, 2 },
        { 2, 2, 2, 2, 2, 2, 1, 2, 2, 0, 2, 2, 2, 2, 2, 2, 2, 2, 0, 2, 2, 1, 2, 2, 2, 2, 2, 2 },
        { 2, 2, 2, 2, 2, 2, 1, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 2, 1, 2, 2, 2, 2, 2, 2 },
        { 2, 2, 2, 2, 2, 2, 1, 2, 2, 0, 2, 2, 2, 2, 2, 2, 2, 2, 0, 2, 2, 1, 2, 2, 2, 2, 2, 2 },
        { 2, 2, 2, 2, 2, 2, 1, 2, 2, 0, 2, 2, 2, 2, 2, 2, 2, 2, 0, 2, 2, 1, 2, 2, 2, 2, 2, 2 },
        { 2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2, 2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2 },
        { 2, 1, 2, 2, 2, 2, 1, 2, 2, 2, 2, 2, 1, 2, 2, 1, 2, 2, 2, 2, 2, 1, 2, 2, 2, 2, 1, 2 },
        { 2, 1, 2, 2, 2, 2, 1, 2, 2, 2, 2, 2, 1, 2, 2, 1, 2, 2, 2, 2, 2, 1, 2, 2, 2, 2, 1, 2 },
        { 2, 4, 1, 1, 2, 2, 1, 1, 1, 1, 1, 1, 1, 0, 0, 1, 1, 1, 1, 1, 1, 1, 2, 2, 1, 1, 4, 2 },
        { 2, 2, 2, 1, 2, 2, 1, 2, 2, 1, 2, 2, 2, 2, 2, 2, 2, 2, 1, 2, 2, 1, 2, 2, 1, 2, 2, 2 },
        { 2, 2, 2, 1, 2, 2, 1, 2, 2, 1, 2, 2, 2, 2, 2, 2, 2, 2, 1, 2, 2, 1, 2, 2, 1, 2, 2, 2 },
        { 2, 1, 1, 1, 1, 1, 1, 2, 2, 1, 1, 1, 1, 2, 2, 1, 1, 1, 1, 2, 2, 1, 1, 1, 1, 1, 1, 2 },
        { 2, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 1, 2, 2, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 1, 2 },
        { 2, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 1, 2, 2, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 1, 2 },
        { 2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2 },
        { 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2 },
    };

    public MapBrainComponent(GameObject owner) : base(owner)
    {
        this.LayerIndex = (int)RenderLayers.UI;
    }

    protected override void InitCore()
    {
        var renderService = GameServicesManager.Instance.GetService<RenderService>();
        var calculateSize = new Action(() =>
        {
            _tileSize.X = (int)((float)renderService.Graphics.GraphicsDevice.Viewport.Width / _tiles.GetLength(1));
            _tileSize.Y = (int)((float)renderService.Graphics.GraphicsDevice.Viewport.Height / _tiles.GetLength(0));
        });
        calculateSize();

        renderService.Window.ClientSizeChanged += (s, e) => calculateSize();
    }

    #region Debug Rendering

    public void Render(SpriteBatch spriteBatch)
    {
        if (_pixelTexture is null)
        {
            var renderService = GameServicesManager.Instance.GetService<RenderService>();
            _pixelTexture = Texture2DUtils.Generate(renderService.Graphics.GraphicsDevice, 1, 1, Color.White);
        }

        for (int j = 0; j < _tiles.GetLength(1); j++) 
        {
            for (int i = 0; i < _tiles.GetLength(0); i++)
            {
                int topLeftX = (int)_tileSize.X * j,
                    topLeftY = (int)_tileSize.Y * i;

                var color = _tiles[i, j] switch
                {
                    0 => Color.Black,
                    1 => Color.White,
                    2 => Color.Blue,
                    3 => Color.Red,
                    4 => Color.Green,
                    _ => Color.Gray
                };

                spriteBatch.Draw(_pixelTexture, new Rectangle(topLeftX, topLeftY, _tileSize.X, 1), color);
                spriteBatch.Draw(_pixelTexture, new Rectangle(topLeftX, topLeftY, 1, _tileSize.Y), color);
                spriteBatch.Draw(_pixelTexture, new Rectangle(topLeftX + _tileSize.X - 1, topLeftY, 1, _tileSize.Y), color);
                spriteBatch.Draw(_pixelTexture, new Rectangle(topLeftX, topLeftY + _tileSize.Y - 1, _tileSize.X, 1), color);
            }
        }

    }

    public int LayerIndex { get; set; }
    public bool Hidden { get; set; }

    #endregion Debug Rendering
}
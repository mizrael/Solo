using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Solo;
using Solo.Assets;
using Solo.Components;
using Solo.Services;
using System;
using System.Collections.Generic;

namespace Pacman.Components;

public class MapLogicComponent : Component
#if DEBUG
   // , IRenderable
#endif
{
    private Texture2D _pixelTexture;
    private Vector2 _tileSize = Vector2.Zero;
    private Vector2 _tileCenter = Vector2.Zero;
    private Vector2 _posOffset = Vector2.Zero;

    public Vector2 TileSize => _tileSize;

    private TransformComponent _transform;

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

    public MapLogicComponent(GameObject owner) : base(owner)
    {
        LayerIndex = (int)RenderLayers.UI;
    }

    protected override void InitCore()
    {
        _transform = Owner.Components.Get<TransformComponent>();

        var renderer = Owner.Components.Add<SpriteRenderComponent>();

        var renderService = GameServicesManager.Instance.GetRequired<RenderService>();
        var calculateSize = new Action(() =>
        {
            _posOffset.Y = renderService.Graphics.GraphicsDevice.Viewport.Height * 0.05f;
            _posOffset.X = renderService.Graphics.GraphicsDevice.Viewport.Width * .125f; 

            var height = renderService.Graphics.GraphicsDevice.Viewport.Height - _posOffset.Y;
            var width = renderService.Graphics.GraphicsDevice.Viewport.Width - _posOffset.X * 2f;

            _tileSize.X = width / _tiles.GetLength(1);
            _tileSize.Y = height / _tiles.GetLength(0);

            _tileCenter = new Vector2(_tileSize.X * .5f, _tileSize.Y * .5f);          

            _transform.Local.Position.X = width * .5f + _posOffset.X;
            _transform.Local.Position.Y = height * .5f + _posOffset.Y;

            _transform.Local.Scale.X = width / (float)renderer.Sprite.Bounds.Width;
            _transform.Local.Scale.Y = height / (float)renderer.Sprite.Bounds.Height;
        });
        calculateSize();

        renderService.Window.ClientSizeChanged += (s, e) => calculateSize();
    }

    public (int row, int col) GetPlayerStartTile() => (14, 1);

    public (int row, int col) GetGhostStartTile(Ghosts ghost) 
        => ghost switch {
            Ghosts.Blinky => (14, 12),
            Ghosts.Pinky => (14, 13),
            Ghosts.Inky => (14, 14),
            Ghosts.Clyde => (14, 15),
            _ => (14, 26),
        };

    public Vector2 GetTileCenter(int row, int col)
        => new Vector2(
            col * _tileSize.X + _tileCenter.X + _posOffset.X, 
            row * _tileSize.Y + _tileCenter.Y + _posOffset.Y);

    public bool IsWalkable(int row, int col)
        => row < _tiles.GetLength(0) && row > -1 &&
           col < _tiles.GetLength(1) && col > -1 &&
           _tiles[row, col] != (int)TileTypes.Wall;

    public bool IsWalkable(Vector2 position)
    {
        var (row, col) = GetTileIndex(position);
        return IsWalkable(row, col);
    }

    public (int row, int col) GetTileIndex(Vector2 position)
    {
        var row = (int)((position.Y - _posOffset.Y) / _tileSize.Y);
        var col = (int)((position.X - _posOffset.X) / _tileSize.X);
        return (row, col);
    }

    public IEnumerable<(int row, int col)> GetTilesByType(TileTypes type)
    {
        for (int j = 0; j < _tiles.GetLength(1); j++)
        {
            for (int i = 0; i < _tiles.GetLength(0); i++)
            {
                if (_tiles[i, j] == (int)type)
                    yield return (i, j);
            }
        }
    }

    public int Cols => _tiles.GetLength(1);
    public int Rows => _tiles.GetLength(0);

    #region Debug Rendering

    public void Render(SpriteBatch spriteBatch)
    {
        if (_pixelTexture is null)
        {
            var renderService = GameServicesManager.Instance.GetRequired<RenderService>();
            _pixelTexture = Texture2DUtils.Generate(renderService.Graphics.GraphicsDevice, 1, 1, new Color(Color.LightGray, 200));
        }

        for (int j = 0; j < _tiles.GetLength(1); j++)
        {
            for (int i = 0; i < _tiles.GetLength(0); i++)
            {
                Vector2 pos = new Vector2(j * _tileSize.X, i * _tileSize.Y);

                var color = _tiles[i, j] switch
                {
                    0 => Color.Black,
                    1 => Color.White,
                    2 => Color.Blue,
                    3 => Color.Red,
                    4 => Color.Green,
                    _ => Color.Gray
                };

                spriteBatch.Draw(_pixelTexture, 
                    pos, 
                    sourceRectangle: null,
                    color,
                    rotation: 0f,
                    origin: Vector2.Zero,
                    scale: _tileSize,
                    SpriteEffects.None,
                    layerDepth: 0f);
            }
        }

    }

    public int LayerIndex { get; set; }
    public bool Hidden { get; set; }

    #endregion Debug Rendering
}
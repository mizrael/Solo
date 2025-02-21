using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Solo;
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

    public Vector2 TileSize => _tileSize;

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
        var renderService = GameServicesManager.Instance.GetService<RenderService>();
        var calculateSize = new Action(() =>
        {
            _tileSize.X = (float)renderService.Graphics.GraphicsDevice.Viewport.Width / _tiles.GetLength(1);
            _tileSize.Y = (float)renderService.Graphics.GraphicsDevice.Viewport.Height / _tiles.GetLength(0);

            _tileCenter = new Vector2(_tileSize.X * .5f, _tileSize.Y * .5f);
        });
        calculateSize();

        renderService.Window.ClientSizeChanged += (s, e) => calculateSize();
    }

    public (int row, int col) GetPlayerStartTile() => (1, 1);

    public Vector2 GetTileCenter(int row, int col)
        => new Vector2(col * _tileSize.X + _tileCenter.X, row * _tileSize.Y + _tileCenter.Y);

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
        var row = (int)(position.Y / _tileSize.Y);
        var col = (int)(position.X / _tileSize.X);
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
            var renderService = GameServicesManager.Instance.GetService<RenderService>();
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

public enum TileTypes 
{
    Empty = 0,
    Pellet = 1,
    Wall = 2
}
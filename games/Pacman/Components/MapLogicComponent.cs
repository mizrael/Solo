using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Solo;
using Solo.AI;
using Solo.Components;
using Solo.Services;
using System;
using System.Collections.Generic;

namespace Pacman.Components;

public class MapLogicComponent : Component
#if DEBUG
    //, IRenderable
#endif
{
    private Texture2D _pixelTexture;
    private Vector2 _tileSize = Vector2.Zero;
    private Vector2 _tileCenter = Vector2.Zero;
    private Vector2 _posOffset = Vector2.Zero;

    public Vector2 TileSize => _tileSize;

    private TransformComponent _transform;

    private readonly static int[,] _tiles =
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

    private readonly TileInfo[,] _tileInfos;

    private Func<TileInfo, IEnumerable<TileInfo>> _findNeighboursFunc;

    public MapLogicComponent(GameObject owner) : base(owner)
    {
        _tileInfos = BuildTileInfoMatrix();
        LayerIndex = (int)RenderLayers.UI;

        _findNeighboursFunc = t => GetNeighbours(t, n => null != n && n.IsWalkable);
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

    public TileInfo GetPlayerStartTile() => _tileInfos[14, 1];

    public TileInfo GetGhostStartTile(Ghosts ghost) 
        => ghost switch {
            Ghosts.Blinky => _tileInfos[11, 13],
            Ghosts.Pinky => _tileInfos[14, 13],
            Ghosts.Inky => _tileInfos[14, 14],
            Ghosts.Clyde => _tileInfos[14, 15],
            _ => _tileInfos[14, 26],
        };

    public Vector2 GetTileCenter(TileInfo tile)
         => new Vector2(
            tile.Col * _tileSize.X + _tileCenter.X + _posOffset.X,
            tile.Row * _tileSize.Y + _tileCenter.Y + _posOffset.Y);

    public TileInfo? GetTileAt(Vector2 position)
    {
        var row = (int)((position.Y - _posOffset.Y) / _tileSize.Y);
        var col = (int)((position.X - _posOffset.X) / _tileSize.X);
        
        return row < RowsCount && row > -1 &&
           col < ColsCount && col > -1 ?
             _tileInfos[row, col] : null;
    }

    public TileInfo? GetTileAt(int row, int col)
        => row < RowsCount && row > -1 &&
           col < ColsCount && col > -1 ? 
        _tileInfos[row, col] : null;
    
    public IEnumerable<TileInfo> GetTilesByType(TileTypes type)
    {
        for (int row = 0; row < RowsCount; row++)
        {
            for (int col = 0; col < ColsCount; col++)
            {
                if (_tileInfos[row, col].Type == type)
                    yield return _tileInfos[row, col];
            }
        }
    }

    public Path<TileInfo> FindPath(TileInfo start, TileInfo destination)
    {
        if(start is null)
            return Path<TileInfo>.Empty; // TODO: this should probably throw
        if (destination is null)
            return new Path<TileInfo>([start]);

        if (start == destination)
            return new Path<TileInfo>([destination]);

        return Pathfinder.FindPath(start, destination,
                                   TileInfo.Distance,
                                   _findNeighboursFunc);
    }

    public int ColsCount => _tiles.GetLength(1);
    public int RowsCount => _tiles.GetLength(0);

    #region Private methods

    private TileInfo[,] BuildTileInfoMatrix()
    {
        var tileInfos = new TileInfo[RowsCount, ColsCount];
        for (int row = 0; row < RowsCount; row++) 
        {
            for (int col = 0; col < ColsCount; col++)
            {
                tileInfos[row, col] = new TileInfo(row, col, (TileTypes)_tiles[row,col]);
            }
        }
        return tileInfos;
    }

    private TileInfo[] GetNeighbours(TileInfo tile, Predicate<TileInfo> filter)
    {
        var results = new List<TileInfo>(8);

        int x = tile.Row - 1;
        int y = tile.Col - 1;
        if (x > -1 && x < RowsCount && y > -1 && y < ColsCount && filter(_tileInfos[x, y]))
            results.Add(_tileInfos[x, y]);

        x = tile.Row;
        y = tile.Col - 1;
        if (x > -1 && x < RowsCount && y > -1 && y < ColsCount && filter(_tileInfos[x, y]))
            results.Add(_tileInfos[x, y]);

        x = tile.Row + 1;
        y = tile.Col - 1;
        if (x > -1 && x < RowsCount && y > -1 && y < ColsCount && filter(_tileInfos[x, y]))
            results.Add(_tileInfos[x, y]);

        x = tile.Row - 1;
        y = tile.Col;
        if (x > -1 && x < RowsCount && y > -1 && y < ColsCount && filter(_tileInfos[x, y]))
            results.Add(_tileInfos[x, y]);

        x = tile.Row + 1;
        y = tile.Col;
        if (x > -1 && x < RowsCount && y > -1 && y < ColsCount && filter(_tileInfos[x, y]))
            results.Add(_tileInfos[x, y]);

        x = tile.Row - 1;
        y = tile.Col + 1;
        if (x > -1 && x < RowsCount && y > -1 && y < ColsCount && filter(_tileInfos[x, y]))
            results.Add(_tileInfos[x, y]);

        x = tile.Row;
        y = tile.Col + 1;
        if (x > -1 && x < RowsCount && y > -1 && y < ColsCount && filter(_tileInfos[x, y]))
            results.Add(_tileInfos[x, y]);

        x = tile.Row + 1;
        y = tile.Col + 1;
        if (x > -1 && x < RowsCount && y > -1 && y < ColsCount && filter(_tileInfos[x, y]))
            results.Add(_tileInfos[x, y]);

        return results.ToArray();
    }

    #endregion

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
                Vector2 pos = new Vector2(j * _tileSize.X + _posOffset.X, i * _tileSize.Y + _posOffset.Y);

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

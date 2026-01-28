using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Solo;
using Solo.Components;
using Solo.Services;
using Solocaster.Entities;
using System;
using System.Collections.Generic;
//using Map = Solocaster.Entities.Map;
//using Random = System.Random;

namespace Solocaster.Components;

public class MiniMapRenderer : Component, IRenderable
{
    private readonly Map _map;
    private readonly GameObject _player;

    private TransformComponent _playerTransform;

    private int _cellWidth;
    private int _cellHeight;
    private Texture2D _texture;
    private Vector2 _cellCenter;

    private static readonly Color _wallsColor = Color.DarkSlateGray;
    private static readonly Color _doorsColor = Color.Brown;
    private static readonly Color _emptyColor = Color.DarkGray;

    public MiniMapRenderer(
        GameObject owner,
        Map map,
        GameObject player) : base(owner)
    {
        _map = map;
        _player = player;
    }

    protected override void InitCore()
    {
        var graphicsDevice = GraphicsDeviceManagerAccessor.Instance.GraphicsDeviceManager.GraphicsDevice;
        float zoom = 0.25f;
        int minimapWidth = (int)(graphicsDevice.Viewport.Width * zoom);
        int minimapHeight = (int)(graphicsDevice.Viewport.Height * zoom);

        _cellWidth = minimapWidth / _map.Cols;
        _cellHeight = minimapHeight / _map.Rows;

        _cellCenter = new Vector2(_cellWidth, _cellHeight) * .25f;

        _texture = new Texture2D(graphicsDevice, 1, 1);
        _texture.SetData([Color.White]);
        
        _playerTransform = _player.Components.Get<TransformComponent>();

        base.InitCore();
    }

    public void Render(SpriteBatch spriteBatch)
    {
        for (int row = 0; row != _map.Rows; row++)
            for (int col = 0; col != _map.Cols; col++)
            {
                var color = _emptyColor;
                var isOpenDoor = (_map.GetDoor(col, row) is Door door && !door.IsBlocking);

                if (!isOpenDoor)
                {
                    var cell = _map.Cells[row][col];
                    color = cell switch
                    {
                        TileTypes.Floor or TileTypes.StartingPosition => _emptyColor,
                        TileTypes.DoorVertical or TileTypes.DoorHorizontal => _doorsColor,
                        _ => _wallsColor,
                    };
                }

                var dest = new Rectangle(
                    col * _cellWidth,
                    row * _cellHeight,
                    _cellWidth,
                    _cellHeight);
                spriteBatch.Draw(_texture, dest, color);
            }

        DrawCameraArrow(spriteBatch);
    }

    private void DrawCameraArrow(SpriteBatch spriteBatch)
    {
        var cameraPos = new Vector2(_playerTransform.World.Position.X * _cellWidth, _playerTransform.World.Position.Y * _cellHeight);
        var center = cameraPos + _cellCenter;

        float arrowSize = _cellWidth * 0.75f; 
        var arrowTip = center + new Vector2(_playerTransform.World.Direction.X, _playerTransform.World.Direction.Y) * arrowSize;
        var perpDir = new Vector2(-_playerTransform.World.Direction.Y, _playerTransform.World.Direction.X);

        var arrowLeft = center - new Vector2(_playerTransform.World.Direction.X, _playerTransform.World.Direction.Y) * (arrowSize * 0.5f) + perpDir * (arrowSize * 0.5f);
        var arrowRight = center - new Vector2(_playerTransform.World.Direction.X, _playerTransform.World.Direction.Y) * (arrowSize * 0.5f) - perpDir * (arrowSize * 0.5f);

        int steps = 10;
        for (int i = 0; i <= steps; i++)
        {
            float t = i / (float)steps;
            var leftPoint = Vector2.Lerp(arrowTip, arrowLeft, t);
            var rightPoint = Vector2.Lerp(arrowTip, arrowRight, t);
            DrawLine(spriteBatch, leftPoint, rightPoint, Color.Black, 1f);
        }

        DrawLine(spriteBatch, arrowTip, arrowLeft, Color.Black, 2f);
        DrawLine(spriteBatch, arrowTip, arrowRight, Color.Black, 2f);
        DrawLine(spriteBatch, arrowLeft, arrowRight, Color.Black, 2f);
    }

    private void DrawLine(SpriteBatch spriteBatch, Vector2 start, Vector2 end, Color color, float thickness)
    {
        var distance = Vector2.Distance(start, end);
        var angle = (float)Math.Atan2(end.Y - start.Y, end.X - start.X);
        var origin = new Vector2(0, 0.5f);
        var scale = new Vector2(distance, thickness);

        spriteBatch.Draw(
            _texture,
            start,
            sourceRectangle: null,
            color: color,
            rotation: angle,
            origin: origin,
            scale: scale,
            effects: SpriteEffects.None,
            layerDepth: 0);
    }


    public int LayerIndex { get; set; }
    public bool Hidden { get; set; }
}
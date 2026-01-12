using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoRaycaster;
using Solo;
using Solo.Components;
using Solo.Services;
using Solocaster.Entities;
using System;
using System.Collections.Generic;

namespace Solocaster.Components;

public class MiniMapRenderer : Component, IRenderable
{
    private readonly Map _map;
    private readonly Camera _camera;

    private int _cellWidth;
    private int _cellHeight;
    private Texture2D _texture;
    private Vector2 _cellCenter;

    public readonly Color[] CellColors;

    public MiniMapRenderer(
        GameObject owner,
        Map map,
        Camera camera) : base(owner)
    {
        _map = map;
        _camera = camera;       

        var cellTypes = new HashSet<int>();
        for (int row = 0; row != _map.Rows; row++)
            for (int col = 0; col != _map.Cols; col++)
            {
                var cell = _map.Cells[row][col];
                cellTypes.Add(cell);
            }

        var colorsCount = cellTypes.Count;

        CellColors = new Color[colorsCount];
        CellColors[0] = Color.DarkSlateGray;
        for (int c = 1; c != colorsCount; c++)
        {
            CellColors[c] = new Color(
                (byte)Random.Shared.Next(100, 220),
                (byte)Random.Shared.Next(100, 220),
                (byte)Random.Shared.Next(100, 220),
                (byte)255);
        }
    }

    protected override void InitCore()
    {
        var renderService = GameServicesManager.Instance.GetRequired<RenderService>();
        var graphicsDevice = renderService.Graphics.GraphicsDevice;
        float zoom = 0.25f;
        int minimapWidth = (int)(graphicsDevice.Viewport.Width * zoom);
        int minimapHeight = (int)(graphicsDevice.Viewport.Height * zoom);

        _cellWidth = minimapWidth / _map.Cols;
        _cellHeight = minimapHeight / _map.Rows;

        _cellCenter = new Vector2(_cellWidth, _cellHeight) * .25f;

        _texture = new Texture2D(graphicsDevice, 1, 1);
        _texture.SetData([Color.White]);

        base.InitCore();
    }

    public void Render(SpriteBatch spriteBatch)
    {
        for (int row = 0; row != _map.Rows; row++)
            for (int col = 0; col != _map.Cols; col++)
            {
                var cell = _map.Cells[row][col];
                if (cell == TileTypes.Floor) continue;

                var color = cell switch
                {
                    TileTypes.Door => Color.Brown,
                    _ => CellColors[cell],
                };

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
        var cameraPos = new Vector2(_camera.Position.X * _cellWidth, _camera.Position.Y * _cellHeight);
        var center = cameraPos + _cellCenter;

        float arrowSize = _cellWidth * 0.75f; 
        var arrowTip = center + new Vector2(_camera.Direction.X, _camera.Direction.Y) * arrowSize;
        var perpDir = new Vector2(-_camera.Direction.Y, _camera.Direction.X);

        var arrowLeft = center - new Vector2(_camera.Direction.X, _camera.Direction.Y) * (arrowSize * 0.5f) + perpDir * (arrowSize * 0.5f);
        var arrowRight = center - new Vector2(_camera.Direction.X, _camera.Direction.Y) * (arrowSize * 0.5f) - perpDir * (arrowSize * 0.5f);

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
using System;
using Microsoft.Xna.Framework;
using Solo;
using Solo.Services;
using Tetris.Components;

namespace Tetris.Scenes;

public class PlayScene : Scene
{
    public PlayScene(Game game) : base(game)
    {
    }

    protected override void EnterCore()
    {
        var renderService = GameServicesManager.Instance.GetService<RenderService>();

        var board = new Board(10, 20);
        var boardObject = new GameObject();
        var boardRenderer = boardObject.Components.Add<BoardRenderer>();
        boardRenderer.Board = board;
        boardRenderer.LayerIndex = (int)RenderLayers.Background;

        var onWindowResize = new Action(() =>
        {
            var w = (float)renderService.Graphics.GraphicsDevice.Viewport.Width;
            var h = (float)renderService.Graphics.GraphicsDevice.Viewport.Height;

            boardRenderer.TileSize = new Vector2(
                w * 0.4f / board.Width,
                h * 0.95f / board.Height
            );

            boardRenderer.Position = new Vector2(
                0.5f * (w - boardRenderer.BoardSize.X),
                0.5f * (h - boardRenderer.BoardSize.Y)
            );
        });
        onWindowResize();
        renderService.Window.ClientSizeChanged += (s, e) => onWindowResize();
        this.Root.AddChild(boardObject);
    }
}

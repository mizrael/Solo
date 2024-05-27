using System;
using Microsoft.Xna.Framework;
using Solo;
using Solo.Components;
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
        var board = new Board(10, 20);

        AddBoard(board);

        AddPieceController(board);
    }

    private void AddPieceController(Board board)
    {
        var controller = new GameObject();
        var brain = controller.Components.Add<PieceController>();
        brain.Board = board;
        brain.Generator = new PieceGenerator();
        this.Root.AddChild(controller);
    }

    private void AddBoard(Board board)
    {
        var renderService = GameServicesManager.Instance.GetService<RenderService>();

        var boardObject = new GameObject();
        var brain = boardObject.Components.Add<LambdaComponent>();
        brain.OnUpdate = (obj, dt) =>
        {
            board.UpdateRows();
        };

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

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
        var gameState = new GameState();
        var board = new Board(10, 20);

        AddBoard(board, gameState);

        AddPieceController(board);
        AddUI(gameState);
    }

    private void AddUI(GameState gameState)
    {
        var ui = new GameObject();
        var uiComponent = ui.Components.Add<GameUIComponent>();
        uiComponent.GameState = gameState;
        uiComponent.LayerIndex = (int)RenderLayers.UI;
        uiComponent.Font = Game.Content.Load<SpriteFont>("Fonts/GameFont");
        this.Root.AddChild(ui);
    }

    private void AddPieceController(Board board)
    {
        var controller = new GameObject();
        var brain = controller.Components.Add<PieceController>();
        brain.Board = board;
        brain.Generator = new PieceGenerator();
        this.Root.AddChild(controller);
    }

    private void AddBoard(Board board, GameState gameState)
    {
        var renderService = GameServicesManager.Instance.GetService<RenderService>();

        var boardObject = new GameObject();
        var brain = boardObject.Components.Add<LambdaComponent>();
        brain.OnUpdate = (obj, dt) =>
        {
            if (board.UpdateRows())
                gameState.IncreaseScore();
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

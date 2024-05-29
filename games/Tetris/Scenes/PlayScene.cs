using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
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
        var pieceGenerator = new PieceGenerator();
        var gameState = new GameState();
        var board = new Board(10, 20);

        var uiObj = AddUI(gameState, pieceGenerator);

        AddBoard(board, gameState, uiObj);

        AddPieceController(board, pieceGenerator);

        var mainTheme = Game.Content.Load<Song>("Audio/Tetris");
        MediaPlayer.Play(mainTheme);
        MediaPlayer.IsRepeating = true;
    }

    private GameObject AddUI(GameState gameState, PieceGenerator pieceGenerator)
    {
        var uiObj = new GameObject();
        var uiComponent = uiObj.Components.Add<GameUIComponent>();
        uiComponent.GameState = gameState;
        uiComponent.LayerIndex = (int)RenderLayers.UI;
        uiComponent.Font = Game.Content.Load<SpriteFont>("Fonts/GameFont");
        uiComponent.PieceGenerator = pieceGenerator;
        this.Root.AddChild(uiObj);

        return uiObj;
    }

    private void AddPieceController(Board board, PieceGenerator pieceGenerator)
    {
        var controller = new GameObject();
        var brain = controller.Components.Add<PieceController>();
        brain.Board = board;
        brain.Generator = pieceGenerator;
        this.Root.AddChild(controller);
    }

    private void AddBoard(Board board, GameState gameState, GameObject uiObj)
    {
        var renderService = GameServicesManager.Instance.GetService<RenderService>();

        var boardObject = new GameObject();
        var brain = boardObject.Components.Add<LambdaComponent>();
        brain.OnUpdate = (obj, dt) =>
        {
            if (board.UpdateRows())
                gameState.IncreaseScore();

            // if(board.CheckGameover())
            //     GameServicesManager.Instance.GetService<SceneManager>().SetCurrentScene(SceneNames.Play);
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

            uiObj.Components.Get<GameUIComponent>().TileSize = boardRenderer.TileSize * .5f;

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

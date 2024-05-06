using System;
using Microsoft.Xna.Framework;
using Snake.Components;
using Solo;
using Solo.Components;
using Solo.Services;

namespace Snake.Scenes;

public class PlayScene : Scene
{
    private RenderService _renderService;

    public PlayScene(Game game) : base(game)
    {
    }

    protected override void EnterCore()
    {
        _renderService = GameServicesManager.Instance.GetService<RenderService>();

        var snake = new Snake();
        var snakeObject = new GameObject();
        var snakeBrain = snakeObject.Components.Add<SnakeBrain>();
        snakeBrain.Snake = snake;
        var snakeRenderer = snakeObject.Components.Add<SnakeRenderer>();
        snakeRenderer.Snake = snake;
        snakeRenderer.LayerIndex = (int)RenderLayers.Player;

        this.Root.AddChild(snakeObject);

        var board = new Board(64, 64);
        var boardObject = new GameObject();
        var boardRenderer = boardObject.Components.Add<BoardRenderer>();
        boardRenderer.Board = board;
        boardRenderer.LayerIndex = (int)RenderLayers.Background;

        var setTileSize = new Action(() =>
        {
            snakeRenderer.TileSize = 
            boardRenderer.TileSize = new Vector2(
                (float)_renderService.Graphics.PreferredBackBufferWidth / board.Width,
                (float)_renderService.Graphics.PreferredBackBufferHeight / board.Height
            );
        });
        setTileSize();
        _renderService.Graphics.DeviceReset += (s, e) => setTileSize();

        this.Root.AddChild(boardObject);

        snake.Head.Tile = board.GetRandomEmptyTile();
    }
}

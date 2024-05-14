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
        
        var setTileSize = new Action(() =>
        {
            boardRenderer.TileSize = new Vector2(
                (float)renderService.Graphics.GraphicsDevice.Viewport.Width / board.Width,
                (float)renderService.Graphics.GraphicsDevice.Viewport.Height / board.Height
            );
        });
        setTileSize();
        renderService.Window.ClientSizeChanged += (s, e) => setTileSize();
        this.Root.AddChild(boardObject);
    }
}

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Solo;
using Solo.Components;
using Solo.Services;
using Solo.Utils;

namespace Tetris.Components;

public class GameUIComponent : Component, IRenderable
{
    private RenderService _renderService;
    private Texture2D _texture;

    private GameUIComponent(GameObject owner) : base(owner)
    {
    }

    protected override void InitCore()
    {
        _renderService = GameServicesManager.Instance.GetRequired<RenderService>();
        _texture = _renderService.CreateTexture(Constants.TileTextureSize.Width, Constants.TileTextureSize.Height, Constants.TileTextureData);
    }

    public void Render(SpriteBatch spriteBatch)
    {
        var scale = 1f;
        var pos = new Vector2(10, 10);
        var text = $"Score: {GameState.Score}";
        spriteBatch.DrawString(Font, text, pos, Color.White,
                               0f, Vector2.Zero, scale, SpriteEffects.None, 0f);

        RenderNextPiece(spriteBatch);
    }

    private void RenderNextPiece(SpriteBatch spriteBatch)
    {
        var nextPiece = PieceGenerator.Peek();
        if (nextPiece is null)
            return;

        spriteBatch.DrawString(Font, "Next:", new Vector2(10, 50), Color.White,
                               0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

        var pos = new Vector2(10, 70);
        var dest = new Rectangle(
                    (int)pos.X,
                    (int)pos.Y,
                    (int)TileSize.X, (int)TileSize.Y);
        var shape = nextPiece.CurrentShape;
        for (var y = 0; y < shape.Tiles.GetLength(1); y++)
        {
            dest.X = (int)pos.X;
            for (var x = 0; x < shape.Tiles.GetLength(0); x++)
            {
                if (shape.Tiles[x, y])
                    spriteBatch.Draw(_texture, dest, Color.Red);

                dest.X += (int)TileSize.X;
            }
            dest.Y += (int)TileSize.Y;
        }
    }

    public int LayerIndex { get; set; }
    public bool Hidden { get; set; }

    public PieceGenerator PieceGenerator;
    public GameState GameState;
    public SpriteFont Font;
    public Vector2 TileSize = new(16, 16);
}
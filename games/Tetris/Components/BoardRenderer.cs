using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Solo;
using Solo.Components;
using Solo.Services;
using Solo.Utils;

namespace Tetris.Components;

public class BoardRenderer : Component, IRenderable
{
    private Texture2D _texture;

    public BoardRenderer(GameObject owner) : base(owner)
    {
    }

    protected override void InitCore()
    {
        var renderService = GameServicesManager.Instance.GetService<RenderService>();

        _texture = renderService.CreateTexture(Constants.TileTextureWidth, Constants.TileTextureHeight, Constants.TileTextureData);

        base.InitCore();
    }

    public void Render(SpriteBatch spriteBatch)
    {
        var rect = new Rectangle(0, 0, Constants.TileTextureWidth, Constants.TileTextureHeight);

        for (var y = 0; y < Board.Height; y++)
        {
            for (var x = 0; x < Board.Width; x++)
            {
                var tile = Board.GetTileAt(x, y);
                var color = Color.White; //tile.Color ?? Color.Gray;

                var position = new Vector2(
                    this.Position.X + x * TileSize.X,
                    this.Position.Y + y * TileSize.Y);

                spriteBatch.Draw(_texture, position, rect, color, 0f, Vector2.Zero, TileSize, SpriteEffects.None, LayerIndex);
            }
        }
    }

    public int LayerIndex { get; set; }
    public bool Hidden { get; set; }
    public Vector2 TileSize = new Vector2(16, 16);
    public Vector2 Position;
    public Vector2 BoardSize => new Vector2(Board.Width * TileSize.X, Board.Height * TileSize.Y);
    public Board Board;
}

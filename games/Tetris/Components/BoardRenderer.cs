using System.Data.Common;
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

        _texture = renderService.CreateTexture(Constants.TileTextureSize.Width, Constants.TileTextureSize.Height, Constants.TileTextureData);

        base.InitCore();
    }

    public void Render(SpriteBatch spriteBatch)
    {
        var dest = new Rectangle(
                    (int)this.Position.X,
                    (int)this.Position.Y,
                    (int)TileSize.X, (int)TileSize.Y);

        for (var y = 0; y < Board.Height; y++)
        {
            dest.X = (int)Position.X;
            for (var x = 0; x < Board.Width; x++)
            {
                spriteBatch.Draw(_texture, dest, Color.Gray);

                dest.X += (int)TileSize.X;
            }
            dest.Y += (int)TileSize.Y;
        }
    }

    public int LayerIndex { get; set; }
    public bool Hidden { get; set; }
    public Vector2 TileSize = new Vector2(16, 16);
    public Vector2 Position;
    public Vector2 BoardSize => new Vector2(Board.Width * TileSize.X, Board.Height * TileSize.Y);
    public Board Board;
}

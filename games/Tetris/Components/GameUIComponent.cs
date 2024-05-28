using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Solo;
using Solo.Components;
using Solo.Services;

namespace Tetris.Components;

public class GameUIComponent : Component, IRenderable
{
    private RenderService _renderService;

    private GameUIComponent(GameObject owner) : base(owner)
    {
    }

    protected override void InitCore()
    {
        _renderService = GameServicesManager.Instance.GetService<RenderService>();
    }

    public void Render(SpriteBatch spriteBatch)
    {
        var scale = 1f;
        var pos = new Vector2(10, 10);
        var text = $"Score: {GameState.Score}";
        spriteBatch.DrawString(Font, text, pos, Color.White,
                               0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
    }

    public int LayerIndex { get; set; }
    public bool Hidden { get; set; }

    public GameState GameState;
    public SpriteFont Font;
}
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Solo;
using Solo.Components;
using Solo.Services;

namespace Solocaster;

public class DebugUIRenderer : Component, IRenderable
{
    private readonly SpriteFont _font;
    private readonly FrameCounter _frameCounter = new();

    private readonly GameObject _player;
    private TransformComponent _playerTransform;

    public DebugUIRenderer(GameObject owner, SpriteFont font, GameObject player) : base(owner)
    {
        _font = font;

        _player = player;
    }

    protected override void InitCore()
    {
        _playerTransform = _player.Components.Get<TransformComponent>();
        base.InitCore();
    }

    protected override void UpdateCore(GameTime gameTime)
    {
        _frameCounter.Update(gameTime);
    }

    public void Render(SpriteBatch spriteBatch)
    {
        var text = string.Format("FPS: {0}\nCamera {1:F2} - {2:F2}\nTile {3},{4}",
                                _frameCounter.AverageFramesPerSecond,
                                _playerTransform.World.Position.X, _playerTransform.World.Position.Y,
                                (int)_playerTransform.World.Position.X, (int)_playerTransform.World.Position.Y);
        spriteBatch.DrawString(_font, text, Vector2.Zero, Color.White,
                               0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
    }

    public int LayerIndex { get; set; }
    public bool Hidden { get; set; }

}
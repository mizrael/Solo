using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Solo.Services;

namespace Solo;

public abstract class OverlayScene : Scene
{
    private KeyboardState _previousKeyboardState;

    protected OverlayScene(Game game, Texture2D? background, Effect? backgroundEffect = null) : base(game)
    {
        Background = background;
        BackgroundEffect = backgroundEffect;
    }

    protected Texture2D? Background { get; }
    protected Effect? BackgroundEffect { get; }

    protected sealed override void InitializeCore()
    {
        Services.Add(new BackgroundRenderable(Game, Background, BackgroundEffect));
        InitializeOverlay();
    }

    protected abstract void InitializeOverlay();

    protected override void EnterCore()
    {
        _previousKeyboardState = Keyboard.GetState();
    }

    protected override void UpdateCore(GameTime gameTime)
    {
        var keyboardState = Keyboard.GetState();
        if (keyboardState.IsKeyDown(Keys.Escape) && !_previousKeyboardState.IsKeyDown(Keys.Escape))
        {
            SceneManager.Instance.PopScene();
            _previousKeyboardState = keyboardState;
            return;
        }

        _previousKeyboardState = keyboardState;
        UpdateOverlay(gameTime);
    }

    protected virtual void UpdateOverlay(GameTime gameTime)
    {
    }

    private sealed class BackgroundRenderable(Game game, Texture2D? background, Effect? effect) : IGameService, IRenderable
    {
        private static readonly Color DarkenedTint = new(0.4f, 0.4f, 0.4f, 1f);

        public int LayerIndex { get; set; }
        public bool Hidden { get; set; }

        public void Render(SpriteBatch spriteBatch)
        {
            if (background is null)
                return;

            var viewport = game.GraphicsDevice.Viewport;
            var destination = new Rectangle(0, 0, viewport.Width, viewport.Height);

            if (effect is null)
            {
                spriteBatch.Draw(background, destination, DarkenedTint);
                return;
            }

            spriteBatch.End();
            spriteBatch.Begin(effect: effect);
            spriteBatch.Draw(background, destination, Color.White);
            spriteBatch.End();
            spriteBatch.Begin();
        }
    }
}

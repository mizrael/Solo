using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoRaycaster;
using Solocaster.Entities;
using Solo;
using System.Linq;
using Solocaster.Persistence;

namespace Solocaster
{
    public class SolocasterGame : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private const int ScreenWidth = 2048;//1024; 
        private const int ScreenHeight = 1536;//768;

        private readonly static Vector2 _halfScreenSize = new(ScreenWidth / 2, ScreenHeight / 2);

        // inverted, the raycaster is rendering data rotated 90 degrees
        private const int FrameBufferWidth = ScreenHeight;
        private const int FrameBufferHeight = ScreenWidth;
        private readonly static Vector2 _halfFrameBufferSize = new(FrameBufferWidth / 2, FrameBufferHeight / 2);

        private Map _map;

        private readonly FrameCounter _frameCounter = new();
        private Camera _camera;

        private Texture2D _frameTexture;
        private Raycaster _raycaster;
        private MiniMap _miniMap;

        private SpriteFont _font;

        public SolocasterGame()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _graphics.PreferredBackBufferWidth = ScreenWidth;
            _graphics.PreferredBackBufferHeight = ScreenHeight;
            _graphics.ApplyChanges();

            _spriteBatch = new SpriteBatch(GraphicsDevice);

            _frameTexture = new Texture2D(GraphicsDevice, FrameBufferWidth, FrameBufferHeight);

            var level = LevelLoader.LoadFromJson("./data/levels/level1.json", this.Content);
            _map = level.Map;

            _camera = new(_map);

            var mainTexture = Content.Load<Texture2D>("wolftextures");
            var textures = mainTexture.Split(64, 64).Select(t => t.Rotate90(RotationDirection.CounterClockwise)).ToArray();
            _raycaster = new Raycaster(_map, FrameBufferWidth, FrameBufferHeight, textures);

            // _raycaster = new Raycaster(_map, FrameBufferWidth, FrameBufferHeight);

            _font = Content.Load<SpriteFont>("Font");

            _miniMap = new MiniMap(_map, ScreenWidth, ScreenHeight, GraphicsDevice, _camera);
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            _camera.Update(gameTime);

            _map.Update(gameTime);

            _raycaster.Update(_camera);
            _frameTexture.SetData(_raycaster.FrameBuffer);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            var deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _frameCounter.Update(deltaTime);

            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin();

            _spriteBatch.Draw(
                _frameTexture,
                position: _halfScreenSize,
                sourceRectangle: null,
                color: Color.White,
                rotation: MathHelper.PiOver2,
                origin: _halfFrameBufferSize,
                scale: 1f,
                effects: SpriteEffects.None,
                layerDepth: 0);

            _miniMap.Render(_spriteBatch);
            _spriteBatch.End();

            var text = string.Format("FPS: {0}\nCamera {1} - {2}\nTile {3},{4}",
                                    _frameCounter.AverageFramesPerSecond,
                                    _camera.Position.X, _camera.Position.Y,
                                    (int)_camera.Position.X, (int)_camera.Position.Y);
            _spriteBatch.Begin();
            _spriteBatch.DrawString(_font, text, Vector2.Zero, Color.White,
                                   0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}

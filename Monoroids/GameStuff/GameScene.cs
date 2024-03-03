using Microsoft.Xna.Framework;
using Monoroids.Core;
using Monoroids.Core.Components;
using Monoroids.Core.Loaders;
using Monoroids.Core.Services;
using Monoroids.GameStuff.Components;

namespace Monoroids.GameStuff
{
    internal class GameScene : Scene
    {
        public GameScene(Game game) : base(game)
        {
        }

        protected override void EnterCore()
        {
            var spritesheetLoader = new SpriteSheetLoader();
            var spriteSheet = spritesheetLoader.Load("meta/sheet.json", this.Game);
            var shipTexture = spriteSheet.Get("playerShip2_green");

            var player = new GameObject();
            var playerTransform = player.Components.Add<TransformComponent>();
            playerTransform.Local.Position = this.Game.GraphicsDevice.Viewport.Bounds.Center.ToVector2() - shipTexture.Center;

            player.Components.Add(new SpriteRenderComponent(player, shipTexture));
            var brain = player.Components.Add<PlayerBrain>();

            var rigidBody = player.Components.Add<MovingBody>();
            rigidBody.MaxSpeed = brain.Stats.EnginePower;

            base.Root.AddChild(player);

            base.EnterCore();
        }
    }
}

using Microsoft.Xna.Framework;
using Monoroids.Core;
using Monoroids.Core.Components;
using Monoroids.Core.Loaders;
using Monoroids.Core.Services;

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

            var ship = new GameObject();
            ship.Components.Add<TransformComponent>();
            ship.Components.Add(new SpriteRenderComponent(ship, shipTexture));

            base.Root.AddChild(ship);

            base.EnterCore();
        }
    }
}

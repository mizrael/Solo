using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monoroids.Core;
using Monoroids.Core.Components;
using Monoroids.Core.Services;

namespace Monoroids.GameStuff
{
    internal class GameScene : Scene
    {
        private Texture2D _shipTexture;

        public GameScene(Game game) : base(game)
        {
        }

        protected override void EnterCore()
        {
            var ship = new GameObject();
            ship.Components.Add<TransformComponent>();
            ship.Components.Add(new SpriteRenderComponent(ship, "sheet"));

            base.Root.AddChild(ship);

            base.EnterCore();
        }
    }
}

using Microsoft.Xna.Framework;
using Monoroids.Core;
using Monoroids.Core.Assets;
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
            
            var bulletSpawner = BuildBulletSpawner(spriteSheet);

            BuildPlayer(spriteSheet, bulletSpawner);

            base.EnterCore();
        }

        private Spawner BuildBulletSpawner(SpriteSheet spriteSheet)
        {
            var spawner = new Spawner(() =>
            {
                var bullet = new GameObject();
                
                bullet.Components.Add<TransformComponent>();

                var bulletSpriteRenderer = new SpriteRenderComponent(bullet, spriteSheet.Get("fire01"));                                
                bulletSpriteRenderer.LayerIndex = (int)RenderLayers.Items;
                bullet.Components.Add(bulletSpriteRenderer);

                var bulletBBox = bullet.Components.Add<BoundingBoxComponent>();
                bulletBBox.SetSize(bulletSpriteRenderer.Sprite.Bounds.Size);

                var speed = 7000f;

                var bulletRigidBody = bullet.Components.Add<MovingBody>();
                bulletRigidBody.MaxSpeed = speed;

                var brain = bullet.Components.Add<BulletBrain>();
                brain.Speed = speed;

                return bullet;
            }, bullet =>
            {
                bullet.Components.Get<MovingBody>().Reset();
                bullet.Components.Get<TransformComponent>().Reset();
            });

            spawner.Components.Add<TransformComponent>();

            this.Root.AddChild(spawner);

            return spawner;
        }

        private void BuildPlayer(SpriteSheet spriteSheet, Spawner bulletSpawner)
        {
            var shipTexture = spriteSheet.Get("playerShip2_green");

            var player = new GameObject();
            var playerTransform = player.Components.Add<TransformComponent>();
            playerTransform.Local.Position = this.Game.GraphicsDevice.Viewport.Bounds.Center.ToVector2() - shipTexture.Center;

            player.Components.Add(new SpriteRenderComponent(player, shipTexture));
            var brain = player.Components.Add<PlayerBrain>();

            var rigidBody = player.Components.Add<MovingBody>();
            rigidBody.MaxSpeed = brain.Stats.EnginePower;

            var weapon = player.Components.Add<Weapon>();
            weapon.Spawner = bulletSpawner;

            base.Root.AddChild(player);
        }
    }
}

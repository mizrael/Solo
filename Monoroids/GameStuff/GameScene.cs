using Microsoft.Xna.Framework;
using Monoroids.Core;
using Monoroids.Core.Assets;
using Monoroids.Core.Components;
using Monoroids.Core.Loaders;
using Monoroids.Core.Services;
using Monoroids.GameStuff.Components;
using System;

namespace Monoroids.GameStuff;

internal class GameScene : Scene
{
    private int _killedAsteroids = 0;
    private double _lastAsteroidSpawnTime = 0;
    private long _startAsteroidSpawnRate = 2000;
    private long _maxAsteroidSpawnRate = 500;
    private long _asteroidSpawnRate = 2000;
    private Spawner _asteroidsSpawner;

    public GameScene(Game game) : base(game)
    {
    }

    protected override void EnterCore()
    {
        var spritesheetLoader = new SpriteSheetLoader();
        var spriteSheet = spritesheetLoader.Load("meta/sheet.json", this.Game);

        var collisionService = GameServicesManager.Instance.GetService<CollisionService>();
        var renderService = GameServicesManager.Instance.GetService<RenderService>();

        var bulletSpawner = BuildBulletSpawner(spriteSheet, collisionService);

        var player = BuildPlayer(spriteSheet, bulletSpawner, collisionService);

        _asteroidsSpawner = BuildAsteroidsSpawner(spriteSheet, collisionService, renderService, player);

        base.EnterCore();
    }

    protected override void Update(GameTime gameTime)
    {
        _asteroidSpawnRate = Math.Max(_asteroidSpawnRate - 1, _maxAsteroidSpawnRate);

        var canSpawnAsteroid = gameTime.TotalGameTime.TotalMilliseconds - _lastAsteroidSpawnTime >= _asteroidSpawnRate;
        if (canSpawnAsteroid)
        {
            _lastAsteroidSpawnTime = gameTime.TotalGameTime.TotalMilliseconds;
            _asteroidsSpawner.Spawn();
        }
    }

    private Spawner BuildBulletSpawner(
        SpriteSheet spriteSheet,
        CollisionService collisionService)
    {
        var spawner = new Spawner(() =>
        {
            var bullet = new GameObject();
            
            bullet.Components.Add<TransformComponent>();

            var bulletSpriteRenderer = bullet.Components.Add<SpriteRenderComponent>();                                  
            bulletSpriteRenderer.LayerIndex = (int)RenderLayers.Items;
            bulletSpriteRenderer.Sprite = spriteSheet.Get("fire01");

            var bulletBBox = bullet.Components.Add<BoundingBoxComponent>();
            bulletBBox.SetSize(bulletSpriteRenderer.Sprite.Bounds.Size);
            collisionService.Add(bulletBBox);

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

    private GameObject BuildPlayer(SpriteSheet spriteSheet, Spawner bulletSpawner, CollisionService collisionService)
    {
        var shipTexture = spriteSheet.Get("playerShip2_green");

        var player = new GameObject();

        var playerTransform = player.Components.Add<TransformComponent>();
        playerTransform.Local.Position = this.Game.GraphicsDevice.Viewport.Bounds.Center.ToVector2() - shipTexture.Center;

        var renderer = player.Components.Add<SpriteRenderComponent>(); 
        renderer.Sprite = shipTexture;

        var brain = player.Components.Add<PlayerBrain>();

        var rigidBody = player.Components.Add<MovingBody>();
        rigidBody.MaxSpeed = brain.Stats.EnginePower;

        var bbox = player.Components.Add<BoundingBoxComponent>();
        bbox.SetSize(shipTexture.Bounds.Size);
        collisionService.Add(bbox);

        var weapon = player.Components.Add<Weapon>();
        weapon.Spawner = bulletSpawner;

        base.Root.AddChild(player);

        return player;
    }

    private Spawner BuildAsteroidsSpawner(
        SpriteSheet spriteSheet, 
        CollisionService collisionService,
        RenderService renderService,
        GameObject player)
    {
        var spriteNames = new[]
        {
            "meteorBrown_big1",
            "meteorBrown_big2",
            "meteorBrown_big3",
            "meteorBrown_big4",
            "meteorGrey_big1",
            "meteorGrey_big2",
            "meteorGrey_big3",
            "meteorGrey_big4",
        };
        int spriteIndex = 0;

       // var powerupsFactory = new PowerupFactory(spriteSheet, collisionService);

        var spawner = new Spawner(() =>
        {
            var asteroid = new GameObject();

            var transform = asteroid.Components.Add<TransformComponent>();

            var spriteRenderer = asteroid.Components.Add<SpriteRenderComponent>();
            var sprite = spriteSheet.Get(spriteNames[spriteIndex]);
            spriteIndex = spriteIndex + 1 % spriteNames.Length;
            spriteRenderer.Sprite = sprite;
            spriteRenderer.LayerIndex = (int)RenderLayers.Enemies;

            var bbox = asteroid.Components.Add<BoundingBoxComponent>();
            bbox.SetSize(sprite.Bounds.Size);
            collisionService.Add(bbox);

            var brain = asteroid.Components.Add<AsteroidBrain>();
            
            brain.OnDeath += o =>
            {
                _killedAsteroids++;
               // _gameStats.IncreaseScore();

                //var explosion = _explosionsSpawner.Spawn();
                //var explosionTransform = explosion.Components.Get<TransformComponent>();
                //explosionTransform.Local.Clone(transform.Local);
                //explosionTransform.World.Clone(transform.Local);

                //var canSpawnPowerup = MathUtils.Random.Next(10) < 2;
                //if (canSpawnPowerup)
                //{
                //    var powerup = powerupsFactory.Create();
                //    var powerupTransform = powerup.Components.Get<TransformComponent>();
                //    powerupTransform.Local.Clone(transform.Local);
                //    powerupTransform.Local.Rotation = 0;
                //    this.Root.AddChild(powerup);
                //}
            };

            return asteroid;
        }, asteroid =>
        {
            var transform = asteroid.Components.Get<TransformComponent>();

            transform.World.Reset();
            transform.Local.Reset();

            transform.Local.Position.X = Random.Shared.Next(2) == 0 ? 0 : renderService.Graphics.PreferredBackBufferWidth;
            transform.Local.Position.Y = Random.Shared.Next(2) == 0 ? 0 : renderService.Graphics.PreferredBackBufferHeight;

            var brain = asteroid.Components.Get<AsteroidBrain>();
            var dir = player.Components.Get<TransformComponent>().Local.Position - transform.Local.Position;
            brain.Direction = Microsoft.Xna.Framework.Vector2.Normalize(dir);
            
            //var w = (double)this.Game.Display.Size.Width;
            //var rx = MathUtils.Random.NextDouble(0, .4, .6, 1);
            //var tx = MathUtils.Normalize(rx, 0, 1, -1, 1);
            //transform.Local.Position.X = (float)(tx * w / 2.5 + w / 2);

            //var h = (double)this.Game.Display.Size.Height;
            //var ry = MathUtils.Random.NextDouble(0, .35, .65, 1);
            //var ty = MathUtils.Normalize(ry, 0, 1, -1, 1);
            //transform.Local.Position.Y = (float)(ty * h / 2.5 + h / 2);
        });

        spawner.Components.Add<TransformComponent>();

        this.Root.AddChild(spawner);

        return spawner;
    }

}

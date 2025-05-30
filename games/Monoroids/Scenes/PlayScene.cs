using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using Solo;
using Solo.Assets;
using Solo.Assets.Loaders;
using Solo.Components;
using Solo.Services;
using Monoroids.Components;
using Monoroids.Services;
using System;
using System.Linq;

namespace Monoroids.Scenes;

internal class PlayScene : Scene
{
    private double _lastAsteroidSpawnTime = 0;
    private long _maxAsteroidSpawnRate = 500;
    private long _asteroidSpawnRate = 2000;
    private Spawner _asteroidsSpawner;

    private GameStatsUIComponent _gameStats;

    public PlayScene(Game game) : base(game)
    {
    }

    protected override void EnterCore()
    {
        var spriteSheet = new SpriteSheetLoader().Load("meta/sheet.json", Game);

        var collisionService = GameServicesManager.Instance.GetRequired<CollisionService>();
        var renderService = GameServicesManager.Instance.GetRequired<RenderService>();

        var explosionSpawner = BuildExplosionsSpawner();
        var bulletSpawner = BuildBulletSpawner(spriteSheet, collisionService);

        var player = BuildPlayer(spriteSheet, bulletSpawner, collisionService);

        _asteroidsSpawner = BuildAsteroidsSpawner(spriteSheet, collisionService, renderService, player, explosionSpawner);

        BuidUI(player);
        BuildBackground(renderService);

        var mainTheme = Game.Content.Load<Song>("Sounds/main-theme");        
        MediaPlayer.Play(mainTheme);
        MediaPlayer.IsRepeating = true;

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

    private Spawner BuildExplosionsSpawner()
    {
        var explosionAnim = new AnimationLoader().Load("meta/animations/explosion1.json", Game);

        var spawner = new Spawner(() =>
        {
            var explosion = new GameObject();
            explosion.Components.Add<TransformComponent>();

            var renderer = explosion.Components.Add<AnimationRenderer>();
            renderer.Animation = explosionAnim;
            renderer.LayerIndex = (int)RenderLayers.Items;
            renderer.OnAnimationComplete += _ => explosion.Enabled = false;

            return explosion;
        }, explosion =>
        {
            var renderer = explosion.Components.Add<AnimationRenderer>();
            renderer.Reset();
        });

        spawner.Components.Add<TransformComponent>();

        Root.AddChild(spawner);

        return spawner;
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

        Root.AddChild(spawner);

        return spawner;
    }

    private GameObject BuildPlayer(SpriteSheet spriteSheet, Spawner bulletSpawner, CollisionService collisionService)
    {
        var shipTemplate = GameState.Instance.ShipTemplate;
        var shipTexture = spriteSheet.Get(shipTemplate.Asset);

        var player = new GameObject();

        var playerTransform = player.Components.Add<TransformComponent>();
        playerTransform.Local.Position = Game.GraphicsDevice.Viewport.Bounds.Center.ToVector2() - shipTexture.Center;

        var renderer = player.Components.Add<SpriteRenderComponent>();
        renderer.Sprite = shipTexture;
        renderer.LayerIndex = (int)RenderLayers.Player;

        var brain = player.Components.Add<PlayerBrain>();
        brain.Stats = shipTemplate.Stats;
        brain.OnDeath += player =>
        {
            GameServicesManager.Instance.GetRequired<SceneManager>()
                .SetCurrentScene(SceneNames.GameOver);
        };

        var rigidBody = player.Components.Add<MovingBody>();
        rigidBody.MaxSpeed = brain.Stats.EnginePower;

        var bbox = player.Components.Add<BoundingBoxComponent>();
        bbox.SetSize(shipTexture.Bounds.Size);
        collisionService.Add(bbox);

        var weapon = player.Components.Add<Weapon>();
        weapon.Spawner = bulletSpawner;
        weapon.ShotSound = Game.Content.Load<SoundEffect>("Sounds/laser");

        var shieldSprites = new[]{
            "shield3",
            "shield2",
            "shield1"
        };
        var shield = new GameObject();
        player.AddChild(shield);
        var shieldTransform = shield.Components.Add<TransformComponent>();
        var shieldRenderer = shield.Components.Add<SpriteRenderComponent>();
        shieldRenderer.Sprite = spriteSheet.Get(shieldSprites[0]);
        shieldRenderer.LayerIndex = (int)RenderLayers.Items;
        var shieldBrain = shield.Components.Add<LambdaComponent>();
        shieldBrain.OnUpdate = (_, _) =>
        {
            shieldRenderer.Hidden = brain.Stats.ShieldPower < 1;
            int index = 2 - (int)(2 * ((float)brain.Stats.ShieldPower / brain.Stats.ShieldMaxPower));
            shieldRenderer.Sprite = spriteSheet.Get(shieldSprites[index]);

            shieldTransform.Local.Rotation = playerTransform.Local.Rotation;
        };

        Root.AddChild(player);

        return player;
    }

    private Spawner BuildAsteroidsSpawner(
        SpriteSheet spriteSheet,
        CollisionService collisionService,
        RenderService renderService,
        GameObject player,
        Spawner explosionSpawner)
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

        var powerupsFactory = new PowerupFactory(spriteSheet, collisionService);
        var explosionSound = Game.Content.Load<SoundEffect>("Sounds/explosion");

        var powerUpsContainer = new GameObject();
        this.Root.AddChild(powerUpsContainer);

        var spawner = new Spawner(() =>
        {
            var asteroid = new GameObject();

            var transform = asteroid.Components.Add<TransformComponent>();

            var spriteRenderer = asteroid.Components.Add<SpriteRenderComponent>();
            var sprite = spriteSheet.Get(spriteNames[spriteIndex]);
            spriteIndex = ++spriteIndex % spriteNames.Length;
            spriteRenderer.Sprite = sprite;
            spriteRenderer.LayerIndex = (int)RenderLayers.Enemies;

            var bbox = asteroid.Components.Add<BoundingBoxComponent>();
            bbox.SetSize(sprite.Bounds.Size);
            collisionService.Add(bbox);

            var brain = asteroid.Components.Add<AsteroidBrain>();

            brain.OnDeath += (_, hasCollidedWithPlayer) =>
            {
                explosionSound.Play();

                var points = hasCollidedWithPlayer ? 10 : 25;
                _gameStats.IncreaseScore(points);

                var explosion = explosionSpawner.Spawn();
                var explosionTransform = explosion.Components.Get<TransformComponent>();
                explosionTransform.Local.Clone(transform.Local);
                explosionTransform.World.Clone(transform.Local);

                var isPlayerAlive = player.Enabled && player.Components.Get<PlayerBrain>().Stats.IsAlive;               
                var canSpawnPowerup = isPlayerAlive &&
                                      powerUpsContainer.Children.Count(c=> c.Enabled) < 3 &&
                                      Random.Shared.Next(10) < 2;                
                if (canSpawnPowerup)
                {
                    var powerup = powerupsFactory.Create();
                    var powerupTransform = powerup.Components.Get<TransformComponent>();
                    powerupTransform.Local.Clone(transform.Local);
                    powerupTransform.Local.Rotation = 0;
                    powerUpsContainer.AddChild(powerup);
                }
            };

            return asteroid;
        }, asteroid =>
        {
            var transform = asteroid.Components.Get<TransformComponent>();

            transform.World.Reset();
            transform.Local.Reset();

            transform.Local.Position.X = Random.Shared.NextBool() ? 0 : renderService.Graphics.GraphicsDevice.Viewport.Width;
            transform.Local.Position.Y = Random.Shared.NextBool() ? 0 : renderService.Graphics.GraphicsDevice.Viewport.Height;

            var brain = asteroid.Components.Get<AsteroidBrain>();
            var dir = player.Components.Get<TransformComponent>().Local.Position - transform.Local.Position;
            brain.Direction = Vector2.Normalize(dir);
        });

        spawner.Components.Add<TransformComponent>();

        Root.AddChild(spawner);

        return spawner;
    }

    private GameObject BuidUI(GameObject player)
    {
        var ui = new GameObject();
        _gameStats = ui.Components.Add<GameStatsUIComponent>();
        _gameStats.LayerIndex = (int)RenderLayers.UI;
        _gameStats.Font = Game.Content.Load<SpriteFont>("Fonts/UI");

        var playerStats = ui.Components.Add<PlayerStatsUIComponent>();
        playerStats.PlayerBrain = player.Components.Get<PlayerBrain>();
        playerStats.LayerIndex = (int)RenderLayers.UI;

        Root.AddChild(ui);

        return ui;
    }

    private GameObject BuildBackground(RenderService renderService)
    {
        var background = new GameObject();

        var sprite = Sprite.FromTexture("Backgrounds/blue", Game.Content);
        var setBackgroundSize = new Action(() =>
        {
            sprite.Bounds = new Rectangle(0, 0,
                               (int)(renderService.Graphics.GraphicsDevice.Viewport.Width * 1.5),
                                (int)(renderService.Graphics.GraphicsDevice.Viewport.Height * 1.5));
        });
        setBackgroundSize();

        renderService.Window.ClientSizeChanged += (s, e) => setBackgroundSize();

        background.Components.Add<TransformComponent>();

        var renderer = background.Components.Add<SpriteRenderComponent>();
        renderer.Sprite = sprite;
        renderer.LayerIndex = (int)RenderLayers.Background;

        Root.AddChild(background);

        return background;
    }
}

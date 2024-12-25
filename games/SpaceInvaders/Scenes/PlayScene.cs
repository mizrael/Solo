using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Solo;
using Solo.Assets;
using Solo.Assets.Loaders;
using Solo.Components;
using Solo.Services;
using Solo.Services.Messaging;
using SpaceInvaders.Logic;
using SpaceInvaders.Logic.Messages;
using System.Linq;

namespace SpaceInvaders.Scenes;

public class PlayScene : Scene
{
    public PlayScene(Game game) : base(game)
    {
    }

    protected override void EnterCore()
    {
        var spriteSheet = new SpriteSheetLoader().Load("meta/spritesheet.json", Game);        
        var collisionService = GameServicesManager.Instance.GetService<CollisionService>();

        var bulletSpawner = BuildBulletSpawner(spriteSheet, collisionService);
        this.Root.AddChild(bulletSpawner);

        var scale = 0.65f;

        AddAliens(spriteSheet, collisionService, scale);

        AddPlayer(spriteSheet, collisionService, scale, bulletSpawner);
    }

    private void AddPlayer(SpriteSheet spriteSheet, CollisionService collisionService, float scale, Spawner bulletSpawner)
    {
        var player = new GameObject();
        player.AddTag(Tags.Player);

        var renderer = player.Components.Add<SpriteRenderComponent>();
        renderer.Sprite = spriteSheet.Get("player");
        renderer.LayerIndex = (int)RenderLayers.Player;
        
        var transform = player.Components.Add<TransformComponent>();
        transform.Local.Position.X = Game.GraphicsDevice.Viewport.Width * .5f;
        transform.Local.Position.Y = Game.GraphicsDevice.Viewport.Height - renderer.Sprite.Bounds.Height * .5f;
        transform.Local.Scale = new Vector2(scale, scale);

        player.Components.Add<PlayerBrain>();

        var weapon = player.Components.Add<Weapon>();
        weapon.Spawner = bulletSpawner;

        Root.AddChild(player);
    }

    private Spawner BuildBulletSpawner(SpriteSheet spriteSheet, CollisionService collisionService)
    {
        var spawner = new Spawner(() =>
        {
            var bullet = new GameObject();
            bullet.AddTag(Tags.Bullet);

            bullet.Components.Add<TransformComponent>();

            var bulletSpriteRenderer = bullet.Components.Add<SpriteRenderComponent>();
            bulletSpriteRenderer.Sprite = spriteSheet.Get("missile");
            bulletSpriteRenderer.LayerIndex = (int)RenderLayers.Items;

            var bulletBBox = bullet.Components.Add<BoundingBoxComponent>();
            bulletBBox.SetSize(bulletSpriteRenderer.Sprite.Bounds.Size);

            var speed = 7000f;

            var bulletRigidBody = bullet.Components.Add<MovingBody>();
            bulletRigidBody.MaxSpeed = speed;

            var brain = bullet.Components.Add<BulletBrain>();
            brain.Speed = speed;

            collisionService.Add(bulletBBox);

            return bullet;
        }, bullet =>
        {
            bullet.Components.Get<MovingBody>().Reset();

            bullet.Components.Get<TransformComponent>().Local.Reset();
            bullet.Components.Get<TransformComponent>().World.Reset();
        });

        spawner.Components.Add<TransformComponent>();

        return spawner;
    }

    private void AddAliens(
        SpriteSheet spriteSheet, 
        CollisionService collisionService, 
        float scale)
    {
        var alienWidth = 110;
        var alienHeight = 120;
        var rows = 5;
        var cols = 8;
        var offsetX = 20;
        var offsetY = alienHeight * scale * .05f;

        var yStep = alienHeight * scale * .05f;
        var boardSize = new Vector2(
            alienWidth * scale,
            Game.GraphicsDevice.Viewport.Width - alienWidth * scale
        );
        var startX = (Game.GraphicsDevice.Viewport.Width - (cols * alienWidth * scale + cols * offsetX * .5f)) * .5f;
        var startY = 50;

        var bus = GameServicesManager.Instance.GetService<MessageBus>();

        var setDirectionTopic = bus.GetTopic<SetDirection>();

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                var alienName = $"alien{i + 1}";
                var pos = new Vector2(startX + j * alienWidth * scale + offsetX * j, startY + i * alienHeight * scale + offsetY * i);
                var alien = AddAlien(spriteSheet, alienName, collisionService, setDirectionTopic, yStep, boardSize, pos, scale);
            }
        }
    }

    private GameObject AddAlien(
        SpriteSheet spriteSheet,
        string alienName,
        CollisionService collisionService,
        MessageTopic<SetDirection> setDirectionTopic,
        float yStep,
        Vector2 boardSize,
        Vector2 position, 
        float scale,
        float speed = 0.1f,
        int framesCount = 2,
        int fps = 2)
    {
        var alien = new GameObject();
        alien.AddTag(Tags.Enemy);
        
        var transform = alien.Components.Add<TransformComponent>();
        transform.Local.Position = position;
        transform.Local.Scale = new Vector2(scale, scale);

        var alienDirX = 1;
        var alienPosY = position.Y;
        setDirectionTopic.Subscribe(alien, (owner, msg) =>
        {
            alienDirX = msg.NewDirection;
            alienPosY += yStep;
        });

        var frames = Enumerable.Range(1, framesCount)
            .Select(i =>
            {
                var spriteName = $"{alienName}_{i}";
                var sprite = spriteSheet.Get(spriteName);
                return new AnimatedSpriteSheet.Frame(sprite.Bounds);
            })
            .ToArray();

        var spriteSheetTexture = Game.Content.Load<Texture2D>(spriteSheet.ImagePath);
        var animation = new AnimatedSpriteSheet(alienName, spriteSheetTexture, fps, frames);

        var renderer = alien.Components.Add<AnimatedSpriteSheetRenderer>();
        renderer.Animation = animation;
        renderer.LayerIndex = (int)RenderLayers.Enemies;

        var bboxSize = new Point(
            (int)(animation.Frames[0].Bounds.Size.X * scale),
            (int)(animation.Frames[0].Bounds.Size.Y * scale));
        var bbox = alien.Components.Add<BoundingBoxComponent>();
        bbox.SetSize(bboxSize);
        bbox.OnCollision += (sender, collidedWith) =>
        {
            if (collidedWith.Owner.HasTag(Tags.Enemy))
                return;
            alien.Enabled = false;
        };
        collisionService.Add(bbox);

        var brain = alien.Components.Add<LambdaComponent>();
        brain.OnUpdate = (owner, gameTime) =>
        {
            var transform = owner.Components.Get<TransformComponent>();

            if (transform.Local.Position.X > boardSize.Y)
                setDirectionTopic.Publish(new SetDirection(-1));
            else if (transform.Local.Position.X < boardSize.X)
                setDirectionTopic.Publish(new SetDirection(1));

            transform.Local.Position.X += alienDirX * gameTime.ElapsedGameTime.Milliseconds * speed;
            transform.Local.Position.Y = alienPosY;
        };

        this.Root.AddChild(alien);

        return alien;
    }
}

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

        var alienWidth = 110;
        var alienHeight = 120;
        var scale = 0.65f;

        AddAliens(spriteSheet, alienWidth, alienHeight, scale);

        AddPlayer(spriteSheet, alienHeight, scale);
    }

    private void AddPlayer(SpriteSheet spriteSheet, int alienHeight, float scale)
    {
        var player = new GameObject();
        var transform = player.Components.Add<TransformComponent>();
        transform.Local.Position.X = Game.GraphicsDevice.Viewport.Width * .5f;
        transform.Local.Position.Y = Game.GraphicsDevice.Viewport.Height - alienHeight * .5f;
        transform.Local.Scale = new Vector2(scale, scale);

        var renderer = player.Components.Add<SpriteRenderComponent>();
        renderer.Sprite = spriteSheet.Get("player");
        renderer.LayerIndex = (int)RenderLayers.Player;

        player.Components.Add<PlayerBrain>();

        Root.AddChild(player);
    }

    private void AddAliens(SpriteSheet spriteSheet, int alienWidth, int alienHeight, float scale)
    {
        var rows = 5;
        var cols = 8;
        var offsetX = 20;
        var offsetY = alienHeight * scale * .05f;
        var speed = 0.1f;

        var yStep = alienHeight * scale * .05f;
        var maxBoundWidth = Game.GraphicsDevice.Viewport.Width - alienWidth * scale;

        var bus = GameServicesManager.Instance.GetService<MessageBus>();

        var setDirectionTopic = bus.GetTopic<SetDirection>();

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                var pos = new Vector2(j * alienWidth * scale + offsetX * j, i * alienHeight * scale + offsetY * i);
                var alien = AddAlien(spriteSheet, i + 1, pos, scale);

                var alienDirX = 1;
                var alienPosY = pos.Y;
                var transform = alien.Components.Get<TransformComponent>();
                setDirectionTopic.Subscribe(alien, (owner, msg) =>
                {
                    alienDirX = msg.NewDirection;
                    alienPosY += yStep;
                });

                alien.Components.Get<LambdaComponent>().OnUpdate = (owner, gameTime) =>
                {
                    var transform = owner.Components.Get<TransformComponent>();

                    if (transform.Local.Position.X > maxBoundWidth)
                        setDirectionTopic.Publish(new SetDirection(-1));
                    else if (transform.Local.Position.X < 0)
                        setDirectionTopic.Publish(new SetDirection(1));

                    transform.Local.Position.X += alienDirX * gameTime.ElapsedGameTime.Milliseconds * speed;
                    transform.Local.Position.Y = alienPosY;
                };
            }
        }
    }

    private GameObject AddAlien(SpriteSheet spriteSheet, 
        int alienIndex, 
        Vector2 position, 
        float scale,
        int framesCount = 2,
        int fps = 2)
    {
        var alien = new GameObject();
        var transform = alien.Components.Add<TransformComponent>();
        transform.Local.Position = position;
        transform.Local.Scale = new Vector2(scale, scale);

        alien.Components.Add<LambdaComponent>();

        var alienName = $"alien{alienIndex}";

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

        this.Root.AddChild(alien);

        return alien;
    }
}

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Solo;
using Solo.Assets;
using Solo.Assets.Loaders;
using Solo.Components;
using Solo.Services;
using SpaceInvaders.Logic;
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

        var rows = 5;
        var cols = 8;
        var alienWidth = 110;
        var alienHeight = 120;
        var scale = 0.65f;
        var offsetX = 20;
        var offsetY = 10;
        for (int i=0; i<rows; i++) 
        {
            for (int j = 0; j < cols; j++)
            {
                var pos = new Vector2(j * alienWidth * scale + offsetX * j, i * alienHeight * scale + offsetY*i);
                AddAlien(spriteSheet, i+1, pos, scale);
            }
        }
    }

    private void AddAlien(SpriteSheet spriteSheet, int alienIndex, Vector2 position, float scale, int framesCount = 2)
    {
        var alien = new GameObject();
        var transform = alien.Components.Add<TransformComponent>();
        transform.Local.Position = position;
        transform.Local.Scale = new Vector2(scale, scale);

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
        var animation = new AnimatedSpriteSheet(alienName, spriteSheetTexture, 2, frames);

        var renderer = alien.Components.Add<AnimatedSpriteSheetRenderer>();
        renderer.Animation = animation;
        renderer.LayerIndex = (int)RenderLayers.Enemies;

        this.Root.AddChild(alien);
    }
}
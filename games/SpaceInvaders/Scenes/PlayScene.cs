using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Solo;
using Solo.Assets;
using Solo.Assets.Loaders;
using Solo.Components;
using Solo.Services;
using SpaceInvaders.Logic;

namespace SpaceInvaders.Scenes;

public class PlayScene : Scene
{
    public PlayScene(Game game) : base(game)
    {
    }

    protected override void EnterCore()
    {
        var spriteSheet = new SpriteSheetLoader().Load("meta/spritesheet.json", Game);

        var alien = new GameObject();
        alien.Components.Add<TransformComponent>();

        var spriteSheetTexture = Game.Content.Load<Texture2D>(spriteSheet.ImagePath);
        var animation = new Animation(spriteSheetTexture, "explosion", 10, 2, new Point(110, 120));
        var renderer = alien.Components.Add<AnimationRenderComponent>();
        renderer.Animation = animation;
        renderer.LayerIndex = (int)RenderLayers.Enemies;
         
        this.Root.AddChild(alien);
    }
}

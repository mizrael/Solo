using Solo;
using Solo.Assets;
using Solo.Components;
using Solo.Services;
using Solo.Utils;
using Monoroids.Components;
using System;

namespace Monoroids.Services;

public class PowerupFactory
{
    private readonly SpriteSheet _spriteSheet;
    private readonly BoundingBoxCollisionService _collisionService;

    public PowerupFactory(SpriteSheet spriteSheet, BoundingBoxCollisionService collisionService)
    {
        _spriteSheet = spriteSheet ?? throw new ArgumentNullException(nameof(spriteSheet));
        _collisionService = collisionService ?? throw new ArgumentNullException(nameof(collisionService));
    }

    public GameObject Create()
    {
        return Random.Shared.NextBool() ? CreateHealth() : CreateShield();
    }

    private GameObject CreateHealth()
        => CreateBase("powerupGreen_bolt", playerBrain =>
        {
            playerBrain.Stats.Health = playerBrain.Stats.MaxHealth;
        });

    private GameObject CreateShield()
        => CreateBase("powerupBlue_shield", playerBrain =>
        {
            playerBrain.Stats.ShieldPower = playerBrain.Stats.ShieldMaxPower;
        });

    private GameObject CreateBase(string spriteName, Action<PlayerBrain> onPlayerCollision)
    {
        var powerup = new GameObject();
        var transform = powerup.Components.Add<TransformComponent>();

        var sprite = _spriteSheet.Get(spriteName);
        var spriteRenderer = powerup.Components.Add<SpriteRenderComponent>();
        spriteRenderer.Sprite = sprite;
        spriteRenderer.LayerIndex = (int)RenderLayers.Items;

        var bbox = powerup.Components.Add<BoundingBoxComponent>();
        bbox.SetSize(sprite.Bounds.Size);
        _collisionService.Add(bbox);

        bbox.OnCollision += (with) =>
        {
            if (!with.Owner.Components.TryGet<PlayerBrain>(out var playerBrain))
                return;

            powerup.Enabled = false;
            powerup.Parent?.RemoveChild(powerup);

            onPlayerCollision(playerBrain);
        };

        var offset = (float)Random.Shared.NextFloat(-5, 5);

        var lambdaComp = powerup.Components.Add<LambdaComponent>();
        lambdaComp.OnUpdate = (_, gameTime) =>
        {
            float dt = (float)gameTime.TotalGameTime.TotalMilliseconds * 0.004f + offset;
            transform.Local.Position.Y += MathF.Sin(dt);
            transform.Local.Position.X += MathF.Cos(dt);
        };

        return powerup;
    }


}
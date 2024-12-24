using Microsoft.Xna.Framework;
using Solo;
using Solo.Components;

namespace SpaceInvaders.Logic;

public class Weapon : Component
{
    private double _lastBulletFiredTime = 0;
    private TransformComponent _ownerTransform;

    public Weapon(GameObject owner) : base(owner)
    {
    }

    protected override void InitCore()
    {
        _ownerTransform = Owner.Components.Get<TransformComponent>();
    }

    public void Shoot(GameTime gameTime)
    {
        var canShoot = gameTime.TotalGameTime.TotalMilliseconds - _lastBulletFiredTime >= FireRate;
        if (!canShoot)
            return;

        _lastBulletFiredTime = gameTime.TotalGameTime.TotalMilliseconds;

        var bullet = Spawner.Spawn();
        var bulletTransform = bullet.Components.Get<TransformComponent>();

        bulletTransform.Local.Rotation = _ownerTransform.Local.Rotation;
        bulletTransform.Local.Position = GetBulletStartPosition();
    }

    private Vector2 GetBulletStartPosition() => _ownerTransform.World.Position +
                                                _ownerTransform.Local.GetDirection() * Offset;

    public Spawner Spawner;

    public float Offset = -50f;
    private long FireRate = 500;
}
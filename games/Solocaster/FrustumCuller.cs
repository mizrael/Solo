using Microsoft.Xna.Framework;
using MonoRaycaster;
using Solo;
using Solo.Components;

namespace Solocaster;

public static class FrustumCuller
{
    public static bool IsVisible(GameObject entity, Camera camera)
    {
        var transform = entity.Components.Get<TransformComponent>();
        if (transform == null) return false;

        var worldPos = transform.Local.Position;

        var toEntity = worldPos - camera.Position;

        var dotDir = Vector2.Dot(toEntity, camera.Direction);
        if (dotDir < 0) return false;

        if (toEntity.LengthSquared() > 50 * 50) return false;

        return true;
    }
}

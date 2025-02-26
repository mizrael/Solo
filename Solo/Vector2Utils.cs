using Microsoft.Xna.Framework;
using System.Runtime.CompilerServices;

namespace Solo;

/// <summary>
/// https://github.com/Unity-Technologies/UnityCsReference/blob/b42ec0031fc505c35aff00b6a36c25e67d81e59e/Runtime/Export/Math/Vector2.cs#L15
/// </summary>
static class MethodImplOptionsEx
{
    public const short AggressiveInlining = 256;
}

public static class Vector2Utils
{
    /// <summary>
    /// https://github.com/Unity-Technologies/UnityCsReference/blob/master/Runtime/Export/Math/Vector2.cs#L223
    /// </summary>
    /// <param name="vector"></param>
    /// <param name="maxLength"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptionsEx.AggressiveInlining)]
    public static Vector2 ClampMagnitude(ref Vector2 vector, float maxLength)
    {
        float sqrMagnitude = vector.LengthSquared();
        if (sqrMagnitude > maxLength * maxLength)
        {
            float mag = (float)Math.Sqrt(sqrMagnitude);

            //these intermediate variables force the intermediate result to be
            //of float precision. without this, the intermediate result can be of higher
            //precision, which changes behavior.
            float normalized_x = vector.X / mag;
            float normalized_y = vector.Y / mag;
            return new Vector2(normalized_x * maxLength,
                normalized_y * maxLength);
        }
        return vector;
    }
}

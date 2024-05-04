using System;

namespace Monoroids.Core;

public static class RandomExtensions
{
    public static bool NextBool(this Random random)
    {
        return random.Next(0, 2) == 0;
    }

    public static float NextFloat(this Random random, float min, float max)
    {
        return (float)random.NextDouble() * (max - min) + min;
    }
}
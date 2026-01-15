using System;
using System.Collections.Generic;
using System.Linq;

namespace Solocaster.Persistence;

public static class DictionaryExtensions
{
    public static T WeightedRandom<T>(this Dictionary<T, int> weightedItems) where T : notnull
    {
        int totalWeight = weightedItems.Values.Sum();
        int randomValue = Random.Shared.Next(totalWeight);

        int cumulative = 0;
        foreach (var kvp in weightedItems)
        {
            cumulative += kvp.Value;
            if (randomValue < cumulative)
                return kvp.Key;
        }

        return weightedItems.Keys.First();
    }
}
using System.Collections.Generic;
using Solo.Assets;
using Solocaster.Monsters;

namespace Solocaster.Animations;

public class DirectionalAnimation
{
    private readonly Dictionary<Direction, AnimatedSpriteSheet> _directions = new();

    public void Add(Direction direction, AnimatedSpriteSheet animation)
    {
        _directions[direction] = animation;
    }

    public AnimatedSpriteSheet Get(Direction direction)
    {
        if (_directions.TryGetValue(direction, out var animation))
            return animation;

        return _directions.TryGetValue(Direction.Front, out var fallback)
            ? fallback
            : throw new KeyNotFoundException($"No animation for direction {direction} and no Front fallback");
    }

    public bool HasDirection(Direction direction) => _directions.ContainsKey(direction);

    public bool HasAny => _directions.Count > 0;
}

using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Solo.Assets;
using Solocaster.Monsters;

namespace Solocaster.Components;

public class DirectionalSpriteProvider : IDirectionalFrameProvider
{
    private readonly Dictionary<Direction, Sprite> _sprites = new();
    private Direction _currentDirection = Direction.Front;
    private Sprite? _currentSprite;

    public void AddSprite(Direction direction, Sprite sprite)
    {
        _sprites[direction] = sprite;
    }

    public void SetDirection(Direction direction)
    {
        _currentDirection = direction;
        _currentSprite = this.GetCurrentSprite();
    }

    private Sprite? GetCurrentSprite()
    {
        if (_sprites.TryGetValue(_currentDirection, out var sprite))
            return sprite;

        return _sprites.TryGetValue(Direction.Front, out var fallback) ? fallback : null;
    }

    public Direction CurrentDirection => _currentDirection;

    public Sprite? Sprite => _currentSprite;

    public Rectangle Bounds => _currentSprite?.Bounds ?? Rectangle.Empty;

    public bool HasAny => _sprites.Count > 0;
}

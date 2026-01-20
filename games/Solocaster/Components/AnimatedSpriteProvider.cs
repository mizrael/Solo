using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Solo.Assets;
using Solocaster.Animations;
using Solocaster.Monsters;

namespace Solocaster.Components;

public class AnimatedSpriteProvider : IFrameProvider
{
    private readonly Dictionary<string, DirectionalAnimation> _animations = new();
    private string _currentState = "idle";
    private Direction _currentDirection = Direction.Front;
    private int _currentFrame;
    private double _frameTimer;

    public void AddAnimation(string state, DirectionalAnimation animation)
    {
        _animations[state] = animation;
    }

    public void SetState(string state)
    {
        if (_currentState == state)
            return;

        if (!_animations.ContainsKey(state))
            return;

        _currentState = state;
        _currentFrame = 0;
        _frameTimer = 0;
    }

    public void SetDirection(Direction direction)
    {
        _currentDirection = direction;
    }

    public void Update(GameTime gameTime)
    {
        var animation = GetCurrentAnimation();
        if (animation == null)
            return;

        _frameTimer += gameTime.ElapsedGameTime.TotalMilliseconds;
        var frameDuration = 1000.0 / animation.Fps;

        if (_frameTimer >= frameDuration)
        {
            _frameTimer -= frameDuration;
            _currentFrame++;

            if (_currentFrame >= animation.Frames.Length)
            {
                _currentFrame = 0;
                OnAnimationComplete?.Invoke(_currentState);
            }
        }
    }

    public Rectangle GetCurrentBounds()
    {
        var animation = GetCurrentAnimation();
        if (animation == null)
            return Rectangle.Empty;

        return animation.Frames[_currentFrame].Bounds;
    }

    public Texture2D? GetTexture()
    {
        var animation = GetCurrentAnimation();
        return animation?.Texture;
    }

    private AnimatedSpriteSheet? GetCurrentAnimation()
    {
        if (!_animations.TryGetValue(_currentState, out var directional))
            return null;

        return directional.Get(_currentDirection);
    }

    public string CurrentState => _currentState;
    public Direction CurrentDirection => _currentDirection;
    public int CurrentFrame => _currentFrame;

    public event Action<string> OnAnimationComplete;
}

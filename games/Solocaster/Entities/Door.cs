using Microsoft.Xna.Framework;
using System;

namespace Solocaster.Entities;

public class Door
{
    public int X { get; }
    public int Y { get; }
    public float OpenAmount { get; private set; } // 0 = closed, 1 = fully open
    public bool IsVertical { get; } // true for N-S door, false for E-W door
    public bool IsOpening { get; private set; }
    public int SpriteIndex { get; }

    private const float OpenSpeed = 2.0f;
    private const float OpenTreshold = 0.9f;

    public Door(int x, int y, bool isVertical, int spriteIndex = 0)
    {
        X = x;
        Y = y;
        IsVertical = isVertical;
        SpriteIndex = spriteIndex;
        OpenAmount = 0;
        IsOpening = false;
    }

    public void StartOpening()
    {
        if (OpenAmount < 1.0f)
        {
            IsOpening = true;
        }
    }

    public void Update(GameTime gameTime)
    {
        if (IsOpening && OpenAmount < 1.0f)
        {
            OpenAmount = Math.Min(1.0f, OpenAmount + (float)gameTime.ElapsedGameTime.TotalSeconds / OpenSpeed);
            if (OpenAmount >= 1.0f)
            {
                IsOpening = false;
            }
        }
    }

    public bool IsBlocking
        => OpenAmount < OpenTreshold;
}
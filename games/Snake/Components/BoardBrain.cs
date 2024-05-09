using Microsoft.Xna.Framework;
using Solo;
using Solo.Components;
using System;

namespace Snake.Components;

public class BoardBrain : Component
{
    private double _lastUpdate;
    private TimeSpan FoodSpawnInterval = TimeSpan.FromSeconds(5);

    public BoardBrain(GameObject owner) : base(owner)
    {
    }

    protected override void UpdateCore(GameTime gameTime)
    {
        base.UpdateCore(gameTime);

        if (gameTime.TotalGameTime.TotalMilliseconds - _lastUpdate < FoodSpawnInterval.TotalMilliseconds)
            return;

        _lastUpdate = gameTime.TotalGameTime.TotalMilliseconds;

        Board.SpawnFood();
    }

    public Board Board { get; set; }
}
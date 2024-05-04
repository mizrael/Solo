using System;

namespace Monoroids.GameStuff;

public class GameState
{
    private GameState(){}

    public ShipTemplate ShipTemplate;

    private static Lazy<GameState> _instance = new(new GameState());
    public static GameState Instance => _instance.Value;
}
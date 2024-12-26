namespace SpaceInvaders.Logic;

public enum RenderLayers
{
    Background = 0,
    Enemies,
    Player,
    Items,
    UI
}

public static class Tags
{
    public const string Player = "Player";
    public const string Enemy = "Enemy";
    public const string Bullet = "Bullet";
}
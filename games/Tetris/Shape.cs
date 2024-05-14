namespace Tetris;

public record Shape(bool[,] Tiles);

public enum RenderLayers
{
    Background = 0, 
    Pieces,
    UI
}
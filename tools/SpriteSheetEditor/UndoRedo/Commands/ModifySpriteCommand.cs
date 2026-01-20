using SpriteSheetEditor.Models;

namespace SpriteSheetEditor.UndoRedo.Commands;

public record struct SpriteState(string Name, int X, int Y, int Width, int Height)
{
    public static SpriteState From(SpriteDefinition sprite) =>
        new(sprite.Name, sprite.X, sprite.Y, sprite.Width, sprite.Height);

    public void ApplyTo(SpriteDefinition sprite)
    {
        sprite.Name = Name;
        sprite.X = X;
        sprite.Y = Y;
        sprite.Width = Width;
        sprite.Height = Height;
    }
}

public class ModifySpriteCommand : IUndoableCommand
{
    private readonly SpriteDefinition _sprite;
    private readonly SpriteState _oldState;
    private readonly SpriteState _newState;

    public string Description { get; }

    public ModifySpriteCommand(SpriteDefinition sprite, SpriteState oldState, SpriteState newState, string description)
    {
        _sprite = sprite;
        _oldState = oldState;
        _newState = newState;
        Description = description;
    }

    public static ModifySpriteCommand Create(SpriteDefinition sprite, SpriteState oldState, string description)
    {
        return new ModifySpriteCommand(sprite, oldState, SpriteState.From(sprite), description);
    }

    public void Execute()
    {
        _newState.ApplyTo(_sprite);
    }

    public void Undo()
    {
        _oldState.ApplyTo(_sprite);
    }
}

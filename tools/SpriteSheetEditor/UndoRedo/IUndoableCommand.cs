namespace SpriteSheetEditor.UndoRedo;

public interface IUndoableCommand
{
    string Description { get; }
    void Execute();
    void Undo();
}

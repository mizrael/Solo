namespace SpriteSheetEditor.UndoRedo;

public interface IUndoableCommand : IDisposable
{
    string Description { get; }
    void Execute();
    void Undo();
}

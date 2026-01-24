namespace SpriteSheetEditor.UndoRedo;

public class UndoRedoManager
{
    private readonly Stack<IUndoableCommand> _undoStack = new();
    private readonly Stack<IUndoableCommand> _redoStack = new();

    public event EventHandler? StateChanged;

    public bool CanUndo => _undoStack.Count > 0;
    public bool CanRedo => _redoStack.Count > 0;

    public string? UndoDescription => _undoStack.Count > 0 ? _undoStack.Peek().Description : null;
    public string? RedoDescription => _redoStack.Count > 0 ? _redoStack.Peek().Description : null;

    public void Execute(IUndoableCommand command)
    {
        command.Execute();
        _undoStack.Push(command);
        DisposeAndClear(_redoStack);
        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    public void Undo()
    {
        if (!CanUndo) return;

        var command = _undoStack.Pop();
        command.Undo();
        _redoStack.Push(command);
        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    public void Redo()
    {
        if (!CanRedo) return;

        var command = _redoStack.Pop();
        command.Execute();
        _undoStack.Push(command);
        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    public void Clear()
    {
        DisposeAndClear(_undoStack);
        DisposeAndClear(_redoStack);
        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    private static void DisposeAndClear(Stack<IUndoableCommand> stack)
    {
        while (stack.Count > 0)
        {
            var command = stack.Pop();
            command.Dispose();
        }
    }
}

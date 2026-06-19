using Komponenta1.Interfaces;

namespace Komponenta1.Services;

public sealed class CommandHistory : ICommandHistory
{
    private readonly Stack<IUndoableCommand> _undoStack = [];
    private readonly Stack<IUndoableCommand> _redoStack = [];

    public bool CanUndo => _undoStack.Count > 0;

    public bool CanRedo => _redoStack.Count > 0;

    public event EventHandler? HistoryChanged;

    public void Execute(IUndoableCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        command.Execute();
        _undoStack.Push(command);
        _redoStack.Clear();
        OnHistoryChanged();
    }

    public void Undo()
    {
        if (!CanUndo)
        {
            return;
        }

        IUndoableCommand command = _undoStack.Peek();
        command.Undo();

        _undoStack.Pop();
        _redoStack.Push(command);
        OnHistoryChanged();
    }

    public void Redo()
    {
        if (!CanRedo)
        {
            return;
        }

        IUndoableCommand command = _redoStack.Peek();
        command.Execute();

        _redoStack.Pop();
        _undoStack.Push(command);
        OnHistoryChanged();
    }

    public void Clear()
    {
        if (!CanUndo && !CanRedo)
        {
            return;
        }

        _undoStack.Clear();
        _redoStack.Clear();
        OnHistoryChanged();
    }

    private void OnHistoryChanged()
    {
        HistoryChanged?.Invoke(this, EventArgs.Empty);
    }
}

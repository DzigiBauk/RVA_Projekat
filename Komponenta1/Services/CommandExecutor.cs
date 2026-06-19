using Komponenta1.Interfaces;

namespace Komponenta1.Services;

public sealed class CommandExecutor : ICommandExecutor
{
    private readonly Stack<IApplicationCommand> _undoStack = [];
    private readonly Stack<IApplicationCommand> _redoStack = [];

    public bool CanUndo => _undoStack.Count > 0;

    public bool CanRedo => _redoStack.Count > 0;

    public event EventHandler? StateChanged;

    public void Execute(IApplicationCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        command.Execute();
        _undoStack.Push(command);
        _redoStack.Clear();
        OnStateChanged();
    }

    public void Undo()
    {
        if (!CanUndo)
        {
            return;
        }

        IApplicationCommand command = _undoStack.Peek();
        command.Undo();

        _undoStack.Pop();
        _redoStack.Push(command);
        OnStateChanged();
    }

    public void Redo()
    {
        if (!CanRedo)
        {
            return;
        }

        IApplicationCommand command = _redoStack.Peek();
        command.Execute();

        _redoStack.Pop();
        _undoStack.Push(command);
        OnStateChanged();
    }

    public void Clear()
    {
        if (!CanUndo && !CanRedo)
        {
            return;
        }

        _undoStack.Clear();
        _redoStack.Clear();
        OnStateChanged();
    }

    private void OnStateChanged()
    {
        StateChanged?.Invoke(this, EventArgs.Empty);
    }
}

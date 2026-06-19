namespace Komponenta1.Interfaces;

public interface ICommandHistory
{
    bool CanUndo { get; }

    bool CanRedo { get; }

    event EventHandler? HistoryChanged;

    void Execute(IUndoableCommand command);

    void Undo();

    void Redo();

    void Clear();
}

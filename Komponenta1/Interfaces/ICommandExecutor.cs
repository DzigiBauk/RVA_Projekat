namespace Komponenta1.Interfaces;

public interface ICommandExecutor
{
    bool CanUndo { get; }

    bool CanRedo { get; }

    event EventHandler? StateChanged;

    void Execute(IApplicationCommand command);

    void Undo();

    void Redo();

    void Clear();
}

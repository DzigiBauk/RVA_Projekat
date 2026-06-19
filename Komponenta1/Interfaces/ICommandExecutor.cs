namespace Komponenta1.Interfaces;

public interface ICommandExecutor
{
    bool CanUndo { get; }

    bool CanRedo { get; }

    void Execute(IApplicationCommand command);

    void Undo();

    void Redo();

    void Clear();
}

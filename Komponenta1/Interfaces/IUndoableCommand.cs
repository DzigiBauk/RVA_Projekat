namespace Komponenta1.Interfaces;

public interface IUndoableCommand
{
    string Description { get; }

    void Execute();

    void Undo();
}

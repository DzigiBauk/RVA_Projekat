namespace Komponenta1.Interfaces;

public interface IApplicationCommand
{
    string Description { get; }

    void Execute();

    void Undo();
}

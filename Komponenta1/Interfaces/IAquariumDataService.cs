namespace Komponenta1.Interfaces;

public interface IAquariumDataService
{
    Task LoadAsync(string filePath, CancellationToken cancellationToken = default);

    Task SaveAsync(string filePath, CancellationToken cancellationToken = default);
}

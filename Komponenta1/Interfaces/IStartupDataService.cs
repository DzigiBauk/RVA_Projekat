namespace Komponenta1.Interfaces;

public interface IStartupDataService
{
    Task InitializeAsync(
        string defaultFilePath,
        CancellationToken cancellationToken = default);
}

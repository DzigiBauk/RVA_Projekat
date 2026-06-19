namespace Komponenta1.Interfaces;

public interface ICoreWcfHostService
{
    bool IsRunning { get; }

    string Status { get; }

    event EventHandler? StatusChanged;

    Task StartAsync(CancellationToken cancellationToken = default);

    Task StopAsync(CancellationToken cancellationToken = default);
}

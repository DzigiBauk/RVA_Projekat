using Komponenta1.Models;

namespace Komponenta1.Interfaces;

public interface IReadingSimulationService
{
    bool IsRunning { get; }

    event EventHandler<ReadingGeneratedEventArgs>? ReadingGenerated;

    void Start();

    void Stop();
}

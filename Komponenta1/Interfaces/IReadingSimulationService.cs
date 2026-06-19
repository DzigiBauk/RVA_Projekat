using Komponenta1.Models;

namespace Komponenta1.Interfaces;

public interface IReadingSimulationService
{
    bool IsRunning { get; }

    event EventHandler<ReadingStateChangedEventArgs>? ReadingStateChanged;

    void Start();

    void Stop();
}

using Shared.Models;

namespace Komponenta1.Models;

public sealed class ReadingStateChangedEventArgs(
    Guid readingId,
    WaterQualityState previousState,
    WaterQualityState currentState) : EventArgs
{
    public Guid ReadingId { get; } = readingId;

    public WaterQualityState PreviousState { get; } = previousState;

    public WaterQualityState CurrentState { get; } = currentState;
}

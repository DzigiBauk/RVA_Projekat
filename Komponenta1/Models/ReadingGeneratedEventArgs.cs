using Shared.Models;

namespace Komponenta1.Models;

public sealed class ReadingGeneratedEventArgs(
    Guid readingId,
    Guid speciesId,
    DateTime measurementTime,
    WaterQualityState state) : EventArgs
{
    public Guid ReadingId { get; } = readingId;

    public Guid SpeciesId { get; } = speciesId;

    public DateTime MeasurementTime { get; } = measurementTime;

    public WaterQualityState State { get; } = state;
}

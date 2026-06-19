using Shared.Models;

namespace Komponenta1.ViewModels;

public sealed class WaterQualityReadingRow(
    WaterQualityReading reading,
    string speciesName)
{
    public WaterQualityReading Reading { get; } = reading;

    public Guid Id => Reading.Id;

    public Guid SpeciesId => Reading.SpeciesId;

    public string SpeciesName { get; } = speciesName;

    public DateTime MeasurementTime => Reading.MeasurementTime;

    public double PHLevel => Reading.PHLevel;

    public double Temperature => Reading.Temperature;

    public double OxygenLevel => Reading.OxygenLevel;

    public WaterQualityState State => Reading.State;
}

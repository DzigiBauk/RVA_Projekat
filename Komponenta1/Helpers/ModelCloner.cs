using Shared.Models;

namespace Komponenta1.Helpers;

internal static class ModelCloner
{
    public static AquaticSpecies Clone(AquaticSpecies source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return new AquaticSpecies
        {
            Id = source.Id,
            Name = source.Name,
            ScientificName = source.ScientificName,
            Habitat = source.Habitat,
            WaterType = source.WaterType,
            AverageLifespan = source.AverageLifespan
        };
    }

    public static WaterQualityReading Clone(WaterQualityReading source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return new WaterQualityReading
        {
            Id = source.Id,
            SpeciesId = source.SpeciesId,
            MeasurementTime = source.MeasurementTime,
            PHLevel = source.PHLevel,
            Temperature = source.Temperature,
            OxygenLevel = source.OxygenLevel,
            State = source.State
        };
    }

    public static List<AquaticSpecies> CloneSpecies(
        IEnumerable<AquaticSpecies> source)
    {
        ArgumentNullException.ThrowIfNull(source);
        return source.Select(Clone).ToList();
    }

    public static List<WaterQualityReading> CloneReadings(
        IEnumerable<WaterQualityReading> source)
    {
        ArgumentNullException.ThrowIfNull(source);
        return source.Select(Clone).ToList();
    }
}

using System.Globalization;
using Komponenta1.Interfaces;
using Shared.Models;

namespace Komponenta1.Services;

public sealed class WaterQualityReadingSearchService
    : IWaterQualityReadingSearchService
{
    public IReadOnlyList<WaterQualityReading> Search(
        IEnumerable<WaterQualityReading> readings,
        IEnumerable<AquaticSpecies> species,
        string? searchText)
    {
        ArgumentNullException.ThrowIfNull(readings);
        ArgumentNullException.ThrowIfNull(species);

        List<WaterQualityReading> source = readings.ToList();
        if (string.IsNullOrWhiteSpace(searchText))
        {
            return source;
        }

        Dictionary<Guid, string> speciesNames = species
            .GroupBy(item => item.Id)
            .ToDictionary(group => group.Key, group => group.First().Name);
        string term = searchText.Trim();

        return source
            .Where(reading =>
                Contains(reading.Id.ToString(), term) ||
                Contains(reading.SpeciesId.ToString(), term) ||
                Contains(
                    speciesNames.GetValueOrDefault(reading.SpeciesId),
                    term) ||
                Contains(
                    reading.MeasurementTime.ToString(
                        CultureInfo.CurrentCulture),
                    term) ||
                Contains(Number(reading.PHLevel), term) ||
                Contains(Number(reading.Temperature), term) ||
                Contains(Number(reading.OxygenLevel), term) ||
                Contains(reading.State.ToString(), term))
            .ToList();
    }

    private static string Number(double value)
    {
        return value.ToString(CultureInfo.CurrentCulture);
    }

    private static bool Contains(string? value, string term)
    {
        return value?.Contains(term, StringComparison.OrdinalIgnoreCase) == true;
    }
}

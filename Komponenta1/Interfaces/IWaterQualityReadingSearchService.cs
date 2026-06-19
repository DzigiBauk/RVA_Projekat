using Shared.Models;

namespace Komponenta1.Interfaces;

public interface IWaterQualityReadingSearchService
{
    IReadOnlyList<WaterQualityReading> Search(
        IEnumerable<WaterQualityReading> readings,
        IEnumerable<AquaticSpecies> species,
        string? searchText);
}

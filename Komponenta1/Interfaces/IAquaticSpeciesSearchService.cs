using Shared.Models;

namespace Komponenta1.Interfaces;

public interface IAquaticSpeciesSearchService
{
    IReadOnlyList<AquaticSpecies> Search(
        IEnumerable<AquaticSpecies> species,
        string? searchText);
}

using System.Globalization;
using Komponenta1.Interfaces;
using Shared.Models;

namespace Komponenta1.Services;

public sealed class AquaticSpeciesSearchService : IAquaticSpeciesSearchService
{
    public IReadOnlyList<AquaticSpecies> Search(
        IEnumerable<AquaticSpecies> species,
        string? searchText)
    {
        ArgumentNullException.ThrowIfNull(species);

        List<AquaticSpecies> source = species.ToList();
        if (string.IsNullOrWhiteSpace(searchText))
        {
            return source;
        }

        string term = searchText.Trim();

        return source
            .Where(item =>
                Contains(item.Id.ToString(), term) ||
                Contains(item.Name, term) ||
                Contains(item.ScientificName, term) ||
                Contains(item.Habitat, term) ||
                Contains(item.WaterType, term) ||
                Contains(
                    item.AverageLifespan.ToString(CultureInfo.CurrentCulture),
                    term))
            .ToList();
    }

    private static bool Contains(string? value, string term)
    {
        return value?.Contains(term, StringComparison.OrdinalIgnoreCase) == true;
    }
}

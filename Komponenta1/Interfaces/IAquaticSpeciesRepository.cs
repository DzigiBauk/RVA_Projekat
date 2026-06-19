using Shared.Models;

namespace Komponenta1.Interfaces;

public interface IAquaticSpeciesRepository
{
    IReadOnlyList<AquaticSpecies> GetAll();

    AquaticSpecies? GetById(Guid id);

    void Add(AquaticSpecies species);

    bool Update(AquaticSpecies species);

    bool Remove(Guid id);

    void ReplaceAll(IEnumerable<AquaticSpecies> species);
}

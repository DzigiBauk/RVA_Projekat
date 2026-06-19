using Komponenta1.Interfaces;
using Shared.Models;

namespace Komponenta1.Services;

public sealed class AquaticSpeciesRepository : IAquaticSpeciesRepository
{
    private readonly List<AquaticSpecies> _species = [];
    private readonly object _syncRoot = new();

    public IReadOnlyList<AquaticSpecies> GetAll()
    {
        lock (_syncRoot)
        {
            return _species.ToList();
        }
    }

    public AquaticSpecies? GetById(Guid id)
    {
        lock (_syncRoot)
        {
            return _species.FirstOrDefault(species => species.Id == id);
        }
    }

    public void Add(AquaticSpecies species)
    {
        ArgumentNullException.ThrowIfNull(species);

        lock (_syncRoot)
        {
            if (_species.Any(existing => existing.Id == species.Id))
            {
                throw new InvalidOperationException(
                    $"An aquatic species with ID '{species.Id}' already exists.");
            }

            _species.Add(species);
        }
    }

    public bool Update(AquaticSpecies species)
    {
        ArgumentNullException.ThrowIfNull(species);

        lock (_syncRoot)
        {
            int index = _species.FindIndex(existing => existing.Id == species.Id);
            if (index < 0)
            {
                return false;
            }

            _species[index] = species;
            return true;
        }
    }

    public bool Remove(Guid id)
    {
        lock (_syncRoot)
        {
            int index = _species.FindIndex(species => species.Id == id);
            if (index < 0)
            {
                return false;
            }

            _species.RemoveAt(index);
            return true;
        }
    }

    public void ReplaceAll(IEnumerable<AquaticSpecies> species)
    {
        ArgumentNullException.ThrowIfNull(species);

        List<AquaticSpecies> replacement = species.ToList();
        if (replacement.Select(item => item.Id).Distinct().Count() != replacement.Count)
        {
            throw new InvalidOperationException("Aquatic species IDs must be unique.");
        }

        lock (_syncRoot)
        {
            _species.Clear();
            _species.AddRange(replacement);
        }
    }
}

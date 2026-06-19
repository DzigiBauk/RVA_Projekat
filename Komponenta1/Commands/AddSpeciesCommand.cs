using Komponenta1.Helpers;
using Komponenta1.Interfaces;
using Shared.Models;

namespace Komponenta1.Commands;

public sealed class AddSpeciesCommand : IApplicationCommand
{
    private readonly IAquaticSpeciesRepository _repository;
    private readonly AquaticSpecies _species;
    private List<AquaticSpecies>? _before;
    private List<AquaticSpecies>? _after;

    public AddSpeciesCommand(
        IAquaticSpeciesRepository repository,
        AquaticSpecies species)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _species = ModelCloner.Clone(species);
    }

    public string Description => $"Add species '{_species.Name}'";

    public void Execute()
    {
        if (_after is null)
        {
            _before = ModelCloner.CloneSpecies(_repository.GetAll());

            if (_before.Any(species => species.Id == _species.Id))
            {
                throw new InvalidOperationException(
                    $"An aquatic species with ID '{_species.Id}' already exists.");
            }

            _after = ModelCloner.CloneSpecies(_before);
            _after.Add(ModelCloner.Clone(_species));
        }

        _repository.ReplaceAll(ModelCloner.CloneSpecies(_after));
    }

    public void Undo()
    {
        EnsureExecuted();
        _repository.ReplaceAll(ModelCloner.CloneSpecies(_before!));
    }

    private void EnsureExecuted()
    {
        if (_before is null)
        {
            throw new InvalidOperationException("The command has not been executed.");
        }
    }
}

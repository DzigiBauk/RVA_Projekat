using Komponenta1.Helpers;
using Komponenta1.Interfaces;
using Shared.Models;

namespace Komponenta1.Commands;

public sealed class UpdateSpeciesCommand : IUndoableCommand
{
    private readonly IAquaticSpeciesRepository _repository;
    private readonly AquaticSpecies _updatedSpecies;
    private List<AquaticSpecies>? _before;
    private List<AquaticSpecies>? _after;

    public UpdateSpeciesCommand(
        IAquaticSpeciesRepository repository,
        AquaticSpecies updatedSpecies)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _updatedSpecies = ModelCloner.Clone(updatedSpecies);
    }

    public string Description => $"Update species '{_updatedSpecies.Name}'";

    public void Execute()
    {
        if (_after is null)
        {
            _before = ModelCloner.CloneSpecies(_repository.GetAll());
            int index = _before.FindIndex(
                species => species.Id == _updatedSpecies.Id);

            if (index < 0)
            {
                throw new InvalidOperationException(
                    $"Aquatic species '{_updatedSpecies.Id}' was not found.");
            }

            _after = ModelCloner.CloneSpecies(_before);
            _after[index] = ModelCloner.Clone(_updatedSpecies);
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

using Komponenta1.Helpers;
using Komponenta1.Interfaces;
using Shared.Models;

namespace Komponenta1.Commands;

public sealed class DeleteSpeciesCommand : IApplicationCommand
{
    private readonly IAquaticSpeciesRepository _speciesRepository;
    private readonly IWaterQualityReadingRepository _readingRepository;
    private readonly Guid _speciesId;
    private List<AquaticSpecies>? _speciesBefore;
    private List<AquaticSpecies>? _speciesAfter;
    private List<WaterQualityReading>? _readingsBefore;
    private List<WaterQualityReading>? _readingsAfter;
    private string? _speciesName;

    public DeleteSpeciesCommand(
        IAquaticSpeciesRepository speciesRepository,
        IWaterQualityReadingRepository readingRepository,
        Guid speciesId)
    {
        _speciesRepository = speciesRepository
            ?? throw new ArgumentNullException(nameof(speciesRepository));
        _readingRepository = readingRepository
            ?? throw new ArgumentNullException(nameof(readingRepository));
        _speciesId = speciesId;
    }

    public string Description =>
        $"Delete species '{_speciesName ?? _speciesId.ToString()}'";

    public void Execute()
    {
        if (_speciesAfter is null)
        {
            _speciesBefore = ModelCloner.CloneSpecies(
                _speciesRepository.GetAll());
            AquaticSpecies? species = _speciesBefore.FirstOrDefault(
                item => item.Id == _speciesId);

            if (species is null)
            {
                throw new InvalidOperationException(
                    $"Aquatic species '{_speciesId}' was not found.");
            }

            _speciesName = species.Name;
            _readingsBefore = ModelCloner.CloneReadings(
                _readingRepository.GetAll());
            _speciesAfter = _speciesBefore
                .Where(item => item.Id != _speciesId)
                .Select(ModelCloner.Clone)
                .ToList();
            _readingsAfter = _readingsBefore
                .Where(reading => reading.SpeciesId != _speciesId)
                .Select(ModelCloner.Clone)
                .ToList();
        }

        _speciesRepository.ReplaceAll(
            ModelCloner.CloneSpecies(_speciesAfter));
        _readingRepository.ReplaceAll(
            ModelCloner.CloneReadings(_readingsAfter!));
    }

    public void Undo()
    {
        EnsureExecuted();
        _speciesRepository.ReplaceAll(
            ModelCloner.CloneSpecies(_speciesBefore!));
        _readingRepository.ReplaceAll(
            ModelCloner.CloneReadings(_readingsBefore!));
    }

    private void EnsureExecuted()
    {
        if (_speciesBefore is null)
        {
            throw new InvalidOperationException("The command has not been executed.");
        }
    }
}

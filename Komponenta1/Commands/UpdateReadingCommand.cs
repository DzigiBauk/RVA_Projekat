using Komponenta1.Helpers;
using Komponenta1.Interfaces;
using Shared.Models;

namespace Komponenta1.Commands;

public sealed class UpdateReadingCommand : IUndoableCommand
{
    private readonly IWaterQualityReadingRepository _repository;
    private readonly WaterQualityReading _updatedReading;
    private List<WaterQualityReading>? _before;
    private List<WaterQualityReading>? _after;

    public UpdateReadingCommand(
        IWaterQualityReadingRepository repository,
        WaterQualityReading updatedReading)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _updatedReading = ModelCloner.Clone(updatedReading);
    }

    public string Description => $"Update reading '{_updatedReading.Id}'";

    public void Execute()
    {
        if (_after is null)
        {
            _before = ModelCloner.CloneReadings(_repository.GetAll());
            int index = _before.FindIndex(
                reading => reading.Id == _updatedReading.Id);

            if (index < 0)
            {
                throw new InvalidOperationException(
                    $"Water-quality reading '{_updatedReading.Id}' was not found.");
            }

            _after = ModelCloner.CloneReadings(_before);
            _after[index] = ModelCloner.Clone(_updatedReading);
        }

        _repository.ReplaceAll(ModelCloner.CloneReadings(_after));
    }

    public void Undo()
    {
        EnsureExecuted();
        _repository.ReplaceAll(ModelCloner.CloneReadings(_before!));
    }

    private void EnsureExecuted()
    {
        if (_before is null)
        {
            throw new InvalidOperationException("The command has not been executed.");
        }
    }
}

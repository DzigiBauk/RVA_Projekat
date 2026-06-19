using Komponenta1.Helpers;
using Komponenta1.Interfaces;
using Shared.Models;

namespace Komponenta1.Commands;

public sealed class AddReadingCommand : IUndoableCommand
{
    private readonly IWaterQualityReadingRepository _repository;
    private readonly WaterQualityReading _reading;
    private List<WaterQualityReading>? _before;
    private List<WaterQualityReading>? _after;

    public AddReadingCommand(
        IWaterQualityReadingRepository repository,
        WaterQualityReading reading)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _reading = ModelCloner.Clone(reading);
    }

    public string Description => $"Add reading '{_reading.Id}'";

    public void Execute()
    {
        if (_after is null)
        {
            _before = ModelCloner.CloneReadings(_repository.GetAll());

            if (_before.Any(reading => reading.Id == _reading.Id))
            {
                throw new InvalidOperationException(
                    $"A water-quality reading with ID '{_reading.Id}' already exists.");
            }

            _after = ModelCloner.CloneReadings(_before);
            _after.Add(ModelCloner.Clone(_reading));
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

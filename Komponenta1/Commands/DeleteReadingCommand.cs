using Komponenta1.Helpers;
using Komponenta1.Interfaces;
using Shared.Models;

namespace Komponenta1.Commands;

public sealed class DeleteReadingCommand : IApplicationCommand
{
    private readonly IWaterQualityReadingRepository _repository;
    private readonly Guid _readingId;
    private List<WaterQualityReading>? _before;
    private List<WaterQualityReading>? _after;

    public DeleteReadingCommand(
        IWaterQualityReadingRepository repository,
        Guid readingId)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _readingId = readingId;
    }

    public string Description => $"Delete reading '{_readingId}'";

    public void Execute()
    {
        if (_after is null)
        {
            _before = ModelCloner.CloneReadings(_repository.GetAll());

            if (_before.All(reading => reading.Id != _readingId))
            {
                throw new InvalidOperationException(
                    $"Water-quality reading '{_readingId}' was not found.");
            }

            _after = _before
                .Where(reading => reading.Id != _readingId)
                .Select(ModelCloner.Clone)
                .ToList();
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

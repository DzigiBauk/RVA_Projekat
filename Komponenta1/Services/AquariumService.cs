using CoreWCF;
using Komponenta1.Helpers;
using Komponenta1.Interfaces;
using Shared.Contracts;
using Shared.Models;

namespace Komponenta1.Services;

public sealed class AquariumService(
    IAquaticSpeciesRepository speciesRepository,
    IWaterQualityReadingRepository readingRepository,
    IActivityLogger activityLogger) : IAquariumService
{
    public Task<List<AquaticSpecies>> GetSpeciesAsync()
    {
        List<AquaticSpecies> species =
            ModelCloner.CloneSpecies(speciesRepository.GetAll());
        activityLogger.Log(
            $"WCF request: returned {species.Count} aquatic species.");

        return Task.FromResult(species);
    }

    public Task<List<WaterQualityReading>> GetReadingsAsync(
        ReadingsRequest request)
    {
        try
        {
            ValidateRequest(request);

            List<WaterQualityReading> readings = readingRepository
                .GetAll()
                .Where(reading =>
                    reading.SpeciesId == request.SpeciesId &&
                    reading.MeasurementTime.Month == request.Month)
                .Select(ModelCloner.Clone)
                .ToList();

            activityLogger.Log(
                $"WCF request: returned {readings.Count} reading(s) for " +
                $"species {request.SpeciesId}, month {request.Month:D2}.");

            return Task.FromResult(readings);
        }
        catch (FaultException exception)
        {
            activityLogger.Log(
                $"WCF request rejected: {exception.Message}");
            throw;
        }
    }

    private void ValidateRequest(ReadingsRequest? request)
    {
        if (request is null)
        {
            throw new FaultException("The readings request is required.");
        }

        if (request.SpeciesId == Guid.Empty)
        {
            throw new FaultException("Species ID is required.");
        }

        if (request.Month is < 1 or > 12)
        {
            throw new FaultException("Month must be between 1 and 12.");
        }

        if (speciesRepository.GetById(request.SpeciesId) is null)
        {
            throw new FaultException(
                $"Aquatic species '{request.SpeciesId}' does not exist.");
        }
    }
}

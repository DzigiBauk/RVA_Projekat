using Komponenta1.Interfaces;
using Komponenta1.Models;
using Shared.Models;

namespace Komponenta1.Services;

public sealed class WaterQualityReadingValidator(
    IAquaticSpeciesRepository speciesRepository,
    IWaterQualityReadingRepository readingRepository)
    : IValidator<WaterQualityReading>
{
    public const double MinimumPH = 0;
    public const double MaximumPH = 14;
    public const double MinimumTemperature = 0;
    public const double MaximumTemperature = 40;
    public const double MinimumOxygen = 0;
    public const double MaximumOxygen = 20;

    public ValidationResult Validate(
        WaterQualityReading value,
        Guid? existingEntityId = null)
    {
        ArgumentNullException.ThrowIfNull(value);

        ValidationResult result = new();

        if (value.Id == Guid.Empty)
        {
            result.AddError(nameof(value.Id), "ID is required.");
        }
        else
        {
            WaterQualityReading? existing = readingRepository.GetById(value.Id);
            if (existing is not null && existingEntityId != value.Id)
            {
                result.AddError(nameof(value.Id), "ID must be unique.");
            }
        }

        if (value.SpeciesId == Guid.Empty)
        {
            result.AddError(nameof(value.SpeciesId), "Species is required.");
        }
        else if (speciesRepository.GetById(value.SpeciesId) is null)
        {
            result.AddError(
                nameof(value.SpeciesId),
                "The selected species does not exist.");
        }

        if (value.MeasurementTime == default)
        {
            result.AddError(
                nameof(value.MeasurementTime),
                "Measurement time is required.");
        }

        ValidateFiniteRange(
            value.PHLevel,
            MinimumPH,
            MaximumPH,
            nameof(value.PHLevel),
            "pH level",
            result);
        ValidateFiniteRange(
            value.Temperature,
            MinimumTemperature,
            MaximumTemperature,
            nameof(value.Temperature),
            "Temperature",
            result);
        ValidateFiniteRange(
            value.OxygenLevel,
            MinimumOxygen,
            MaximumOxygen,
            nameof(value.OxygenLevel),
            "Oxygen level",
            result);

        return result;
    }

    private static void ValidateFiniteRange(
        double value,
        double minimum,
        double maximum,
        string propertyName,
        string displayName,
        ValidationResult result)
    {
        if (!double.IsFinite(value))
        {
            result.AddError(propertyName, $"{displayName} must be a finite number.");
        }
        else if (value < minimum || value > maximum)
        {
            result.AddError(
                propertyName,
                $"{displayName} must be between {minimum} and {maximum}.");
        }
    }
}

using Komponenta1.Interfaces;
using Komponenta1.Models;
using Shared.Models;

namespace Komponenta1.Services;

public sealed class AquaticSpeciesValidator(
    IAquaticSpeciesRepository speciesRepository) : IValidator<AquaticSpecies>
{
    public const int MinimumLifespan = 1;
    public const int MaximumLifespan = 500;

    private static readonly HashSet<string> AllowedWaterTypes =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "Freshwater",
            "Saltwater",
            "Brackish"
        };

    public ValidationResult Validate(
        AquaticSpecies value,
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
            AquaticSpecies? existing = speciesRepository.GetById(value.Id);
            if (existing is not null && existingEntityId != value.Id)
            {
                result.AddError(nameof(value.Id), "ID must be unique.");
            }
        }

        ValidateRequired(value.Name, nameof(value.Name), "Name", result);
        ValidateRequired(
            value.ScientificName,
            nameof(value.ScientificName),
            "Scientific name",
            result);
        ValidateRequired(value.Habitat, nameof(value.Habitat), "Habitat", result);

        if (string.IsNullOrWhiteSpace(value.WaterType))
        {
            result.AddError(nameof(value.WaterType), "Water type is required.");
        }
        else if (!AllowedWaterTypes.Contains(value.WaterType.Trim()))
        {
            result.AddError(
                nameof(value.WaterType),
                "Water type must be Freshwater, Saltwater, or Brackish.");
        }

        if (value.AverageLifespan is < MinimumLifespan or > MaximumLifespan)
        {
            result.AddError(
                nameof(value.AverageLifespan),
                $"Average lifespan must be between {MinimumLifespan} and {MaximumLifespan} years.");
        }

        return result;
    }

    private static void ValidateRequired(
        string value,
        string propertyName,
        string displayName,
        ValidationResult result)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            result.AddError(propertyName, $"{displayName} is required.");
        }
    }
}

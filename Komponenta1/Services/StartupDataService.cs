using System.IO;
using Komponenta1.Interfaces;
using Shared.Models;

namespace Komponenta1.Services;

public sealed class StartupDataService(
    IAquariumDataService dataService,
    IAquaticSpeciesRepository speciesRepository,
    IWaterQualityReadingRepository readingRepository,
    IActivityLogger activityLogger)
    : IStartupDataService
{
    private static readonly Guid ClownfishId =
        Guid.Parse("00f54355-cc18-4ca5-9976-59d593ca0061");
    private static readonly Guid BettaId =
        Guid.Parse("6c8ebf4d-1606-41bf-b83b-3ef0799a4a38");
    private static readonly Guid GuppyId =
        Guid.Parse("36b5b4f2-cbf1-4358-a9b9-bef0a763ecbd");

    public async Task InitializeAsync(
        string defaultFilePath,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(defaultFilePath);

        if (File.Exists(defaultFilePath) &&
            new FileInfo(defaultFilePath).Length > 0)
        {
            await dataService.LoadAsync(defaultFilePath, cancellationToken);

            if (speciesRepository.GetAll().Count > 0 ||
                readingRepository.GetAll().Count > 0)
            {
                activityLogger.Log(
                    $"Loaded startup data from '{defaultFilePath}'.");
                return;
            }
        }

        speciesRepository.ReplaceAll(CreateSpecies());
        readingRepository.ReplaceAll(CreateReadings());
        activityLogger.Log(
            "Loaded fallback startup data with three species and three readings.");
    }

    private static List<AquaticSpecies> CreateSpecies()
    {
        return
        [
            new AquaticSpecies
            {
                Id = ClownfishId,
                Name = "Clownfish",
                ScientificName = "Amphiprion ocellaris",
                Habitat = "Coral reefs",
                WaterType = "Saltwater",
                AverageLifespan = 8
            },
            new AquaticSpecies
            {
                Id = BettaId,
                Name = "Betta",
                ScientificName = "Betta splendens",
                Habitat = "Shallow tropical waters",
                WaterType = "Freshwater",
                AverageLifespan = 4
            },
            new AquaticSpecies
            {
                Id = GuppyId,
                Name = "Guppy",
                ScientificName = "Poecilia reticulata",
                Habitat = "Tropical streams and pools",
                WaterType = "Freshwater",
                AverageLifespan = 3
            }
        ];
    }

    private static List<WaterQualityReading> CreateReadings()
    {
        return
        [
            new WaterQualityReading
            {
                Id = Guid.Parse("1487d544-e67c-4972-ab06-098e17dfd88f"),
                SpeciesId = ClownfishId,
                MeasurementTime = new DateTime(2026, 1, 15, 10, 30, 0),
                PHLevel = 8.1,
                Temperature = 25.5,
                OxygenLevel = 7.8,
                State = WaterQualityState.Optimal
            },
            new WaterQualityReading
            {
                Id = Guid.Parse("6e41fd2a-b430-4162-b95e-327f6031f182"),
                SpeciesId = BettaId,
                MeasurementTime = new DateTime(2026, 2, 10, 14, 15, 0),
                PHLevel = 7.2,
                Temperature = 26,
                OxygenLevel = 6.5,
                State = WaterQualityState.Acceptable
            },
            new WaterQualityReading
            {
                Id = Guid.Parse("5c32a2ef-bcf4-4f36-b980-b2cdaf3787aa"),
                SpeciesId = GuppyId,
                MeasurementTime = new DateTime(2026, 3, 5, 9, 45, 0),
                PHLevel = 6.8,
                Temperature = 24.5,
                OxygenLevel = 5.9,
                State = WaterQualityState.Suboptimal
            }
        ];
    }
}

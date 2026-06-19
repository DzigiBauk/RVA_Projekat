using Komponenta1.Interfaces;
using Komponenta1.ViewModels;
using Komponenta1.Views;
using Microsoft.Extensions.DependencyInjection;

namespace Komponenta1.Services;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddComponentOne(this IServiceCollection services)
    {
        services.AddSingleton<IAquaticSpeciesRepository, AquaticSpeciesRepository>();
        services.AddSingleton<IWaterQualityReadingRepository, WaterQualityReadingRepository>();
        services.AddSingleton<IAquariumDataService, AquariumDataService>();
        services.AddSingleton<IStartupDataService, StartupDataService>();
        services.AddSingleton<IValidator<Shared.Models.AquaticSpecies>, AquaticSpeciesValidator>();
        services.AddSingleton<IValidator<Shared.Models.WaterQualityReading>, WaterQualityReadingValidator>();
        services.AddSingleton<IAquaticSpeciesSearchService, AquaticSpeciesSearchService>();
        services.AddSingleton<IWaterQualityReadingSearchService, WaterQualityReadingSearchService>();
        services.AddSingleton<IActivityLogger, FileActivityLogger>();
        services.AddSingleton<ICommandExecutor, CommandExecutor>();
        services.AddSingleton<IDialogService, DialogService>();
        services.AddSingleton<IReadingSimulationService, ReadingSimulationService>();
        services.AddSingleton<AquariumService>();
        services.AddSingleton<ICoreWcfHostService, CoreWcfHostService>();
        services.AddSingleton<MainWindowViewModel>();
        services.AddSingleton<MainWindow>();

        return services;
    }
}

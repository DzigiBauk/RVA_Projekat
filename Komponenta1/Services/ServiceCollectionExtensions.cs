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
        services.AddSingleton<MainWindowViewModel>();
        services.AddSingleton<MainWindow>();

        return services;
    }
}

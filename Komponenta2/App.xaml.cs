using System.Windows;
using Komponenta2.Services;
using Komponenta2.ViewModels;
using Komponenta2.Views;
using Microsoft.Extensions.DependencyInjection;
using Shared.Contracts;
using System.ServiceModel;
using Komponenta2.Interfaces;
using Komponenta2.Services.Adapters;
using Komponenta2.Services.Strategies;

namespace Komponenta2;

public partial class App : Application
{
    private ServiceProvider? _serviceProvider;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        ServiceCollection services = new();

        //View + ViewModel
        services.AddSingleton<StatisticsViewModel>();
        services.AddSingleton<StatisticsView>();

        // WCF proxy
        services.AddSingleton<IAquariumService>(_ =>
        {
            var factory = new ChannelFactory<IAquariumService>(
                new BasicHttpBinding(),
                new EndpointAddress(AquariumServiceDefaults.EndpointAddress));

            return factory.CreateChannel();
        });

        // Client wrapper
        services.AddSingleton<IAquariumClient, AquariumClient>();

        // Services
        services.AddSingleton<WaterQualityAdapter>();
        services.AddSingleton<IStatisticsService, StatisticsService>();
        services.AddSingleton<IExportService, ExportService>();
        services.AddSingleton<IStatisticsStrategy, AveragePhStrategy>();
        services.AddSingleton<IStatisticsStrategy, MinimalOxygenStrategy>();
        services.AddSingleton<IStatisticsStrategy, CriticalCountStrategy>();
        services.AddSingleton<IStatisticsStrategyFactory, StatisticsStrategyFactory>();

        _serviceProvider = services.BuildServiceProvider(
            new ServiceProviderOptions
            {
                ValidateOnBuild = true,
                ValidateScopes = true
            });

        _serviceProvider.GetRequiredService<StatisticsView>().Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _serviceProvider?.Dispose();
        base.OnExit(e);
    }
}
using System.IO;
using System.Windows;
using Komponenta1.Interfaces;
using Komponenta1.Services;
using Komponenta1.Views;
using Microsoft.Extensions.DependencyInjection;

namespace Komponenta1;

public partial class App : Application
{
    private ServiceProvider? _serviceProvider;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        ServiceCollection services = new();
        services.AddComponentOne();

        _serviceProvider = services.BuildServiceProvider(
            new ServiceProviderOptions
            {
                ValidateOnBuild = true,
                ValidateScopes = true
            });

        string defaultFilePath = Path.Combine(
            AppContext.BaseDirectory,
            "aquarium-data.json");
        await _serviceProvider
            .GetRequiredService<IStartupDataService>()
            .InitializeAsync(defaultFilePath);

        _serviceProvider
            .GetRequiredService<IActivityLogger>()
            .Log("Application started.");
        _serviceProvider.GetRequiredService<MainWindow>().Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _serviceProvider?
            .GetService<IActivityLogger>()?
            .Log("Application stopped.");
        _serviceProvider?
            .GetService<IReadingSimulationService>()?
            .Stop();
        _serviceProvider?.Dispose();
        base.OnExit(e);
    }
}

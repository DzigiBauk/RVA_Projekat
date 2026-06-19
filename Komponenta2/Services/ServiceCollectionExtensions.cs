using Komponenta2.ViewModels;
using Komponenta2.Views;
using Microsoft.Extensions.DependencyInjection;

namespace Komponenta2.Services;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddComponentTwo(this IServiceCollection services)
    {
        services.AddSingleton<MainWindowViewModel>();
        services.AddSingleton<MainWindow>();

        return services;
    }
}

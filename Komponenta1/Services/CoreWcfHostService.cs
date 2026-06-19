using CoreWCF;
using CoreWCF.Configuration;
using Komponenta1.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Shared.Contracts;

namespace Komponenta1.Services;

public sealed class CoreWcfHostService(
    AquariumService aquariumService,
    IActivityLogger activityLogger) : ICoreWcfHostService
{
    private WebApplication? _application;

    public bool IsRunning { get; private set; }

    public string Status { get; private set; } = "Stopped";

    public event EventHandler? StatusChanged;

    public async Task StartAsync(
        CancellationToken cancellationToken = default)
    {
        if (IsRunning)
        {
            return;
        }

        try
        {
            Uri endpoint = new(AquariumServiceDefaults.EndpointAddress);
            string baseAddress =
                $"{endpoint.Scheme}://{endpoint.Authority}";

            WebApplicationOptions options = new()
            {
                ApplicationName = typeof(CoreWcfHostService).Assembly.FullName,
                ContentRootPath = AppContext.BaseDirectory
            };
            WebApplicationBuilder builder =
                WebApplication.CreateBuilder(options);

            builder.WebHost.UseUrls(baseAddress);
            builder.Services.AddServiceModelServices();
            builder.Services.AddSingleton(aquariumService);

            WebApplication application = builder.Build();
            application.UseServiceModel(serviceBuilder =>
            {
                serviceBuilder.AddService<AquariumService>();
                serviceBuilder.AddServiceEndpoint<
                    AquariumService,
                    IAquariumService>(
                    new BasicHttpBinding(),
                    endpoint.AbsolutePath);
            });

            await application.StartAsync(cancellationToken);
            _application = application;
            IsRunning = true;
            SetStatus($"Running at {AquariumServiceDefaults.EndpointAddress}");
            activityLogger.Log(
                $"CoreWCF service started at " +
                $"'{AquariumServiceDefaults.EndpointAddress}'.");
        }
        catch (Exception exception)
        {
            IsRunning = false;
            _application = null;
            SetStatus($"Unavailable: {exception.Message}");
            activityLogger.Log(
                $"CoreWCF service failed to start: {exception.Message}");
        }
    }

    public async Task StopAsync(
        CancellationToken cancellationToken = default)
    {
        if (_application is null)
        {
            IsRunning = false;
            SetStatus("Stopped");
            return;
        }

        try
        {
            await _application.StopAsync(cancellationToken);
            await _application.DisposeAsync();
            activityLogger.Log("CoreWCF service stopped.");
        }
        catch (Exception exception)
        {
            activityLogger.Log(
                $"CoreWCF service failed to stop cleanly: {exception.Message}");
        }
        finally
        {
            _application = null;
            IsRunning = false;
            SetStatus("Stopped");
        }
    }

    private void SetStatus(string status)
    {
        Status = status;
        StatusChanged?.Invoke(this, EventArgs.Empty);
    }
}

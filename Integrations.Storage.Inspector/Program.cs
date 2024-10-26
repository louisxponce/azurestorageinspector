using Integrations.Storage.Inspector;
using Integrations.Storage.Inspector.Models;
using Integrations.Storage.Inspector.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddOptions<AppSettings>()
            .Configure<IConfiguration>((settings, configuration) =>
            {
                configuration.GetSection("AppSettings").Bind(settings);
            });
        services.AddSingleton<ServiceBusService, ServiceBusService>();
        services.AddSingleton<StorageService, StorageService>();
        services.AddSingleton<LocalBlobService, LocalBlobService>();
        services.AddHostedService<App>();
    })
    .Build();

await host.RunAsync();


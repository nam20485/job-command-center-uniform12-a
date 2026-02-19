using JobCommandCenter.Data;
using JobCommandCenter.Harvester.Services;
using JobCommandCenter.ServiceDefaults;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace JobCommandCenter.Harvester;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        // Configure service defaults (OpenTelemetry, health checks, etc.)
        builder.AddServiceDefaults();

        // Add database context
        builder.Services.AddDbContext<JobCommandCenterDbContext>();

        // Register the harvester worker service
        builder.Services.AddHostedService<LinkedInHarvesterService>();

        // Register Playwright services
        builder.Services.AddSingleton<IPlaywrightService, PlaywrightService>();

        // Register scraping services
        builder.Services.AddScoped<ILinkedInScraper, LinkedInScraper>();

        var host = builder.Build();
        await host.RunAsync();
    }
}

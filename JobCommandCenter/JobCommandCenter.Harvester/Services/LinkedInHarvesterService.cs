using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Playwright;
using JobCommandCenter.Harvester.Services;
using JobCommandCenter.Data;

namespace JobCommandCenter.Harvester;

/// <summary>
/// Background worker service that orchestrates the LinkedIn harvesting process.
/// </summary>
public class LinkedInHarvesterService : BackgroundService
{
    private readonly ILogger<LinkedInHarvesterService> _logger;
    private readonly IPlaywrightService _playwrightService;
    private readonly ILinkedInScraper _scraper;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _configuration;

    private readonly int _chromeDebugPort;
    private readonly int _searchCycleDelayMs;
    private readonly int _minDelayMs;
    private readonly int _maxDelayMs;
    private readonly string _linkedinSearchUrl;
    private readonly bool _humanizationEnabled;

    private readonly Random _random = new();

    public LinkedInHarvesterService(
        ILogger<LinkedInHarvesterService> logger,
        IPlaywrightService playwrightService,
        ILinkedInScraper scraper,
        IServiceScopeFactory scopeFactory,
        IConfiguration configuration)
    {
        _logger = logger;
        _playwrightService = playwrightService;
        _scraper = scraper;
        _scopeFactory = scopeFactory;
        _configuration = configuration;

        // Load configuration
        _chromeDebugPort = configuration.GetValue("Chrome:DebugPort", 9222);
        _searchCycleDelayMs = configuration.GetValue("Harvester:SearchCycleDelayMs", 60000);
        _minDelayMs = configuration.GetValue("Harvester:MinDelayMs", 2000);
        _maxDelayMs = configuration.GetValue("Harvester:MaxDelayMs", 8000);
        _linkedinSearchUrl = configuration["LinkedIn:SearchUrl"] ?? "https://www.linkedin.com/jobs/search/";
        _humanizationEnabled = configuration.GetValue("Harvester:HumanizationEnabled", true);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("LinkedIn Harvester Service starting...");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunHarvestCycleAsync(stoppingToken);
            }
            catch (ChromeNotRunningException ex)
            {
                _logger.LogWarning(ex, "Chrome is not running. Waiting before retry...");
                await HumanizedDelayAsync(30000, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during harvest cycle");
                await HumanizedDelayAsync(10000, stoppingToken);
            }

            // Wait before next cycle
            await HumanizedDelayAsync(_searchCycleDelayMs, stoppingToken);
        }
    }

    private async Task RunHarvestCycleAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting harvest cycle...");

        // Connect to Chrome
        await using var browser = await _playwrightService.ConnectToChromeAsync(_chromeDebugPort, stoppingToken);
        var context = browser.Contexts.FirstOrDefault() ?? await browser.NewContextAsync();
        var page = context.Pages.FirstOrDefault() ?? await context.NewPageAsync();

        // Navigate to LinkedIn
        await _scraper.NavigateToSearchAsync(page, _linkedinSearchUrl, stoppingToken);

        // Check authentication
        if (!await _scraper.IsAuthenticatedAsync(page))
        {
            _logger.LogError("Not authenticated to LinkedIn. Please log in via Chrome.");
            throw new InvalidOperationException("LinkedIn session not authenticated");
        }

        // Scrape jobs
        var jobs = await _scraper.ScrapeJobsAsync(page, stoppingToken);
        _logger.LogInformation("Harvested {Count} jobs", jobs.Count());

        // Save to database
        await SaveJobsAsync(jobs, stoppingToken);

        _logger.LogInformation("Harvest cycle completed");
    }

    private async Task SaveJobsAsync(IEnumerable<Shared.Models.Job> jobs, CancellationToken stoppingToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<JobCommandCenterDbContext>();

        foreach (var job in jobs)
        {
            if (stoppingToken.IsCancellationRequested)
                break;

            // Check for duplicates
            var exists = await dbContext.Jobs
                .AnyAsync(j => j.LinkedInJobId == job.LinkedInJobId, stoppingToken);

            if (!exists)
            {
                dbContext.Jobs.Add(job);
                _logger.LogDebug("Added new job: {Title} at {Company}", job.Title, job.Company);
            }
        }

        await dbContext.SaveChangesAsync(stoppingToken);
    }

    /// <summary>
    /// Applies a humanized (random) delay to avoid detection.
    /// </summary>
    private async Task HumanizedDelayAsync(int baseMs, CancellationToken stoppingToken)
    {
        var delay = _humanizationEnabled
            ? baseMs + _random.Next(-baseMs / 4, baseMs / 4)  // +/- 25% jitter
            : baseMs;

        delay = Math.Max(delay, 1000);  // Minimum 1 second

        _logger.LogDebug("Applying humanized delay: {Delay}ms", delay);
        await Task.Delay(delay, stoppingToken);
    }
}

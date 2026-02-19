using Microsoft.Playwright;
using Microsoft.Extensions.Logging;
using JobCommandCenter.Shared.Enums;
using JobCommandCenter.Shared.Models;

namespace JobCommandCenter.Harvester.Services;

/// <summary>
/// Interface for LinkedIn scraping operations.
/// </summary>
public interface ILinkedInScraper
{
    /// <summary>
    /// Scrapes job listings from the current page.
    /// </summary>
    Task<IEnumerable<Job>> ScrapeJobsAsync(IPage page, CancellationToken cancellationToken = default);

    /// <summary>
    /// Navigates to the LinkedIn job search.
    /// </summary>
    Task NavigateToSearchAsync(IPage page, string searchUrl, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if the current session is authenticated.
    /// </summary>
    Task<bool> IsAuthenticatedAsync(IPage page);
}

/// <summary>
/// LinkedIn scraper implementation.
/// </summary>
public class LinkedInScraper : ILinkedInScraper
{
    private readonly ILogger<LinkedInScraper> _logger;

    public LinkedInScraper(ILogger<LinkedInScraper> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Navigates to the LinkedIn job search page.
    /// </summary>
    public async Task NavigateToSearchAsync(IPage page, string searchUrl, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Navigating to LinkedIn job search...");
        await page.GotoAsync(searchUrl, new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle,
            Timeout = 30000
        });
    }

    /// <summary>
    /// Checks if the user is logged in to LinkedIn.
    /// </summary>
    public async Task<bool> IsAuthenticatedAsync(IPage page)
    {
        try
        {
            // Check for login button presence (indicates not logged in)
            var loginButton = await page.QuerySelectorAsync("a[href*='login']");
            if (loginButton != null)
            {
                _logger.LogWarning("User appears to be logged out of LinkedIn");
                return false;
            }

            // Check for profile menu (indicates logged in)
            var profileMenu = await page.QuerySelectorAsync("[data-control-name='identity_welcome_message']");
            return profileMenu != null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking authentication status");
            return false;
        }
    }

    /// <summary>
    /// Scrapes job listings from the current page.
    /// </summary>
    public async Task<IEnumerable<Job>> ScrapeJobsAsync(IPage page, CancellationToken cancellationToken = default)
    {
        var jobs = new List<Job>();

        try
        {
            _logger.LogInformation("Scraping job listings...");

            // Wait for job cards to load
            await page.WaitForSelectorAsync(".job-card-container", new PageWaitForSelectorOptions
            {
                Timeout = 10000
            });

            var jobCards = await page.QuerySelectorAllAsync(".job-card-container");
            _logger.LogInformation("Found {Count} job cards", jobCards.Count);

            foreach (var card in jobCards)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                var job = await ScrapeJobCardAsync(card);
                if (job != null)
                {
                    jobs.Add(job);
                }
            }
        }
        catch (TimeoutException)
        {
            _logger.LogWarning("Timeout waiting for job cards - page may not be loaded correctly");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scraping job listings");
        }

        return jobs;
    }

    private async Task<Job?> ScrapeJobCardAsync(IElementHandle card)
    {
        try
        {
            var job = new Job
            {
                LinkedInJobId = await GetAttributeAsync(card, "data-job-id") ?? Guid.NewGuid().ToString(),
                Title = await GetTextAsync(card, ".job-card-list__title") ?? "Unknown Title",
                Company = await GetTextAsync(card, ".job-card-container__company-name") ?? "Unknown Company",
                Location = await GetTextAsync(card, ".job-card-container__metadata-item"),
                JobUrl = "https://www.linkedin.com/jobs/view/" + await GetAttributeAsync(card, "data-job-id"),
                DateFound = DateTime.UtcNow,
                Status = JobStatus.Found
            };

            // Check for promoted/sponsored listings
            var promotedLabel = await card.QuerySelectorAsync(".job-card-container__footer-item--promoted");
            job.IsPromoted = promotedLabel != null;

            // Check for Easy Apply
            var easyApplyButton = await card.QuerySelectorAsync("[data-job-easy-apply]");
            job.ApplicationType = easyApplyButton != null ? ApplicationType.EasyApply : ApplicationType.ExternalApply;

            // Check for remote indicator
            var location = job.Location?.ToLowerInvariant() ?? "";
            job.IsRemote = location.Contains("remote") || location.Contains("work from home");

            _logger.LogDebug("Scraped job: {Title} at {Company}", job.Title, job.Company);
            return job;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to scrape job card");
            return null;
        }
    }

    private static async Task<string?> GetTextAsync(IElementHandle parent, string selector)
    {
        var element = await parent.QuerySelectorAsync(selector);
        return element != null ? await element.TextContentAsync() : null;
    }

    private static async Task<string?> GetAttributeAsync(IElementHandle element, string attribute)
    {
        return await element.GetAttributeAsync(attribute);
    }
}

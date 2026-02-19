using Microsoft.Playwright;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace JobCommandCenter.Harvester.Services;

/// <summary>
/// Exception thrown when Chrome is not running or CDP port is not accessible.
/// </summary>
public class ChromeNotRunningException : Exception
{
    public ChromeNotRunningException(string message) : base(message) { }
    public ChromeNotRunningException(string message, Exception innerException) 
        : base(message, innerException) { }
}

/// <summary>
/// Interface for Playwright browser automation services.
/// </summary>
public interface IPlaywrightService : IDisposable
{
    /// <summary>
    /// Connects to an existing Chrome instance via CDP.
    /// </summary>
    Task<IBrowser> ConnectToChromeAsync(int port, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if Chrome is running with debugging port enabled.
    /// </summary>
    Task<bool> IsChromeRunningAsync(int port);
}

/// <summary>
/// Playwright service implementation for connecting to Chrome via CDP.
/// </summary>
public class PlaywrightService : IPlaywrightService
{
    private readonly ILogger<PlaywrightService> _logger;
    private readonly HttpClient _httpClient;
    private IPlaywright? _playwright;
    private IBrowser? _browser;
    private bool _disposed;

    public PlaywrightService(ILogger<PlaywrightService> logger)
    {
        _logger = logger;
        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(5)
        };
    }

    /// <summary>
    /// Connects to an existing Chrome instance via Chrome DevTools Protocol.
    /// </summary>
    /// <param name="port">The debugging port (default: 9222).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Connected browser instance.</returns>
    /// <exception cref="ChromeNotRunningException">Thrown when Chrome is not accessible on the specified port.</exception>
    public async Task<IBrowser> ConnectToChromeAsync(int port, CancellationToken cancellationToken = default)
    {
        if (_browser != null)
        {
            return _browser;
        }

        _logger.LogInformation("Attempting to connect to Chrome on port {Port} via CDP...", port);

        // First verify Chrome is running
        if (!await IsChromeRunningAsync(port))
        {
            var message = $"Chrome is not running or CDP port {port} is not accessible. " +
                          $"Please start Chrome with --remote-debugging-port={port}";
            _logger.LogError(message);
            throw new ChromeNotRunningException(message);
        }

        try
        {
            _playwright = await Playwright.CreateAsync();
            var endpoint = $"http://localhost:{port}";

            _browser = await _playwright.Chromium.ConnectOverCDPAsync(endpoint, new()
            {
                Timeout = 30000
            });

            _logger.LogInformation("Successfully connected to Chrome via CDP");
            return _browser;
        }
        catch (Exception ex) when (ex is not ChromeNotRunningException)
        {
            _logger.LogError(ex, "Failed to connect to Chrome via CDP");
            throw new ChromeNotRunningException($"Failed to connect to Chrome: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Checks if Chrome is running with the debugging port enabled.
    /// </summary>
    public async Task<bool> IsChromeRunningAsync(int port)
    {
        try
        {
            var response = await _httpClient.GetAsync($"http://localhost:{port}/json/version");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Chrome CDP port {Port} not accessible", port);
            return false;
        }
    }

    public void Dispose()
    {
        if (_disposed) return;

        _browser?.DisposeAsync().GetAwaiter().GetResult();
        _playwright?.Dispose();
        _httpClient.Dispose();

        _disposed = true;
    }
}

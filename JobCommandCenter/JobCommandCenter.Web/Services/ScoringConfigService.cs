using JobCommandCenter.Data;
using JobCommandCenter.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace JobCommandCenter.Web.Services;

/// <summary>
/// Service interface for scoring configuration operations.
/// </summary>
public interface IScoringConfigService
{
    Task<List<ScoringConfig>> GetAllConfigsAsync();
    Task<ScoringConfig?> GetConfigByIdAsync(Guid id);
    Task<ScoringConfig> CreateConfigAsync(ScoringConfig config);
    Task<ScoringConfig> UpdateConfigAsync(ScoringConfig config);
    Task<bool> DeleteConfigAsync(Guid id);
    Task<List<ScoringConfig>> GetActiveConfigsAsync();
}

/// <summary>
/// Implementation of scoring config service using Entity Framework Core.
/// </summary>
public class ScoringConfigService : IScoringConfigService
{
    private readonly JobCommandCenterDbContext _dbContext;
    private readonly ILogger<ScoringConfigService> _logger;

    public ScoringConfigService(JobCommandCenterDbContext dbContext, ILogger<ScoringConfigService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<List<ScoringConfig>> GetAllConfigsAsync()
    {
        return await _dbContext.ScoringConfigs
            .OrderByDescending(c => c.IsActive)
            .ThenBy(c => c.Order)
            .ToListAsync();
    }

    public async Task<ScoringConfig?> GetConfigByIdAsync(Guid id)
    {
        return await _dbContext.ScoringConfigs.FindAsync(id);
    }

    public async Task<ScoringConfig> CreateConfigAsync(ScoringConfig config)
    {
        _dbContext.ScoringConfigs.Add(config);
        await _dbContext.SaveChangesAsync();
        _logger.LogInformation("Created scoring config: {Name}", config.Name);
        return config;
    }

    public async Task<ScoringConfig> UpdateConfigAsync(ScoringConfig config)
    {
        _dbContext.ScoringConfigs.Update(config);
        await _dbContext.SaveChangesAsync();
        _logger.LogInformation("Updated scoring config: {Name}", config.Name);
        return config;
    }

    public async Task<bool> DeleteConfigAsync(Guid id)
    {
        var config = await _dbContext.ScoringConfigs.FindAsync(id);
        if (config == null) return false;

        _dbContext.ScoringConfigs.Remove(config);
        await _dbContext.SaveChangesAsync();
        _logger.LogInformation("Deleted scoring config ID: {Id}", id);
        return true;
    }

    public async Task<List<ScoringConfig>> GetActiveConfigsAsync()
    {
        return await _dbContext.ScoringConfigs
            .Where(c => c.IsActive)
            .OrderBy(c => c.Order)
            .ToListAsync();
    }
}

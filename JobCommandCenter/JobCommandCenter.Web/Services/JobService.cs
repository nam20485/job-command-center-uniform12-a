using JobCommandCenter.Data;
using JobCommandCenter.Shared.Models;
using JobCommandCenter.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace JobCommandCenter.Web.Services;

/// <summary>
/// Service interface for job-related operations.
/// </summary>
public interface IJobService
{
    Task<List<Job>> GetJobsAsync(int skip = 0, int take = 50, JobStatus? status = null);
    Task<Job?> GetJobByIdAsync(Guid id);
    Task<Job> CreateJobAsync(Job job);
    Task<Job> UpdateJobAsync(Job job);
    Task<bool> DeleteJobAsync(Guid id);
    Task<int> GetTotalJobCountAsync();
    Task<Dictionary<JobStatus, int>> GetJobStatsAsync();
}

/// <summary>
/// Implementation of job service using Entity Framework Core.
/// </summary>
public class JobService : IJobService
{
    private readonly JobCommandCenterDbContext _dbContext;
    private readonly ILogger<JobService> _logger;

    public JobService(JobCommandCenterDbContext dbContext, ILogger<JobService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<List<Job>> GetJobsAsync(int skip = 0, int take = 50, JobStatus? status = null)
    {
        var query = _dbContext.Jobs.AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(j => j.Status == status.Value);
        }

        return await query
            .OrderByDescending(j => j.Score)
            .ThenByDescending(j => j.DateFound)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }

    public async Task<Job?> GetJobByIdAsync(Guid id)
    {
        return await _dbContext.Jobs.FindAsync(id);
    }

    public async Task<Job> CreateJobAsync(Job job)
    {
        job.DateFound = DateTime.UtcNow;
        job.LastModified = DateTime.UtcNow;
        _dbContext.Jobs.Add(job);
        await _dbContext.SaveChangesAsync();
        _logger.LogInformation("Created job: {Title} at {Company}", job.Title, job.Company);
        return job;
    }

    public async Task<Job> UpdateJobAsync(Job job)
    {
        job.LastModified = DateTime.UtcNow;
        _dbContext.Jobs.Update(job);
        await _dbContext.SaveChangesAsync();
        _logger.LogInformation("Updated job: {Title} at {Company}", job.Title, job.Company);
        return job;
    }

    public async Task<bool> DeleteJobAsync(Guid id)
    {
        var job = await _dbContext.Jobs.FindAsync(id);
        if (job == null) return false;

        _dbContext.Jobs.Remove(job);
        await _dbContext.SaveChangesAsync();
        _logger.LogInformation("Deleted job ID: {Id}", id);
        return true;
    }

    public async Task<int> GetTotalJobCountAsync()
    {
        return await _dbContext.Jobs.CountAsync();
    }

    public async Task<Dictionary<JobStatus, int>> GetJobStatsAsync()
    {
        return await _dbContext.Jobs
            .GroupBy(j => j.Status)
            .ToDictionaryAsync(g => g.Key, g => g.Count());
    }
}

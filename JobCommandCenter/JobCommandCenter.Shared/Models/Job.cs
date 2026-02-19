using JobCommandCenter.Shared.Enums;

namespace JobCommandCenter.Shared.Models;

/// <summary>
/// Represents a job listing scraped from LinkedIn.
/// </summary>
public class Job
{
    /// <summary>
    /// Unique identifier for the job record.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// LinkedIn's unique job ID.
    /// </summary>
    public required string LinkedInJobId { get; set; }

    /// <summary>
    /// Job title.
    /// </summary>
    public required string Title { get; set; }

    /// <summary>
    /// Company name.
    /// </summary>
    public required string Company { get; set; }

    /// <summary>
    /// Job location (city/state/country or "Remote").
    /// </summary>
    public string? Location { get; set; }

    /// <summary>
    /// Salary range or compensation information.
    /// </summary>
    public string? Salary { get; set; }

    /// <summary>
    /// URL to the LinkedIn job posting.
    /// </summary>
    public required string JobUrl { get; set; }

    /// <summary>
    /// Full job description text.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Whether the job is marked as "Promoted" (sponsored listing).
    /// </summary>
    public bool IsPromoted { get; set; }

    /// <summary>
    /// Whether the job is no longer accepting applications.
    /// </summary>
    public bool IsExpired { get; set; }

    /// <summary>
    /// Whether this is a remote position.
    /// </summary>
    public bool IsRemote { get; set; }

    /// <summary>
    /// The type of application method.
    /// </summary>
    public ApplicationType ApplicationType { get; set; }

    /// <summary>
    /// Current status in the job pipeline.
    /// </summary>
    public JobStatus Status { get; set; } = JobStatus.Found;

    /// <summary>
    /// Calculated relevance score based on scoring matrix.
    /// </summary>
    public int Score { get; set; }

    /// <summary>
    /// Date the job was first scraped.
    /// </summary>
    public DateTime DateFound { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date of last status change.
    /// </summary>
    public DateTime LastModified { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// User notes for this job.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Navigation to history log entries.
    /// </summary>
    public ICollection<HistoryLog> HistoryLogs { get; set; } = [];
}

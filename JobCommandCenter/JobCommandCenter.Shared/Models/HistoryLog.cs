using JobCommandCenter.Shared.Enums;

namespace JobCommandCenter.Shared.Models;

/// <summary>
/// Represents an audit trail entry for job status changes.
/// </summary>
public class HistoryLog
{
    /// <summary>
    /// Unique identifier for the history entry.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Foreign key to the job.
    /// </summary>
    public Guid JobId { get; set; }

    /// <summary>
    /// Navigation to the job.
    /// </summary>
    public Job? Job { get; set; }

    /// <summary>
    /// The previous status (null if new job).
    /// </summary>
    public JobStatus? PreviousStatus { get; set; }

    /// <summary>
    /// The new status.
    /// </summary>
    public JobStatus NewStatus { get; set; }

    /// <summary>
    /// Source of the change (System/Harvester or User).
    /// </summary>
    public required string ChangedBy { get; set; }

    /// <summary>
    /// Timestamp of the change.
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Optional note explaining the change.
    /// </summary>
    public string? Note { get; set; }
}

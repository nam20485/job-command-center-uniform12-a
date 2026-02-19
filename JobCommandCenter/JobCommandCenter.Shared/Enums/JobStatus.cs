namespace JobCommandCenter.Shared.Enums;

/// <summary>
/// Represents the lifecycle stages of a job application.
/// </summary>
public enum JobStatus
{
    /// <summary>
    /// Job has been scraped but not yet reviewed.
    /// </summary>
    Found = 0,

    /// <summary>
    /// Job has been processed by the scoring engine.
    /// </summary>
    Scored = 1,

    /// <summary>
    /// User has approved the job for application.
    /// </summary>
    Pending = 2,

    /// <summary>
    /// Application has been submitted.
    /// </summary>
    Applied = 3,

    /// <summary>
    /// In the interview process.
    /// </summary>
    Interviewing = 4,

    /// <summary>
    /// Archived (rejected, closed, or no longer relevant).
    /// </summary>
    Archive = 5
}

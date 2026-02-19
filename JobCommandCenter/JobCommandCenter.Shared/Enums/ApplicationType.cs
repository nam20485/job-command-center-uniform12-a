namespace JobCommandCenter.Shared.Enums;

/// <summary>
/// Represents the type of application method for a job.
/// </summary>
public enum ApplicationType
{
    /// <summary>
    /// LinkedIn's native "Easy Apply" button.
    /// </summary>
    EasyApply = 0,

    /// <summary>
    /// External application (redirects to company site).
    /// </summary>
    ExternalApply = 1
}

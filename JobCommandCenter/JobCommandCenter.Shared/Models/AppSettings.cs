namespace JobCommandCenter.Shared.Models;

/// <summary>
/// Application-wide settings.
/// </summary>
public class AppSettings
{
    /// <summary>
    /// Unique identifier.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Setting key name.
    /// </summary>
    public required string Key { get; set; }

    /// <summary>
    /// Setting value (JSON serialized for complex types).
    /// </summary>
    public required string Value { get; set; }

    /// <summary>
    /// Human-readable description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Category for grouping settings.
    /// </summary>
    public string? Category { get; set; }
}

namespace JobCommandCenter.Shared.Models;

/// <summary>
/// Represents a user-configurable scoring rule for job ranking.
/// </summary>
public class ScoringConfig
{
    /// <summary>
    /// Unique identifier for the scoring rule.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Display name for the rule (e.g., "Remote Bonus", "Python Requirement").
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Description of what this rule evaluates.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// The keyword or pattern to match.
    /// </summary>
    public required string Keyword { get; set; }

    /// <summary>
    /// Points to add (positive) or subtract (negative) when matched.
    /// </summary>
    public int Weight { get; set; }

    /// <summary>
    /// Whether this rule is currently active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Field to search (Title, Description, Location, etc.).
    /// </summary>
    public ScoringField Field { get; set; } = ScoringField.All;

    /// <summary>
    /// Match type (Exact, Contains, Regex).
    /// </summary>
    public MatchType MatchType { get; set; } = MatchType.Contains;

    /// <summary>
    /// Case sensitivity for matching.
    /// </summary>
    public bool CaseSensitive { get; set; } = false;

    /// <summary>
    /// Display order for the rule in the UI.
    /// </summary>
    public int Order { get; set; } = 0;
}

/// <summary>
/// Which field to apply scoring rule to.
/// </summary>
public enum ScoringField
{
    All,
    Title,
    Description,
    Location,
    Company
}

/// <summary>
/// How to match keywords.
/// </summary>
public enum MatchType
{
    Exact,
    Contains,
    Regex
}

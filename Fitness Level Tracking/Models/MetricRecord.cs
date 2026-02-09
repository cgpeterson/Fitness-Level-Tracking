namespace Fitness_Level_Tracking.Models;

/// <summary>
/// Represents a single metric measurement taken during a quarterly assessment.
/// </summary>
public sealed class MetricRecord
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public required FitnessGroup Group { get; init; }

    public required FitnessMetricType MetricType { get; init; }

    public required double Value { get; init; }

    public required DateOnly RecordedDate { get; init; }

    public required int Quarter { get; init; }

    public required int Year { get; init; }

    public string? Notes { get; init; }

    /// <summary>
    /// Gets the quarter label for display purposes (e.g., "Q1 2024").
    /// </summary>
    public string QuarterLabel => $"Q{Quarter} {Year}";
}

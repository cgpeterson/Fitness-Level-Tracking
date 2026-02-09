using Fitness_Level_Tracking.Models;

namespace Fitness_Level_Tracking.Services;

/// <summary>
/// Provides utility operations for fitness metrics.
/// </summary>
public interface IMetricService
{
    /// <summary>
    /// Gets the display name for a metric type.
    /// </summary>
    string GetMetricDisplayName(FitnessMetricType metricType);

    /// <summary>
    /// Gets the display name for a fitness group.
    /// </summary>
    string GetGroupDisplayName(FitnessGroup group);

    /// <summary>
    /// Gets the unit of measurement for a metric type.
    /// </summary>
    string GetMetricUnit(FitnessMetricType metricType);

    /// <summary>
    /// Determines if a lower value is better for this metric type.
    /// </summary>
    bool IsLowerBetter(FitnessMetricType metricType);

    /// <summary>
    /// Calculates the percentage improvement between two values for a metric.
    /// </summary>
    double CalculateImprovement(FitnessMetricType metricType, double oldValue, double newValue);

    /// <summary>
    /// Gets the current quarter based on a date.
    /// </summary>
    int GetQuarter(DateOnly date);

    /// <summary>
    /// Gets all available metric types.
    /// </summary>
    IReadOnlyList<FitnessMetricType> GetAllMetricTypes();

    /// <summary>
    /// Gets all metric types for a specific group.
    /// </summary>
    IReadOnlyList<FitnessMetricType> GetMetricsForGroup(FitnessGroup group);

    /// <summary>
    /// Gets the group that a metric belongs to.
    /// </summary>
    FitnessGroup GetGroupForMetric(FitnessMetricType metricType);

    /// <summary>
    /// Evaluates the performance tier for a metric value.
    /// </summary>
    /// <param name="metricType">The type of metric.</param>
    /// <param name="value">The recorded value.</param>
    /// <param name="bodyweightLbs">Athlete's bodyweight (for relative strength metrics).</param>
    /// <param name="isMale">Athlete's sex (for gender-specific thresholds).</param>
    PerformanceTier EvaluateTier(FitnessMetricType metricType, double value, double? bodyweightLbs = null, bool? isMale = null);

    /// <summary>
    /// Gets the tier thresholds for display purposes.
    /// </summary>
    (string Average, string Good, string Peak) GetTierThresholds(FitnessMetricType metricType, bool isMale = true);

    /// <summary>
    /// Gets all fitness groups.
    /// </summary>
    IReadOnlyList<FitnessGroup> GetAllGroups();
}

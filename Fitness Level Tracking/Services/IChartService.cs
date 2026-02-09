using Fitness_Level_Tracking.Models;

namespace Fitness_Level_Tracking.Services;

/// <summary>
/// Configuration for chart rendering.
/// </summary>
public sealed class ChartConfiguration
{
    public bool ShowLegend { get; init; } = true;
    public bool ShowGrid { get; init; } = true;
    public bool OverlayAthletes { get; init; } = false;
    public Guid? SelectedAthleteId { get; init; }
    public HashSet<FitnessMetricType> SelectedMetrics { get; init; } = [];
}

/// <summary>
/// Provides chart rendering functionality for fitness metrics.
/// </summary>
public interface IChartService
{
    /// <summary>
    /// Renders the metric chart to a panel.
    /// </summary>
    void RenderChart(Panel panel, IEnumerable<Athlete> athletes, ChartConfiguration configuration);

    /// <summary>
    /// Renders a chart from a specific set of records for a single athlete.
    /// </summary>
    void RenderChartFromRecords(Panel panel, string athleteName, IEnumerable<MetricRecord> records);

    /// <summary>
    /// Gets the color associated with a metric type.
    /// </summary>
    Color GetMetricColor(FitnessMetricType metricType);

    /// <summary>
    /// Gets the color associated with an athlete.
    /// </summary>
    Color GetAthleteColor(int athleteIndex);
}

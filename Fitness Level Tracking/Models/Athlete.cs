namespace Fitness_Level_Tracking.Models;

/// <summary>
/// Represents an athlete whose fitness metrics are being tracked.
/// </summary>
public sealed class Athlete
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public required string Name { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    /// <summary>
    /// Current bodyweight in pounds (required for relative strength calculations).
    /// </summary>
    public double? BodyweightLbs { get; set; }

    /// <summary>
    /// Height in inches (required for waist-to-height ratio).
    /// </summary>
    public double? HeightInches { get; set; }

    /// <summary>
    /// Biological sex for tier thresholds (true = Male, false = Female).
    /// </summary>
    public bool? IsMale { get; set; }

    private readonly List<MetricRecord> _metricRecords = [];

    public IReadOnlyList<MetricRecord> MetricRecords => _metricRecords.AsReadOnly();

    /// <summary>
    /// Adds a new metric record to the athlete's history.
    /// </summary>
    public void AddMetricRecord(MetricRecord record)
    {
        ArgumentNullException.ThrowIfNull(record);
        _metricRecords.Add(record);
    }

    /// <summary>
    /// Removes a metric record from the athlete's history.
    /// </summary>
    public bool RemoveMetricRecord(Guid recordId)
    {
        var record = _metricRecords.Find(r => r.Id == recordId);
        return record is not null && _metricRecords.Remove(record);
    }

    /// <summary>
    /// Gets all records for a specific metric type, ordered by date.
    /// </summary>
    public IEnumerable<MetricRecord> GetRecordsForMetric(FitnessMetricType metricType)
    {
        return _metricRecords
            .Where(r => r.MetricType == metricType)
            .OrderBy(r => r.Year)
            .ThenBy(r => r.Quarter);
    }

    /// <summary>
    /// Gets all records for a specific group, ordered by date.
    /// </summary>
    public IEnumerable<MetricRecord> GetRecordsForGroup(FitnessGroup group)
    {
        return _metricRecords
            .Where(r => r.Group == group)
            .OrderBy(r => r.Year)
            .ThenBy(r => r.Quarter);
    }

    /// <summary>
    /// Gets all records for a specific quarter and year.
    /// </summary>
    public IEnumerable<MetricRecord> GetRecordsForQuarter(int quarter, int year)
    {
        return _metricRecords.Where(r => r.Quarter == quarter && r.Year == year);
    }

    /// <summary>
    /// Gets the distinct metric types that have been recorded.
    /// </summary>
    public IEnumerable<FitnessMetricType> GetTrackedMetricTypes()
    {
        return _metricRecords.Select(r => r.MetricType).Distinct();
    }

    /// <summary>
    /// Loads metric records from a collection (used during deserialization).
    /// </summary>
    public void LoadMetricRecords(IEnumerable<MetricRecord> records)
    {
        _metricRecords.Clear();
        _metricRecords.AddRange(records);
    }
}

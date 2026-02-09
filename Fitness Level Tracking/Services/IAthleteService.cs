using Fitness_Level_Tracking.Models;

namespace Fitness_Level_Tracking.Services;

/// <summary>
/// Interface for managing athlete data and operations.
/// </summary>
public interface IAthleteService
{
    /// <summary>
    /// Gets all athletes.
    /// </summary>
    IReadOnlyList<Athlete> GetAllAthletes();

    /// <summary>
    /// Gets an athlete by their unique identifier.
    /// </summary>
    Athlete? GetAthleteById(Guid id);

    /// <summary>
    /// Adds a new athlete.
    /// </summary>
    Athlete AddAthlete(string name, DateOnly? dateOfBirth = null, double? bodyweightLbs = null, 
        double? heightInches = null, bool? isMale = null);

    /// <summary>
    /// Removes an athlete by their unique identifier.
    /// </summary>
    bool RemoveAthlete(Guid id);

    /// <summary>
    /// Updates an athlete's information.
    /// </summary>
    bool UpdateAthlete(Guid id, string name, DateOnly? dateOfBirth, double? bodyweightLbs = null,
        double? heightInches = null, bool? isMale = null);

    /// <summary>
    /// Records a new metric for an athlete.
    /// </summary>
    MetricRecord RecordMetric(Guid athleteId, FitnessGroup group, FitnessMetricType metricType, 
        double value, DateOnly recordedDate, string? notes = null);

    /// <summary>
    /// Records a complete group of metrics for an athlete.
    /// </summary>
    IReadOnlyList<MetricRecord> RecordGroupMetrics(Guid athleteId, FitnessGroup group,
        Dictionary<FitnessMetricType, double> metrics, DateOnly recordedDate, string? notes = null);

    /// <summary>
    /// Removes a metric record from an athlete.
    /// </summary>
    bool RemoveMetricRecord(Guid athleteId, Guid recordId);

    /// <summary>
    /// Gets metric history for a specific athlete and metric type.
    /// </summary>
    IEnumerable<MetricRecord> GetMetricHistory(Guid athleteId, FitnessMetricType metricType);

    /// <summary>
    /// Gets all records for a specific group.
    /// </summary>
    IEnumerable<MetricRecord> GetGroupHistory(Guid athleteId, FitnessGroup group);

    /// <summary>
    /// Saves all data to persistent storage.
    /// </summary>
    Task SaveAsync();

    /// <summary>
    /// Loads all data from persistent storage.
    /// </summary>
    Task LoadAsync();

    /// <summary>
    /// Creates a test athlete with a year's worth of sample data for demonstration purposes.
    /// </summary>
    Athlete CreateTestAthlete();
}

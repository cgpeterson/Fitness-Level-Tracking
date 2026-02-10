using System.Text.Json;
using Fitness_Level_Tracking.Models;

namespace Fitness_Level_Tracking.Services;

/// <summary>
/// Manages athlete data with JSON file persistence.
/// </summary>
public sealed class AthleteService : IAthleteService
{
    private readonly List<Athlete> _athletes = [];
    private readonly string _dataFilePath;
    private readonly IMetricService _metricService;
    private readonly SemaphoreSlim _fileLock = new(1, 1);

    public AthleteService(IMetricService metricService, string? dataFilePath = null)
    {
        _metricService = metricService ?? throw new ArgumentNullException(nameof(metricService));
        _dataFilePath = dataFilePath ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "FitnessLevelTracking",
            "athletes.json");
    }

    public IReadOnlyList<Athlete> GetAllAthletes() => _athletes.AsReadOnly();

    public Athlete? GetAthleteById(Guid id) => _athletes.Find(a => a.Id == id);

    public Athlete AddAthlete(string name, DateOnly? dateOfBirth = null, double? bodyweightLbs = null,
        double? heightInches = null, bool? isMale = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var athlete = new Athlete
        {
            Name = name.Trim(),
            DateOfBirth = dateOfBirth,
            BodyweightLbs = bodyweightLbs,
            HeightInches = heightInches,
            IsMale = isMale
        };

        _athletes.Add(athlete);
        return athlete;
    }

    public bool RemoveAthlete(Guid id)
    {
        var athlete = GetAthleteById(id);
        return athlete is not null && _athletes.Remove(athlete);
    }

    public bool UpdateAthlete(Guid id, string name, DateOnly? dateOfBirth, double? bodyweightLbs = null,
        double? heightInches = null, bool? isMale = null)
    {
        var athlete = GetAthleteById(id);
        if (athlete is null)
        {
            return false;
        }

        athlete.Name = name.Trim();
        athlete.DateOfBirth = dateOfBirth;
        athlete.BodyweightLbs = bodyweightLbs;
        athlete.HeightInches = heightInches;
        athlete.IsMale = isMale;
        return true;
    }

    public MetricRecord RecordMetric(Guid athleteId, FitnessGroup group, FitnessMetricType metricType,
        double value, DateOnly recordedDate, string? notes = null)
    {
        var athlete = GetAthleteById(athleteId)
            ?? throw new ArgumentException("Athlete not found.", nameof(athleteId));

        var quarter = _metricService.GetQuarter(recordedDate);

        var record = new MetricRecord
        {
            Group = group,
            MetricType = metricType,
            Value = value,
            RecordedDate = recordedDate,
            Quarter = quarter,
            Year = recordedDate.Year,
            Notes = notes
        };

        athlete.AddMetricRecord(record);
        return record;
    }

    public IReadOnlyList<MetricRecord> RecordGroupMetrics(Guid athleteId, FitnessGroup group,
        Dictionary<FitnessMetricType, double> metrics, DateOnly recordedDate, string? notes = null)
    {
        var records = new List<MetricRecord>();

        foreach (var (metricType, value) in metrics)
        {
            var record = RecordMetric(athleteId, group, metricType, value, recordedDate, notes);
            records.Add(record);
        }

        return records.AsReadOnly();
    }

    public MetricRecord? UpdateMetricRecord(Guid athleteId, Guid recordId, double newValue, DateOnly newDate, string? notes = null)
    {
        var athlete = GetAthleteById(athleteId);
        if (athlete is null) return null;

        var existing = athlete.MetricRecords.FirstOrDefault(r => r.Id == recordId);
        if (existing is null) return null;

        athlete.RemoveMetricRecord(recordId);

        var quarter = _metricService.GetQuarter(newDate);
        var updated = new MetricRecord
        {
            Group = existing.Group,
            MetricType = existing.MetricType,
            Value = newValue,
            RecordedDate = newDate,
            Quarter = quarter,
            Year = newDate.Year,
            Notes = notes ?? existing.Notes
        };

        athlete.AddMetricRecord(updated);
        return updated;
    }

    public bool RemoveMetricRecord(Guid athleteId, Guid recordId)
    {
        var athlete = GetAthleteById(athleteId);
        return athlete?.RemoveMetricRecord(recordId) ?? false;
    }

    public IEnumerable<MetricRecord> GetMetricHistory(Guid athleteId, FitnessMetricType metricType)
    {
        var athlete = GetAthleteById(athleteId);
        return athlete?.GetRecordsForMetric(metricType) ?? [];
    }

    public IEnumerable<MetricRecord> GetGroupHistory(Guid athleteId, FitnessGroup group)
    {
        var athlete = GetAthleteById(athleteId);
        return athlete?.GetRecordsForGroup(group) ?? [];
    }

    public async Task SaveAsync()
    {
        await _fileLock.WaitAsync();
        try
        {
            var directory = Path.GetDirectoryName(_dataFilePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var data = _athletes.Select(a => new AthleteDto
            {
                Id = a.Id,
                Name = a.Name,
                DateOfBirth = a.DateOfBirth,
                BodyweightLbs = a.BodyweightLbs,
                HeightInches = a.HeightInches,
                IsMale = a.IsMale,
                MetricRecords = a.MetricRecords.ToList()
            }).ToList();

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            // Write to a temporary file first, then replace the original
            var tempFilePath = _dataFilePath + ".tmp";
            await using (var stream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                await JsonSerializer.SerializeAsync(stream, data, options);
            }

            // Atomic replace
            File.Move(tempFilePath, _dataFilePath, overwrite: true);
        }
        finally
        {
            _fileLock.Release();
        }
    }

    public async Task LoadAsync()
    {
        if (!File.Exists(_dataFilePath))
        {
            return;
        }

        await _fileLock.WaitAsync();
        try
        {
            await using var stream = new FileStream(_dataFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var data = await JsonSerializer.DeserializeAsync<List<AthleteDto>>(stream, options);

            _athletes.Clear();
            if (data is null)
            {
                return;
            }

            foreach (var dto in data)
            {
                var athlete = new Athlete
                {
                    Id = dto.Id,
                    Name = dto.Name,
                    DateOfBirth = dto.DateOfBirth,
                    BodyweightLbs = dto.BodyweightLbs,
                    HeightInches = dto.HeightInches,
                    IsMale = dto.IsMale
                };
                athlete.LoadMetricRecords(dto.MetricRecords);
                _athletes.Add(athlete);
            }
        }
        catch (JsonException)
        {
            _athletes.Clear();
        }
        finally
        {
            _fileLock.Release();
        }
    }

    /// <summary>
    /// DTO for JSON serialization of Athlete.
    /// </summary>
    private sealed class AthleteDto
    {
        public Guid Id { get; init; }
        public required string Name { get; init; }
        public DateOnly? DateOfBirth { get; init; }
        public double? BodyweightLbs { get; init; }
        public double? HeightInches { get; init; }
        public bool? IsMale { get; init; }
        public List<MetricRecord> MetricRecords { get; init; } = [];
    }

    public Athlete CreateTestAthlete()
    {
        var athlete = AddAthlete(
            "Test Athlete",
            DateOnly.FromDateTime(DateTime.Today.AddYears(-28)),
            bodyweightLbs: 185,
            heightInches: 70,
            isMale: true);

        var random = new Random(42); // Fixed seed for reproducible data
        var today = DateOnly.FromDateTime(DateTime.Today);

        // Create quarterly data for the past year (4 quarters)
        for (var quarterOffset = 0; quarterOffset < 4; quarterOffset++)
        {
            var recordDate = today.AddMonths(-3 * quarterOffset);

            // Group 1: Metabolic & Morphological
            RecordMetric(athlete.Id, FitnessGroup.MetabolicMorphological, FitnessMetricType.RestingHeartRate,
                60 + random.Next(-5, 10) - quarterOffset, recordDate); // Improving over time
            RecordMetric(athlete.Id, FitnessGroup.MetabolicMorphological, FitnessMetricType.WaistToHeightRatio,
                0.45 + random.NextDouble() * 0.05 + quarterOffset * 0.01, recordDate);
            RecordMetric(athlete.Id, FitnessGroup.MetabolicMorphological, FitnessMetricType.TwelveMinuteRun,
                1.6 + random.NextDouble() * 0.2 - quarterOffset * 0.05, recordDate);
            RecordMetric(athlete.Id, FitnessGroup.MetabolicMorphological, FitnessMetricType.HeartRateRecovery,
                45 + random.Next(-5, 10) - quarterOffset * 2, recordDate);

            // Group 2: Neuromuscular & Structural
            RecordMetric(athlete.Id, FitnessGroup.NeuromuscularStructural, FitnessMetricType.DeadliftFiveRepMax,
                315 + random.Next(-20, 30) - quarterOffset * 10, recordDate);
            RecordMetric(athlete.Id, FitnessGroup.NeuromuscularStructural, FitnessMetricType.NeutralPressRepMax,
                120 + random.Next(-10, 15) - quarterOffset * 5, recordDate);
            RecordMetric(athlete.Id, FitnessGroup.NeuromuscularStructural, FitnessMetricType.MaxPushUps,
                35 + random.Next(-5, 8) - quarterOffset * 2, recordDate);
            RecordMetric(athlete.Id, FitnessGroup.NeuromuscularStructural, FitnessMetricType.DeadHangTime,
                75 + random.Next(-10, 15) - quarterOffset * 5, recordDate);

            // Group 3: Functional & Dynamic
            RecordMetric(athlete.Id, FitnessGroup.FunctionalDynamic, FitnessMetricType.ShoeAndSockBalance,
                random.NextDouble() > 0.3 ? 1 : 0, recordDate);
            RecordMetric(athlete.Id, FitnessGroup.FunctionalDynamic, FitnessMetricType.DeepSquatHold,
                random.NextDouble() > 0.5 ? 2 : 1, recordDate);
            RecordMetric(athlete.Id, FitnessGroup.FunctionalDynamic, FitnessMetricType.FarmerCarryDistance,
                80 + random.Next(-10, 20) - quarterOffset * 5, recordDate);
            RecordMetric(athlete.Id, FitnessGroup.FunctionalDynamic, FitnessMetricType.SittingRisingTest,
                8 + random.Next(-1, 2), recordDate);
        }

        return athlete;
    }
}

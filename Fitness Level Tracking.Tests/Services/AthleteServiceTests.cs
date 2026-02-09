using Fitness_Level_Tracking.Models;
using Fitness_Level_Tracking.Services;

namespace Fitness_Level_Tracking_Tests.Services;

public class AthleteServiceTests : IDisposable
{
    private readonly AthleteService _sut;
    private readonly MetricService _metricService;
    private readonly string _testDataFile;

    public AthleteServiceTests()
    {
        _metricService = new MetricService();
        _testDataFile = Path.Combine(Path.GetTempPath(), $"test_athletes_{Guid.NewGuid()}.json");
        _sut = new AthleteService(_metricService, _testDataFile);
    }

    public void Dispose()
    {
        if (File.Exists(_testDataFile))
        {
            File.Delete(_testDataFile);
        }
        GC.SuppressFinalize(this);
    }

    [Fact]
    public void GetAllAthletes_Initially_ShouldReturnEmptyList()
    {
        // Act
        var result = _sut.GetAllAthletes();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void AddAthlete_ShouldCreateAndReturnNewAthlete()
    {
        // Act
        var athlete = _sut.AddAthlete("John Doe", bodyweightLbs: 180, heightInches: 70, isMale: true);

        // Assert
        Assert.NotNull(athlete);
        Assert.Equal("John Doe", athlete.Name);
        Assert.Equal(180, athlete.BodyweightLbs);
        Assert.Equal(70, athlete.HeightInches);
        Assert.True(athlete.IsMale);
        Assert.NotEqual(Guid.Empty, athlete.Id);
        Assert.Single(_sut.GetAllAthletes());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void AddAthlete_WithEmptyOrWhitespaceName_ShouldThrowArgumentException(string invalidName)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => _sut.AddAthlete(invalidName));
    }

    [Fact]
    public void AddAthlete_WithNullName_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _sut.AddAthlete(null!));
    }

    [Fact]
    public void GetAthleteById_WithValidId_ShouldReturnAthlete()
    {
        // Arrange
        var athlete = _sut.AddAthlete("Test Athlete");

        // Act
        var result = _sut.GetAthleteById(athlete.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(athlete.Id, result.Id);
    }

    [Fact]
    public void GetAthleteById_WithInvalidId_ShouldReturnNull()
    {
        // Act
        var result = _sut.GetAthleteById(Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void RemoveAthlete_WithValidId_ShouldRemoveAndReturnTrue()
    {
        // Arrange
        var athlete = _sut.AddAthlete("To Be Removed");

        // Act
        var result = _sut.RemoveAthlete(athlete.Id);

        // Assert
        Assert.True(result);
        Assert.Empty(_sut.GetAllAthletes());
    }

    [Fact]
    public void UpdateAthlete_WithValidId_ShouldUpdateAndReturnTrue()
    {
        // Arrange
        var athlete = _sut.AddAthlete("Original Name");
        var newDob = new DateOnly(1995, 3, 20);

        // Act
        var result = _sut.UpdateAthlete(athlete.Id, "Updated Name", null, 200, 72, false);

        // Assert
        Assert.True(result);
        var updated = _sut.GetAthleteById(athlete.Id);
        Assert.Equal("Updated Name", updated!.Name);
        Assert.Equal(200, updated.BodyweightLbs);
        Assert.Equal(72, updated.HeightInches);
        Assert.False(updated.IsMale);
    }

    [Fact]
    public void RecordMetric_ShouldAddRecordToAthlete()
    {
        // Arrange
        var athlete = _sut.AddAthlete("Test Athlete");
        var date = new DateOnly(2024, 4, 15);

        // Act
        var record = _sut.RecordMetric(
            athlete.Id,
            FitnessGroup.MetabolicMorphological,
            FitnessMetricType.RestingHeartRate,
            55,
            date,
            "Good morning measurement");

        // Assert
        Assert.NotNull(record);
        Assert.Equal(FitnessMetricType.RestingHeartRate, record.MetricType);
        Assert.Equal(FitnessGroup.MetabolicMorphological, record.Group);
        Assert.Equal(55, record.Value);
        Assert.Equal(2, record.Quarter);
        Assert.Equal(2024, record.Year);
        Assert.Single(athlete.MetricRecords);
    }

    [Fact]
    public void RecordGroupMetrics_ShouldAddAllMetricsForGroup()
    {
        // Arrange
        var athlete = _sut.AddAthlete("Test Athlete");
        var date = new DateOnly(2024, 1, 15);

        var metrics = new Dictionary<FitnessMetricType, double>
        {
            [FitnessMetricType.RestingHeartRate] = 55,
            [FitnessMetricType.WaistToHeightRatio] = 0.48,
            [FitnessMetricType.TwelveMinuteRun] = 1.7,
            [FitnessMetricType.HeartRateRecovery] = 35
        };

        // Act
        var records = _sut.RecordGroupMetrics(
            athlete.Id,
            FitnessGroup.MetabolicMorphological,
            metrics,
            date);

        // Assert
        Assert.Equal(4, records.Count);
        Assert.All(records, r => Assert.Equal(FitnessGroup.MetabolicMorphological, r.Group));
        Assert.Equal(4, athlete.MetricRecords.Count);
    }

    [Fact]
    public void GetGroupHistory_ShouldReturnRecordsForGroup()
    {
        // Arrange
        var athlete = _sut.AddAthlete("Test Athlete");

        _sut.RecordMetric(athlete.Id, FitnessGroup.MetabolicMorphological,
            FitnessMetricType.RestingHeartRate, 55, new DateOnly(2024, 1, 15));
        _sut.RecordMetric(athlete.Id, FitnessGroup.NeuromuscularStructural,
            FitnessMetricType.MaxPushUps, 40, new DateOnly(2024, 1, 15));
        _sut.RecordMetric(athlete.Id, FitnessGroup.MetabolicMorphological,
            FitnessMetricType.TwelveMinuteRun, 1.6, new DateOnly(2024, 1, 15));

        // Act
        var history = _sut.GetGroupHistory(athlete.Id, FitnessGroup.MetabolicMorphological).ToList();

        // Assert
        Assert.Equal(2, history.Count);
        Assert.All(history, r => Assert.Equal(FitnessGroup.MetabolicMorphological, r.Group));
    }

    [Fact]
    public async Task SaveAsync_And_LoadAsync_ShouldPersistData()
    {
        // Arrange
        var athlete = _sut.AddAthlete("Persistent Athlete", null, 185, 71, true);

        _sut.RecordMetric(athlete.Id, FitnessGroup.MetabolicMorphological,
            FitnessMetricType.RestingHeartRate, 52, new DateOnly(2024, 1, 15));
        _sut.RecordMetric(athlete.Id, FitnessGroup.NeuromuscularStructural,
            FitnessMetricType.DeadliftFiveRepMax, 315, new DateOnly(2024, 1, 15));

        // Act - Save
        await _sut.SaveAsync();
        Assert.True(File.Exists(_testDataFile));

        // Create new service instance and load
        var newService = new AthleteService(_metricService, _testDataFile);
        await newService.LoadAsync();

        // Assert
        var loadedAthletes = newService.GetAllAthletes();
        Assert.Single(loadedAthletes);

        var loadedAthlete = loadedAthletes[0];
        Assert.Equal("Persistent Athlete", loadedAthlete.Name);
        Assert.Equal(185, loadedAthlete.BodyweightLbs);
        Assert.Equal(71, loadedAthlete.HeightInches);
        Assert.True(loadedAthlete.IsMale);
        Assert.Equal(2, loadedAthlete.MetricRecords.Count);
    }

    [Fact]
    public async Task LoadAsync_WithNonExistentFile_ShouldNotThrow()
    {
        // Arrange
        var nonExistentPath = Path.Combine(Path.GetTempPath(), $"nonexistent_{Guid.NewGuid()}.json");
        var service = new AthleteService(_metricService, nonExistentPath);

        // Act & Assert - Should not throw
        await service.LoadAsync();
        Assert.Empty(service.GetAllAthletes());
    }
}

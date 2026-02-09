using Fitness_Level_Tracking.Models;

namespace Fitness_Level_Tracking_Tests.Models;

public class AthleteTests
{
    [Fact]
    public void Constructor_ShouldInitializeWithEmptyMetricRecords()
    {
        // Arrange & Act
        var athlete = new Athlete { Name = "Test Athlete" };

        // Assert
        Assert.Empty(athlete.MetricRecords);
        Assert.NotEqual(Guid.Empty, athlete.Id);
        Assert.Equal("Test Athlete", athlete.Name);
    }

    [Fact]
    public void AddMetricRecord_ShouldAddRecordToCollection()
    {
        // Arrange
        var athlete = new Athlete { Name = "Test Athlete" };
        var record = new MetricRecord
        {
            Group = FitnessGroup.NeuromuscularStructural,
            MetricType = FitnessMetricType.DeadliftFiveRepMax,
            Value = 315,
            RecordedDate = new DateOnly(2024, 3, 15),
            Quarter = 1,
            Year = 2024
        };

        // Act
        athlete.AddMetricRecord(record);

        // Assert
        Assert.Single(athlete.MetricRecords);
        Assert.Contains(record, athlete.MetricRecords);
    }

    [Fact]
    public void AddMetricRecord_WithNullRecord_ShouldThrowArgumentNullException()
    {
        // Arrange
        var athlete = new Athlete { Name = "Test Athlete" };

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => athlete.AddMetricRecord(null!));
    }

    [Fact]
    public void RemoveMetricRecord_WithValidId_ShouldRemoveAndReturnTrue()
    {
        // Arrange
        var athlete = new Athlete { Name = "Test Athlete" };
        var record = new MetricRecord
        {
            Group = FitnessGroup.NeuromuscularStructural,
            MetricType = FitnessMetricType.MaxPushUps,
            Value = 40,
            RecordedDate = new DateOnly(2024, 3, 15),
            Quarter = 1,
            Year = 2024
        };
        athlete.AddMetricRecord(record);

        // Act
        var result = athlete.RemoveMetricRecord(record.Id);

        // Assert
        Assert.True(result);
        Assert.Empty(athlete.MetricRecords);
    }

    [Fact]
    public void RemoveMetricRecord_WithInvalidId_ShouldReturnFalse()
    {
        // Arrange
        var athlete = new Athlete { Name = "Test Athlete" };

        // Act
        var result = athlete.RemoveMetricRecord(Guid.NewGuid());

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GetRecordsForMetric_ShouldReturnFilteredAndOrderedRecords()
    {
        // Arrange
        var athlete = new Athlete { Name = "Test Athlete" };

        athlete.AddMetricRecord(new MetricRecord
        {
            Group = FitnessGroup.MetabolicMorphological,
            MetricType = FitnessMetricType.RestingHeartRate,
            Value = 55,
            RecordedDate = new DateOnly(2024, 6, 15),
            Quarter = 2,
            Year = 2024
        });

        athlete.AddMetricRecord(new MetricRecord
        {
            Group = FitnessGroup.MetabolicMorphological,
            MetricType = FitnessMetricType.RestingHeartRate,
            Value = 60,
            RecordedDate = new DateOnly(2024, 1, 15),
            Quarter = 1,
            Year = 2024
        });

        athlete.AddMetricRecord(new MetricRecord
        {
            Group = FitnessGroup.NeuromuscularStructural,
            MetricType = FitnessMetricType.MaxPushUps,
            Value = 35,
            RecordedDate = new DateOnly(2024, 3, 15),
            Quarter = 1,
            Year = 2024
        });

        // Act
        var records = athlete.GetRecordsForMetric(FitnessMetricType.RestingHeartRate).ToList();

        // Assert
        Assert.Equal(2, records.Count);
        Assert.Equal(60, records[0].Value); // Q1 should come first
        Assert.Equal(55, records[1].Value); // Q2 should come second
    }

    [Fact]
    public void GetRecordsForGroup_ShouldReturnMatchingRecords()
    {
        // Arrange
        var athlete = new Athlete { Name = "Test Athlete" };

        athlete.AddMetricRecord(new MetricRecord
        {
            Group = FitnessGroup.MetabolicMorphological,
            MetricType = FitnessMetricType.RestingHeartRate,
            Value = 55,
            RecordedDate = new DateOnly(2024, 4, 15),
            Quarter = 2,
            Year = 2024
        });

        athlete.AddMetricRecord(new MetricRecord
        {
            Group = FitnessGroup.MetabolicMorphological,
            MetricType = FitnessMetricType.TwelveMinuteRun,
            Value = 1.6,
            RecordedDate = new DateOnly(2024, 5, 15),
            Quarter = 2,
            Year = 2024
        });

        athlete.AddMetricRecord(new MetricRecord
        {
            Group = FitnessGroup.NeuromuscularStructural,
            MetricType = FitnessMetricType.DeadliftFiveRepMax,
            Value = 315,
            RecordedDate = new DateOnly(2024, 1, 15),
            Quarter = 1,
            Year = 2024
        });

        // Act
        var records = athlete.GetRecordsForGroup(FitnessGroup.MetabolicMorphological).ToList();

        // Assert
        Assert.Equal(2, records.Count);
        Assert.All(records, r => Assert.Equal(FitnessGroup.MetabolicMorphological, r.Group));
    }

    [Fact]
    public void GetTrackedMetricTypes_ShouldReturnDistinctTypes()
    {
        // Arrange
        var athlete = new Athlete { Name = "Test Athlete" };

        athlete.AddMetricRecord(new MetricRecord
        {
            Group = FitnessGroup.MetabolicMorphological,
            MetricType = FitnessMetricType.RestingHeartRate,
            Value = 55,
            RecordedDate = new DateOnly(2024, 1, 15),
            Quarter = 1,
            Year = 2024
        });

        athlete.AddMetricRecord(new MetricRecord
        {
            Group = FitnessGroup.MetabolicMorphological,
            MetricType = FitnessMetricType.RestingHeartRate,
            Value = 52,
            RecordedDate = new DateOnly(2024, 4, 15),
            Quarter = 2,
            Year = 2024
        });

        athlete.AddMetricRecord(new MetricRecord
        {
            Group = FitnessGroup.NeuromuscularStructural,
            MetricType = FitnessMetricType.MaxPushUps,
            Value = 40,
            RecordedDate = new DateOnly(2024, 1, 15),
            Quarter = 1,
            Year = 2024
        });

        // Act
        var types = athlete.GetTrackedMetricTypes().ToList();

        // Assert
        Assert.Equal(2, types.Count);
        Assert.Contains(FitnessMetricType.RestingHeartRate, types);
        Assert.Contains(FitnessMetricType.MaxPushUps, types);
    }

    [Fact]
    public void LoadMetricRecords_ShouldReplaceExistingRecords()
    {
        // Arrange
        var athlete = new Athlete { Name = "Test Athlete" };
        athlete.AddMetricRecord(new MetricRecord
        {
            Group = FitnessGroup.MetabolicMorphological,
            MetricType = FitnessMetricType.RestingHeartRate,
            Value = 55,
            RecordedDate = new DateOnly(2024, 1, 15),
            Quarter = 1,
            Year = 2024
        });

        var newRecords = new List<MetricRecord>
        {
            new()
            {
                Group = FitnessGroup.NeuromuscularStructural,
                MetricType = FitnessMetricType.MaxPushUps,
                Value = 40,
                RecordedDate = new DateOnly(2024, 3, 15),
                Quarter = 1,
                Year = 2024
            },
            new()
            {
                Group = FitnessGroup.NeuromuscularStructural,
                MetricType = FitnessMetricType.DeadHangTime,
                Value = 90,
                RecordedDate = new DateOnly(2024, 3, 15),
                Quarter = 1,
                Year = 2024
            }
        };

        // Act
        athlete.LoadMetricRecords(newRecords);

        // Assert
        Assert.Equal(2, athlete.MetricRecords.Count);
        Assert.DoesNotContain(athlete.MetricRecords, r => r.MetricType == FitnessMetricType.RestingHeartRate);
    }

    [Fact]
    public void AthleteProperties_ShouldStoreBodyweightAndHeight()
    {
        // Arrange
        var athlete = new Athlete
        {
            Name = "Test Athlete",
            BodyweightLbs = 180,
            HeightInches = 70,
            IsMale = true
        };

        // Assert
        Assert.Equal(180, athlete.BodyweightLbs);
        Assert.Equal(70, athlete.HeightInches);
        Assert.True(athlete.IsMale);
    }
}

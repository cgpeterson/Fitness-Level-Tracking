using Fitness_Level_Tracking.Models;

namespace Fitness_Level_Tracking_Tests.Models;

public class MetricRecordTests
{
    [Fact]
    public void Constructor_ShouldGenerateUniqueId()
    {
        var record1 = new MetricRecord
        {
            Group = FitnessGroup.MetabolicMorphological,
            MetricType = FitnessMetricType.RestingHeartRate,
            Value = 55,
            RecordedDate = new DateOnly(2024, 3, 15),
            Quarter = 1,
            Year = 2024
        };

        var record2 = new MetricRecord
        {
            Group = FitnessGroup.MetabolicMorphological,
            MetricType = FitnessMetricType.RestingHeartRate,
            Value = 55,
            RecordedDate = new DateOnly(2024, 3, 15),
            Quarter = 1,
            Year = 2024
        };

        Assert.NotEqual(Guid.Empty, record1.Id);
        Assert.NotEqual(Guid.Empty, record2.Id);
        Assert.NotEqual(record1.Id, record2.Id);
    }

    [Theory]
    [InlineData(1, 2024, "Q1 2024")]
    [InlineData(2, 2024, "Q2 2024")]
    [InlineData(3, 2024, "Q3 2024")]
    [InlineData(4, 2024, "Q4 2024")]
    [InlineData(1, 2025, "Q1 2025")]
    public void QuarterLabel_ShouldFormatCorrectly(int quarter, int year, string expected)
    {
        var record = new MetricRecord
        {
            Group = FitnessGroup.MetabolicMorphological,
            MetricType = FitnessMetricType.TwelveMinuteRun,
            Value = 1.6,
            RecordedDate = new DateOnly(year, quarter * 3, 15),
            Quarter = quarter,
            Year = year
        };

        Assert.Equal(expected, record.QuarterLabel);
    }

    [Fact]
    public void Notes_ShouldBeOptional()
    {
        var recordWithNotes = new MetricRecord
        {
            Group = FitnessGroup.MetabolicMorphological,
            MetricType = FitnessMetricType.TwelveMinuteRun,
            Value = 1.8,
            RecordedDate = new DateOnly(2024, 6, 15),
            Quarter = 2,
            Year = 2024,
            Notes = "Personal best!"
        };

        var recordWithoutNotes = new MetricRecord
        {
            Group = FitnessGroup.MetabolicMorphological,
            MetricType = FitnessMetricType.TwelveMinuteRun,
            Value = 1.6,
            RecordedDate = new DateOnly(2024, 6, 15),
            Quarter = 2,
            Year = 2024
        };

        Assert.Equal("Personal best!", recordWithNotes.Notes);
        Assert.Null(recordWithoutNotes.Notes);
    }

    [Fact]
    public void Group_ShouldBeStoredCorrectly()
    {
        var record = new MetricRecord
        {
            Group = FitnessGroup.FunctionalDynamic,
            MetricType = FitnessMetricType.SittingRisingTest,
            Value = 10,
            RecordedDate = new DateOnly(2024, 3, 15),
            Quarter = 1,
            Year = 2024
        };

        Assert.Equal(FitnessGroup.FunctionalDynamic, record.Group);
    }
}

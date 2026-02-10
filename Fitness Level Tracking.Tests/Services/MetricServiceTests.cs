using Fitness_Level_Tracking.Models;
using Fitness_Level_Tracking.Services;

namespace Fitness_Level_Tracking_Tests.Services;

public class MetricServiceTests
{
    private readonly MetricService _sut = new();

    [Theory]
    [InlineData(FitnessMetricType.RestingHeartRate, "Resting Heart Rate")]
    [InlineData(FitnessMetricType.WaistToHeightRatio, "Waist:Height Ratio")]
    [InlineData(FitnessMetricType.TwelveMinuteRun, "12-Minute Run")]
    [InlineData(FitnessMetricType.HeartRateRecovery, "HR Recovery (1 min)")]
    [InlineData(FitnessMetricType.DeadliftFiveRepMax, "Deadlift 5RM")]
    [InlineData(FitnessMetricType.NeutralPressRepMax, "Neutral Press 5RM")]
    [InlineData(FitnessMetricType.MaxPushUps, "Max Push-Ups")]
    [InlineData(FitnessMetricType.DeadHangTime, "Dead Hang")]
    [InlineData(FitnessMetricType.ShoeAndSockBalance, "Shoe & Sock Balance")]
    [InlineData(FitnessMetricType.DeepSquatHold, "Deep Squat Hold")]
    [InlineData(FitnessMetricType.FarmerCarryDistance, "Farmer's Carry")]
    [InlineData(FitnessMetricType.SittingRisingTest, "Sitting-Rising Test")]
    public void GetMetricDisplayName_ShouldReturnCorrectName(FitnessMetricType metricType, string expected)
    {
        // Act
        var result = _sut.GetMetricDisplayName(metricType);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(FitnessMetricType.RestingHeartRate, "bpm")]
    [InlineData(FitnessMetricType.WaistToHeightRatio, "ratio")]
    [InlineData(FitnessMetricType.TwelveMinuteRun, "miles")]
    [InlineData(FitnessMetricType.DeadliftFiveRepMax, "lbs")]
    [InlineData(FitnessMetricType.MaxPushUps, "reps")]
    [InlineData(FitnessMetricType.DeadHangTime, "sec")]
    [InlineData(FitnessMetricType.SittingRisingTest, "points")]
    public void GetMetricUnit_ShouldReturnCorrectUnit(FitnessMetricType metricType, string expected)
    {
        // Act
        var result = _sut.GetMetricUnit(metricType);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(FitnessMetricType.RestingHeartRate, true)]
    [InlineData(FitnessMetricType.WaistToHeightRatio, true)]
    [InlineData(FitnessMetricType.TwelveMinuteRun, false)]
    [InlineData(FitnessMetricType.HeartRateRecovery, false)]
    [InlineData(FitnessMetricType.DeadliftFiveRepMax, false)]
    [InlineData(FitnessMetricType.MaxPushUps, false)]
    [InlineData(FitnessMetricType.DeadHangTime, false)]
    public void IsLowerBetter_ShouldReturnCorrectValue(FitnessMetricType metricType, bool expected)
    {
        // Act
        var result = _sut.IsLowerBetter(metricType);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(3, 1)]
    [InlineData(4, 2)]
    [InlineData(6, 2)]
    [InlineData(7, 3)]
    [InlineData(9, 3)]
    [InlineData(10, 4)]
    [InlineData(12, 4)]
    public void GetQuarter_ShouldReturnCorrectQuarter(int month, int expectedQuarter)
    {
        // Arrange
        var date = new DateOnly(2024, month, 15);

        // Act
        var result = _sut.GetQuarter(date);

        // Assert
        Assert.Equal(expectedQuarter, result);
    }

    [Fact]
    public void GetAllMetricTypes_ShouldReturnAllEnumValues()
    {
        // Arrange
        var expectedCount = Enum.GetValues<FitnessMetricType>().Length;

        // Act
        var result = _sut.GetAllMetricTypes();

        // Assert
        Assert.Equal(expectedCount, result.Count);
    }

    [Fact]
    public void GetAllGroups_ShouldReturnThreeGroups()
    {
        // Act
        var result = _sut.GetAllGroups();

        // Assert
        Assert.Equal(3, result.Count);
    }

    [Theory]
    [InlineData(FitnessGroup.MetabolicMorphological, 4)]
    [InlineData(FitnessGroup.NeuromuscularStructural, 4)]
    [InlineData(FitnessGroup.FunctionalDynamic, 4)]
    public void GetMetricsForGroup_ShouldReturnCorrectCount(FitnessGroup group, int expectedCount)
    {
        // Act
        var result = _sut.GetMetricsForGroup(group);

        // Assert
        Assert.Equal(expectedCount, result.Count);
    }

    [Theory]
    [InlineData(45, PerformanceTier.Peak)]
    [InlineData(55, PerformanceTier.Good)]
    [InlineData(72, PerformanceTier.Average)]
    [InlineData(80, PerformanceTier.BelowAverage)]
    public void EvaluateTier_RestingHeartRate_ShouldReturnCorrectTier(double value, PerformanceTier expected)
    {
        // Act
        var result = _sut.EvaluateTier(FitnessMetricType.RestingHeartRate, value);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(0.42, PerformanceTier.Peak)]
    [InlineData(0.48, PerformanceTier.Good)]
    [InlineData(0.52, PerformanceTier.Average)]
    [InlineData(0.60, PerformanceTier.BelowAverage)]
    public void EvaluateTier_WaistToHeight_ShouldReturnCorrectTier(double value, PerformanceTier expected)
    {
        // Act
        var result = _sut.EvaluateTier(FitnessMetricType.WaistToHeightRatio, value);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(2.0, PerformanceTier.Peak)]
    [InlineData(1.6, PerformanceTier.Good)]
    [InlineData(1.35, PerformanceTier.Average)]
    [InlineData(1.1, PerformanceTier.BelowAverage)]
    public void EvaluateTier_TwelveMinuteRun_ShouldReturnCorrectTier(double value, PerformanceTier expected)
    {
        // Act
        var result = _sut.EvaluateTier(FitnessMetricType.TwelveMinuteRun, value);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(80, true, PerformanceTier.Peak)]
    [InlineData(50, true, PerformanceTier.Good)]
    [InlineData(30, true, PerformanceTier.Average)]
    [InlineData(10, true, PerformanceTier.BelowAverage)]
    public void EvaluateTier_MaxPushUps_Male_ShouldReturnCorrectTier(double value, bool isMale, PerformanceTier expected)
    {
        // Act
        var result = _sut.EvaluateTier(FitnessMetricType.MaxPushUps, value, 180, isMale);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(10, PerformanceTier.Peak)]
    [InlineData(8, PerformanceTier.Good)]
    [InlineData(6, PerformanceTier.Average)]
    [InlineData(4, PerformanceTier.BelowAverage)]
    public void EvaluateTier_SittingRisingTest_ShouldReturnCorrectTier(double value, PerformanceTier expected)
    {
        // Act
        var result = _sut.EvaluateTier(FitnessMetricType.SittingRisingTest, value);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(55, false, PerformanceTier.Peak)]
    [InlineData(30, false, PerformanceTier.Good)]
    [InlineData(15, false, PerformanceTier.Average)]
    [InlineData(5, false, PerformanceTier.BelowAverage)]
    public void EvaluateTier_MaxPushUps_Female_ShouldReturnCorrectTier(double value, bool isMale, PerformanceTier expected)
    {
        // Act
        var result = _sut.EvaluateTier(FitnessMetricType.MaxPushUps, value, 140, isMale);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(200, true, PerformanceTier.Peak)]
    [InlineData(100, true, PerformanceTier.Good)]
    [InlineData(45, true, PerformanceTier.Average)]
    [InlineData(15, true, PerformanceTier.BelowAverage)]
    public void EvaluateTier_DeadHang_Male_ShouldReturnCorrectTier(double value, bool isMale, PerformanceTier expected)
    {
        // Act
        var result = _sut.EvaluateTier(FitnessMetricType.DeadHangTime, value, 180, isMale);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(130, false, PerformanceTier.Peak)]
    [InlineData(60, false, PerformanceTier.Good)]
    [InlineData(20, false, PerformanceTier.Average)]
    [InlineData(10, false, PerformanceTier.BelowAverage)]
    public void EvaluateTier_DeadHang_Female_ShouldReturnCorrectTier(double value, bool isMale, PerformanceTier expected)
    {
        // Act
        var result = _sut.EvaluateTier(FitnessMetricType.DeadHangTime, value, 140, isMale);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(2, PerformanceTier.Peak)]
    [InlineData(1, PerformanceTier.Good)]
    [InlineData(0, PerformanceTier.Average)]
    public void EvaluateTier_ShoeAndSockBalance_ShouldReturnCorrectTier(double value, PerformanceTier expected)
    {
        // Act
        var result = _sut.EvaluateTier(FitnessMetricType.ShoeAndSockBalance, value);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void CalculateImprovement_HigherIsBetter_ShouldCalculatePositiveImprovement()
    {
        // Act
        var result = _sut.CalculateImprovement(FitnessMetricType.MaxPushUps, 20, 30);

        // Assert
        Assert.Equal(50, result, 1);
    }

    [Fact]
    public void CalculateImprovement_LowerIsBetter_ShouldCalculatePositiveImprovement()
    {
        // Act
        var result = _sut.CalculateImprovement(FitnessMetricType.RestingHeartRate, 70, 60);

        // Assert
        Assert.True(result > 0);
    }
}

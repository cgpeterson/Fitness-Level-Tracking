using Fitness_Level_Tracking.Models;
using Fitness_Level_Tracking.Services;

namespace Fitness_Level_Tracking_Tests.Integration;

/// <summary>
/// End-to-end integration tests verifying the complete Integrated Fitness Protocol workflow.
/// </summary>
public class EndToEndTests : IDisposable
{
    private readonly string _testDataFile;
    private readonly MetricService _metricService;

    public EndToEndTests()
    {
        _testDataFile = Path.Combine(Path.GetTempPath(), $"e2e_test_{Guid.NewGuid()}.json");
        _metricService = new MetricService();
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
    public async Task CompleteWorkflow_RecordAllThreeGroupsAndTrackProgress()
    {
        var athleteService = new AthleteService(_metricService, _testDataFile);

        // Add a new athlete with body metrics
        var athlete = athleteService.AddAthlete("John Smith", null, 180, 70, true);
        Assert.NotNull(athlete);

        // Q1 2024: Record Group 1 (Metabolic & Morphological)
        var q1Group1 = new Dictionary<FitnessMetricType, double>
        {
            [FitnessMetricType.RestingHeartRate] = 68,
            [FitnessMetricType.WaistToHeightRatio] = 0.52,
            [FitnessMetricType.TwelveMinuteRun] = 1.4,
            [FitnessMetricType.HeartRateRecovery] = 25
        };
        athleteService.RecordGroupMetrics(athlete.Id, FitnessGroup.MetabolicMorphological,
            q1Group1, new DateOnly(2024, 1, 15));

        // Q1 2024: Record Group 2 (Neuromuscular & Structural)
        var q1Group2 = new Dictionary<FitnessMetricType, double>
        {
            [FitnessMetricType.DeadliftFiveRepMax] = 225, // ~1.44x BW
            [FitnessMetricType.NeutralPressRepMax] = 80,
            [FitnessMetricType.MaxPushUps] = 25,
            [FitnessMetricType.DeadHangTime] = 45
        };
        athleteService.RecordGroupMetrics(athlete.Id, FitnessGroup.NeuromuscularStructural,
            q1Group2, new DateOnly(2024, 1, 16));

        // Q1 2024: Record Group 3 (Functional & Dynamic)
        var q1Group3 = new Dictionary<FitnessMetricType, double>
        {
            [FitnessMetricType.ShoeAndSockBalance] = 0, // Fail
            [FitnessMetricType.DeepSquatHold] = 0, // Heels up
            [FitnessMetricType.FarmerCarryDistance] = 15,
            [FitnessMetricType.SittingRisingTest] = 6
        };
        athleteService.RecordGroupMetrics(athlete.Id, FitnessGroup.FunctionalDynamic,
            q1Group3, new DateOnly(2024, 1, 17));

        Assert.Equal(12, athlete.MetricRecords.Count);

        // Q2 2024: Record improved results
        var q2Group1 = new Dictionary<FitnessMetricType, double>
        {
            [FitnessMetricType.RestingHeartRate] = 58, // Improved
            [FitnessMetricType.WaistToHeightRatio] = 0.48, // Improved to Good
            [FitnessMetricType.TwelveMinuteRun] = 1.65, // Improved to Good
            [FitnessMetricType.HeartRateRecovery] = 38 // Improved to Good
        };
        athleteService.RecordGroupMetrics(athlete.Id, FitnessGroup.MetabolicMorphological,
            q2Group1, new DateOnly(2024, 4, 15));

        Assert.Equal(16, athlete.MetricRecords.Count);

        // Verify tier improvements
        var rhrQ1 = _metricService.EvaluateTier(FitnessMetricType.RestingHeartRate, 68);
        var rhrQ2 = _metricService.EvaluateTier(FitnessMetricType.RestingHeartRate, 58);
        Assert.Equal(PerformanceTier.Average, rhrQ1);
        Assert.Equal(PerformanceTier.Good, rhrQ2);

        // Save and reload
        await athleteService.SaveAsync();

        var newService = new AthleteService(_metricService, _testDataFile);
        await newService.LoadAsync();

        var loadedAthlete = newService.GetAthleteById(athlete.Id);
        Assert.NotNull(loadedAthlete);
        Assert.Equal(16, loadedAthlete.MetricRecords.Count);
    }

    [Fact]
    public async Task MultiAthleteComparison_CompareProgressBetweenAthletes()
    {
        var athleteService = new AthleteService(_metricService, _testDataFile);

        var athlete1 = athleteService.AddAthlete("Athlete One", null, 180, 70, true);
        var athlete2 = athleteService.AddAthlete("Athlete Two", null, 160, 65, false);

        var testDate = new DateOnly(2024, 1, 15);

        // Record Group 2 for both athletes
        athleteService.RecordMetric(athlete1.Id, FitnessGroup.NeuromuscularStructural,
            FitnessMetricType.MaxPushUps, 45, testDate);
        athleteService.RecordMetric(athlete2.Id, FitnessGroup.NeuromuscularStructural,
            FitnessMetricType.MaxPushUps, 30, testDate);

        // Evaluate tiers (different thresholds for male/female)
        var athlete1Tier = _metricService.EvaluateTier(
            FitnessMetricType.MaxPushUps, 45, 180, true);
        var athlete2Tier = _metricService.EvaluateTier(
            FitnessMetricType.MaxPushUps, 30, 160, false);

        Assert.Equal(PerformanceTier.Good, athlete1Tier); // Male: 40 is Good
        Assert.Equal(PerformanceTier.Good, athlete2Tier); // Female: 25 is Good

        await athleteService.SaveAsync();

        var newService = new AthleteService(_metricService, _testDataFile);
        await newService.LoadAsync();

        Assert.Equal(2, newService.GetAllAthletes().Count);
    }

    [Fact]
    public void TierEvaluation_RelativeStrength_ShouldUseBodyweight()
    {
        // Athlete A: 180 lbs, Deadlift 5RM 315 lbs
        // Est 1RM = 315 * 1.15 = 362.25 -> 362.25/180 = 2.01x BW = Good
        var tierA = _metricService.EvaluateTier(
            FitnessMetricType.DeadliftFiveRepMax, 315, 180, true);

        // Athlete B: 150 lbs, Deadlift 5RM 315 lbs
        // Est 1RM = 315 * 1.15 = 362.25 -> 362.25/150 = 2.42x BW = Good (near Peak)
        var tierB = _metricService.EvaluateTier(
            FitnessMetricType.DeadliftFiveRepMax, 315, 150, true);

        Assert.Equal(PerformanceTier.Good, tierA);
        Assert.Equal(PerformanceTier.Good, tierB);

        // Athlete C: 150 lbs, Deadlift 5RM 350 lbs
        // Est 1RM = 350 * 1.15 = 402.5 -> 402.5/150 = 2.68x BW = Peak
        var tierC = _metricService.EvaluateTier(
            FitnessMetricType.DeadliftFiveRepMax, 350, 150, true);

        Assert.Equal(PerformanceTier.Peak, tierC);
    }

    [Fact]
    public void QuarterlyProgress_ShouldTrackImprovementAcrossQuarters()
    {
        var athleteService = new AthleteService(_metricService, _testDataFile);
        var athlete = athleteService.AddAthlete("Progress Tracker", null, 175, 69, true);

        // Record Resting Heart Rate each quarter
        var quarters = new (DateOnly Date, double Value)[]
        {
            (new DateOnly(2024, 2, 15), 72),  // Q1: Average
            (new DateOnly(2024, 5, 15), 65),  // Q2: Average (improving)
            (new DateOnly(2024, 8, 15), 58),  // Q3: Good
            (new DateOnly(2024, 11, 15), 52), // Q4: Good (near Peak)
        };

        foreach (var (date, value) in quarters)
        {
            athleteService.RecordMetric(athlete.Id, FitnessGroup.MetabolicMorphological,
                FitnessMetricType.RestingHeartRate, value, date);
        }

        var history = athleteService.GetMetricHistory(athlete.Id, FitnessMetricType.RestingHeartRate).ToList();

        Assert.Equal(4, history.Count);
        Assert.Equal(72, history[0].Value); // Q1
        Assert.Equal(52, history[3].Value); // Q4

        // Calculate overall improvement
        var improvement = _metricService.CalculateImprovement(
            FitnessMetricType.RestingHeartRate,
            history[0].Value,
            history[3].Value);

        Assert.True(improvement > 0); // Positive improvement (lower is better for RHR)
    }

    [Fact]
    public void FunctionalTests_ShouldEvaluateQualitativeMetrics()
    {
        // Shoe & Sock Balance: 0=Fail, 1=Struggle, 2=Smooth
        var balanceSmooth = _metricService.EvaluateTier(FitnessMetricType.ShoeAndSockBalance, 2);
        var balanceStruggle = _metricService.EvaluateTier(FitnessMetricType.ShoeAndSockBalance, 1);
        var balanceFail = _metricService.EvaluateTier(FitnessMetricType.ShoeAndSockBalance, 0);
        Assert.Equal(PerformanceTier.Peak, balanceSmooth);
        Assert.Equal(PerformanceTier.Good, balanceStruggle);
        Assert.Equal(PerformanceTier.Average, balanceFail);

        // Deep Squat Hold: 0=Heels up, 1=Heels down, 2=Resting
        var squatHeelsUp = _metricService.EvaluateTier(FitnessMetricType.DeepSquatHold, 0);
        var squatHeelsDown = _metricService.EvaluateTier(FitnessMetricType.DeepSquatHold, 1);
        var squatResting = _metricService.EvaluateTier(FitnessMetricType.DeepSquatHold, 2);
        Assert.Equal(PerformanceTier.Average, squatHeelsUp);
        Assert.Equal(PerformanceTier.Good, squatHeelsDown);
        Assert.Equal(PerformanceTier.Peak, squatResting);
    }

    [Fact]
    public void TierEvaluation_ShouldChangeWhenBodyweightChanges()
    {
        var athleteService = new AthleteService(_metricService, _testDataFile);

        // Athlete at 200 lbs, Deadlift 5RM 275 lbs
        // Est 1RM = 275 * 1.15 = 316.25 -> 316.25/200 = 1.58x BW = Average
        var athlete = athleteService.AddAthlete("Weight Changer", null, 200, 70, true);
        athleteService.RecordMetric(athlete.Id, FitnessGroup.NeuromuscularStructural,
            FitnessMetricType.DeadliftFiveRepMax, 275, new DateOnly(2024, 1, 15));

        var tierBefore = _metricService.EvaluateTier(
            FitnessMetricType.DeadliftFiveRepMax, 275, 200, true);
        Assert.Equal(PerformanceTier.Average, tierBefore);

        // Athlete loses weight to 150 lbs
        // Est 1RM = 275 * 1.15 = 316.25 -> 316.25/150 = 2.11x BW = Good
        athleteService.UpdateAthlete(athlete.Id, athlete.Name, null, 150, 70, true);

        var tierAfter = _metricService.EvaluateTier(
            FitnessMetricType.DeadliftFiveRepMax, 275, athlete.BodyweightLbs, athlete.IsMale);
        Assert.Equal(PerformanceTier.Good, tierAfter);
    }

    [Fact]
    public void TierEvaluation_ShouldChangeWhenSexChanges()
    {
        // Dead Hang 50 seconds as male = Average (threshold: 30s)
        var tierMale = _metricService.EvaluateTier(
            FitnessMetricType.DeadHangTime, 50, 160, true);
        Assert.Equal(PerformanceTier.Average, tierMale);

        // Dead Hang 50 seconds as female = Good (threshold: 45s)
        var tierFemale = _metricService.EvaluateTier(
            FitnessMetricType.DeadHangTime, 50, 160, false);
        Assert.Equal(PerformanceTier.Good, tierFemale);
    }

    [Fact]
    public async Task DataIntegrity_MultipleSaveLoadCycles()
    {
        var athleteService = new AthleteService(_metricService, _testDataFile);

        var athlete = athleteService.AddAthlete("Integrity Test", null, 185, 71, true);
        athleteService.RecordMetric(athlete.Id, FitnessGroup.MetabolicMorphological,
            FitnessMetricType.RestingHeartRate, 60, new DateOnly(2024, 1, 15));
        await athleteService.SaveAsync();

        for (var cycle = 1; cycle <= 3; cycle++)
        {
            var service = new AthleteService(_metricService, _testDataFile);
            await service.LoadAsync();

            var loadedAthlete = service.GetAthleteById(athlete.Id);
            Assert.NotNull(loadedAthlete);

            service.RecordMetric(loadedAthlete.Id, FitnessGroup.MetabolicMorphological,
                FitnessMetricType.RestingHeartRate, 60 - cycle, new DateOnly(2024, 1 + cycle * 3, 15));

            await service.SaveAsync();
        }

        var finalService = new AthleteService(_metricService, _testDataFile);
        await finalService.LoadAsync();

        var finalAthlete = finalService.GetAthleteById(athlete.Id);
        Assert.NotNull(finalAthlete);
        Assert.Equal(4, finalAthlete.MetricRecords.Count);
    }
}

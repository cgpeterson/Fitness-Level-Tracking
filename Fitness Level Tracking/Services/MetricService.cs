using Fitness_Level_Tracking.Models;

namespace Fitness_Level_Tracking.Services;

/// <summary>
/// Provides utility operations for fitness metrics based on the Integrated Fitness Protocol.
/// </summary>
public sealed class MetricService : IMetricService
{
    private static readonly Dictionary<FitnessMetricType, (string DisplayName, string Unit, bool LowerIsBetter, FitnessGroup Group)> MetricInfo = new()
    {
        // Group 1: Metabolic & Morphological
        [FitnessMetricType.RestingHeartRate] = ("Resting Heart Rate", "bpm", true, FitnessGroup.MetabolicMorphological),
        [FitnessMetricType.WaistToHeightRatio] = ("Waist:Height Ratio", "ratio", true, FitnessGroup.MetabolicMorphological),
        [FitnessMetricType.TwelveMinuteRun] = ("12-Minute Run", "miles", false, FitnessGroup.MetabolicMorphological),
        [FitnessMetricType.HeartRateRecovery] = ("HR Recovery (1 min)", "bpm drop", false, FitnessGroup.MetabolicMorphological),

        // Group 2: Neuromuscular & Structural
        [FitnessMetricType.DeadliftFiveRepMax] = ("Deadlift 5RM", "lbs", false, FitnessGroup.NeuromuscularStructural),
        [FitnessMetricType.NeutralPressRepMax] = ("Neutral Press 5RM", "lbs", false, FitnessGroup.NeuromuscularStructural),
        [FitnessMetricType.MaxPushUps] = ("Max Push-Ups", "reps", false, FitnessGroup.NeuromuscularStructural),
        [FitnessMetricType.DeadHangTime] = ("Dead Hang", "sec", false, FitnessGroup.NeuromuscularStructural),

        // Group 3: Functional & Dynamic
        [FitnessMetricType.ShoeAndSockBalance] = ("Shoe & Sock Balance", "pass/fail", false, FitnessGroup.FunctionalDynamic),
        [FitnessMetricType.DeepSquatHold] = ("Deep Squat Hold", "quality", false, FitnessGroup.FunctionalDynamic),
        [FitnessMetricType.FarmerCarryDistance] = ("Farmer's Carry", "meters", false, FitnessGroup.FunctionalDynamic),
        [FitnessMetricType.SittingRisingTest] = ("Sitting-Rising Test", "points", false, FitnessGroup.FunctionalDynamic)
    };

    private static readonly Dictionary<FitnessGroup, string> GroupNames = new()
    {
        [FitnessGroup.MetabolicMorphological] = "Group 1: Metabolic & Morphological",
        [FitnessGroup.NeuromuscularStructural] = "Group 2: Neuromuscular & Structural",
        [FitnessGroup.FunctionalDynamic] = "Group 3: Functional & Dynamic"
    };

    public string GetMetricDisplayName(FitnessMetricType metricType)
    {
        return MetricInfo.TryGetValue(metricType, out var info) ? info.DisplayName : metricType.ToString();
    }

    public string GetGroupDisplayName(FitnessGroup group)
    {
        return GroupNames.TryGetValue(group, out var name) ? name : group.ToString();
    }

    public string GetMetricUnit(FitnessMetricType metricType)
    {
        return MetricInfo.TryGetValue(metricType, out var info) ? info.Unit : "";
    }

    public bool IsLowerBetter(FitnessMetricType metricType)
    {
        return MetricInfo.TryGetValue(metricType, out var info) && info.LowerIsBetter;
    }

    public double CalculateImprovement(FitnessMetricType metricType, double oldValue, double newValue)
    {
        if (oldValue == 0)
        {
            return 0;
        }

        var percentChange = ((newValue - oldValue) / oldValue) * 100;
        return IsLowerBetter(metricType) ? -percentChange : percentChange;
    }

    public int GetQuarter(DateOnly date)
    {
        return (date.Month - 1) / 3 + 1;
    }

    public IReadOnlyList<FitnessMetricType> GetAllMetricTypes()
    {
        return Enum.GetValues<FitnessMetricType>();
    }

    public IReadOnlyList<FitnessGroup> GetAllGroups()
    {
        return Enum.GetValues<FitnessGroup>();
    }

    public IReadOnlyList<FitnessMetricType> GetMetricsForGroup(FitnessGroup group)
    {
        return MetricInfo
            .Where(kvp => kvp.Value.Group == group)
            .Select(kvp => kvp.Key)
            .ToList();
    }

    public FitnessGroup GetGroupForMetric(FitnessMetricType metricType)
    {
        return MetricInfo.TryGetValue(metricType, out var info) ? info.Group : FitnessGroup.MetabolicMorphological;
    }

    public PerformanceTier EvaluateTier(FitnessMetricType metricType, double value, double? bodyweightLbs = null, bool? isMale = null)
    {
        var male = isMale ?? true;
        var bw = bodyweightLbs ?? 180;

        return metricType switch
        {
            // Group 1: Metabolic & Morphological
            FitnessMetricType.RestingHeartRate => value switch
            {
                < 50 => PerformanceTier.Peak,
                <= 60 => PerformanceTier.Good,
                <= 75 => PerformanceTier.Average,
                _ => PerformanceTier.BelowAverage
            },

            FitnessMetricType.WaistToHeightRatio => value switch
            {
                <= 0.45 => PerformanceTier.Peak,
                <= 0.50 => PerformanceTier.Good,
                <= 0.55 => PerformanceTier.Average,
                _ => PerformanceTier.BelowAverage
            },

            FitnessMetricType.TwelveMinuteRun => value switch
            {
                >= 1.85 => PerformanceTier.Peak,
                >= 1.5 => PerformanceTier.Good,
                >= 1.3 => PerformanceTier.Average,
                _ => PerformanceTier.BelowAverage
            },

            FitnessMetricType.HeartRateRecovery => value switch
            {
                >= 50 => PerformanceTier.Peak,
                >= 30 => PerformanceTier.Good,
                >= 20 => PerformanceTier.Average,
                _ => PerformanceTier.BelowAverage
            },

            // Group 2: Neuromuscular & Structural (relative to bodyweight)
            FitnessMetricType.DeadliftFiveRepMax => EvaluateRelativeStrength(value, bw, 1.5, 2.0, 2.5, isOneRm: false),

            FitnessMetricType.NeutralPressRepMax => EvaluateRelativeStrength(value, bw, 0.5, 0.75, 1.0, isOneRm: false),

            FitnessMetricType.MaxPushUps => male switch
            {
                true => value switch
                {
                    >= 75 => PerformanceTier.Peak,
                    >= 40 => PerformanceTier.Good,
                    >= 20 => PerformanceTier.Average,
                    _ => PerformanceTier.BelowAverage
                },
                false => value switch
                {
                    >= 50 => PerformanceTier.Peak,
                    >= 25 => PerformanceTier.Good,
                    >= 10 => PerformanceTier.Average,
                    _ => PerformanceTier.BelowAverage
                }
            },

            FitnessMetricType.DeadHangTime => male switch
            {
                true => value switch
                {
                    >= 180 => PerformanceTier.Peak,
                    >= 90 => PerformanceTier.Good,
                    >= 30 => PerformanceTier.Average,
                    _ => PerformanceTier.BelowAverage
                },
                false => value switch
                {
                    >= 120 => PerformanceTier.Peak,
                    >= 45 => PerformanceTier.Good,
                    >= 15 => PerformanceTier.Average,
                    _ => PerformanceTier.BelowAverage
                }
            },

            // Group 3: Functional & Dynamic
            FitnessMetricType.ShoeAndSockBalance => value switch
            {
                >= 2 => PerformanceTier.Peak,    // Smooth/Fluid
                >= 1 => PerformanceTier.Good,    // Complete w/ struggle
                _ => PerformanceTier.Average     // Cannot complete
            },

            FitnessMetricType.DeepSquatHold => value switch
            {
                >= 2 => PerformanceTier.Peak,   // Resting position
                >= 1 => PerformanceTier.Good,   // Heels down 2 min
                _ => PerformanceTier.Average    // Heels up / Tension
            },

            FitnessMetricType.FarmerCarryDistance => value switch
            {
                >= 60 => PerformanceTier.Peak,
                >= 20 => PerformanceTier.Good,
                > 0 => PerformanceTier.Average,
                _ => PerformanceTier.BelowAverage
            },

            FitnessMetricType.SittingRisingTest => value switch
            {
                >= 10 => PerformanceTier.Peak,
                >= 8 => PerformanceTier.Good,
                >= 6 => PerformanceTier.Average,
                _ => PerformanceTier.BelowAverage
            },

            _ => PerformanceTier.Average
        };
    }

    private static PerformanceTier EvaluateRelativeStrength(double weight, double bodyweight, double avgMultiple, double goodMultiple, double peakMultiple, bool isOneRm)
    {
        // Convert 5RM to estimated 1RM: Weight × 1.15
        var effectiveWeight = isOneRm ? weight : weight * 1.15;
        var ratio = effectiveWeight / bodyweight;

        return ratio switch
        {
            _ when ratio >= peakMultiple => PerformanceTier.Peak,
            _ when ratio >= goodMultiple => PerformanceTier.Good,
            _ when ratio >= avgMultiple => PerformanceTier.Average,
            _ => PerformanceTier.BelowAverage
        };
    }

    public (string Average, string Good, string Peak) GetTierThresholds(FitnessMetricType metricType, bool isMale = true)
    {
        return metricType switch
        {
            FitnessMetricType.RestingHeartRate => ("70-75 bpm", "50-60 bpm", "< 50 bpm"),
            FitnessMetricType.WaistToHeightRatio => ("> 0.55", "0.45-0.50", "0.40-0.45"),
            FitnessMetricType.TwelveMinuteRun => ("< 1.3 miles", "1.5-1.75 miles", "> 1.85 miles"),
            FitnessMetricType.HeartRateRecovery => ("< 20 bpm", "30-40 bpm", "> 50 bpm"),

            FitnessMetricType.DeadliftFiveRepMax => ("1.5x BW", "2.0x BW", "> 2.5x BW"),
            FitnessMetricType.NeutralPressRepMax => ("0.5x BW", "0.75x BW", "> 1.0x BW"),
            FitnessMetricType.MaxPushUps => isMale ? ("20 reps", "40 reps", "75+ reps") : ("10 reps", "25 reps", "50+ reps"),
            FitnessMetricType.DeadHangTime => isMale ? ("30-60 sec", "90-120 sec", "> 180 sec") : ("15-30 sec", "45-90 sec", "> 120 sec"),

            FitnessMetricType.ShoeAndSockBalance => ("Cannot complete (0)", "Complete w/ struggle (1)", "Smooth/Fluid (2)"),
            FitnessMetricType.DeepSquatHold => ("Heels up", "Heels down 2 min", "Resting position"),
            FitnessMetricType.FarmerCarryDistance => ("Cannot lift", "20-40 meters", "> 60 meters"),
            FitnessMetricType.SittingRisingTest => ("< 6 points", "8 points", "10 points"),

            _ => ("—", "—", "—")
        };
    }
}

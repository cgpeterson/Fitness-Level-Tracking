namespace Fitness_Level_Tracking.Models;

/// <summary>
/// Represents the fitness testing groups from the Integrated Fitness Protocol.
/// </summary>
public enum FitnessGroup
{
    /// <summary>Group 1: Metabolic & Morphological Profile - The Engine and The Vessel.</summary>
    MetabolicMorphological = 1,

    /// <summary>Group 2: Neuromuscular & Structural Profile - Mechanical Output and Force Production.</summary>
    NeuromuscularStructural = 2,

    /// <summary>Group 3: Functional & Dynamic Profile - Eastern Quality, Mobility, and Balance.</summary>
    FunctionalDynamic = 3
}

/// <summary>
/// Represents the performance tier for a metric result.
/// </summary>
public enum PerformanceTier
{
    /// <summary>Below average or pathological range.</summary>
    BelowAverage,

    /// <summary>Average or intermediate level.</summary>
    Average,

    /// <summary>Good level - the longevity/health target.</summary>
    Good,

    /// <summary>Peak/Elite performance level.</summary>
    Peak
}

/// <summary>
/// Represents the types of fitness metrics from the Integrated Fitness Protocol.
/// </summary>
public enum FitnessMetricType
{
    // === Group 1: Metabolic & Morphological Profile ===

    /// <summary>Resting heart rate in BPM after 5 minutes of quiet sitting.</summary>
    RestingHeartRate,

    /// <summary>Waist circumference divided by height (ratio).</summary>
    WaistToHeightRatio,

    /// <summary>Distance covered in 12-minute Cooper Test in miles.</summary>
    TwelveMinuteRun,

    /// <summary>Heart rate drop in 1 minute after max effort (BPM).</summary>
    HeartRateRecovery,

    // === Group 2: Neuromuscular & Structural Profile ===

    /// <summary>5-Rep Max Deadlift weight in pounds.</summary>
    DeadliftFiveRepMax,

    /// <summary>5-Rep Max Neutral Grip Dumbbell Press (combined weight) in pounds.</summary>
    NeutralPressRepMax,

    /// <summary>Maximum continuous push-ups with proper form.</summary>
    MaxPushUps,

    /// <summary>Dead hang time from pull-up bar in seconds.</summary>
    DeadHangTime,

    // === Group 3: Functional & Dynamic Profile ===

    /// <summary>Shoe & Sock balance test result (Pass/Fail as 1/0).</summary>
    ShoeAndSockBalance,

    /// <summary>Deep squat hold quality (0=Heels up, 1=Heels down 2min, 2=Resting position).</summary>
    DeepSquatHold,

    /// <summary>Farmer's carry distance at bodyweight in meters.</summary>
    FarmerCarryDistance,

    /// <summary>Sitting-Rising Test score (0-10 points).</summary>
    SittingRisingTest
}


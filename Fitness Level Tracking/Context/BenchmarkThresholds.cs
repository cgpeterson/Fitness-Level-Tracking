namespace Fitness_Level_Tracking.Context;

/// <summary>
/// Documents all benchmark thresholds used in the Integrated Fitness Protocol.
/// This static class serves as a reference for tier evaluation logic in <see cref="Services.MetricService"/>.
/// </summary>
public static class BenchmarkThresholds
{
    // ──────────────────────────────────────────────────────────
    // Group 1: Metabolic & Morphological
    // ──────────────────────────────────────────────────────────

    // Resting Heart Rate (bpm) — lower is better
    //   Peak:         < 50
    //   Good:         50–60
    //   Average:      61–75
    //   Below Average: > 75

    // Waist-to-Height Ratio — lower is better
    //   Peak:         ≤ 0.45
    //   Good:         0.46–0.50
    //   Average:      0.51–0.55
    //   Below Average: > 0.55

    // 12-Minute Run (miles) — higher is better
    //   Peak:         ≥ 1.85
    //   Good:         1.50–1.84
    //   Average:      1.30–1.49
    //   Below Average: < 1.30

    // Heart Rate Recovery, 1-min drop (bpm) — higher is better
    //   Peak:         ≥ 50
    //   Good:         30–49
    //   Average:      20–29
    //   Below Average: < 20

    // ──────────────────────────────────────────────────────────
    // Group 2: Neuromuscular & Structural
    // ──────────────────────────────────────────────────────────

    // Deadlift 5RM — relative to bodyweight (5RM × 1.15 = est. 1RM)
    //   Peak:         ≥ 2.5× BW
    //   Good:         2.0–2.49× BW
    //   Average:      1.5–1.99× BW
    //   Below Average: < 1.5× BW

    // Neutral Grip Dumbbell Press 5RM — relative to bodyweight (5RM × 1.15 = est. 1RM)
    //   Peak:         ≥ 1.0× BW
    //   Good:         0.75–0.99× BW
    //   Average:      0.50–0.74× BW
    //   Below Average: < 0.50× BW

    // Max Push-Ups (reps) — sex-specific
    //   Male:   Peak ≥ 75, Good ≥ 40, Average ≥ 20, Below Average < 20
    //   Female: Peak ≥ 50, Good ≥ 25, Average ≥ 10, Below Average < 10

    // Dead Hang (seconds) — sex-specific
    //   Male:   Peak ≥ 180, Good ≥ 90, Average ≥ 30, Below Average < 30
    //   Female: Peak ≥ 120, Good ≥ 45, Average ≥ 15, Below Average < 15

    // ──────────────────────────────────────────────────────────
    // Group 3: Functional & Dynamic
    // ──────────────────────────────────────────────────────────

    // Shoe & Sock Balance — qualitative (3 levels)
    //   Peak:    2 (Smooth/Fluid completion)
    //   Good:    1 (Complete w/ struggle)
    //   Average: 0 (Cannot complete)

    // Deep Squat Hold — qualitative
    //   Peak:    2 (Resting position)
    //   Good:    1 (Heels down, 2 min)
    //   Average: 0 (Heels up / Tension)

    // Farmer's Carry Distance (meters) — higher is better
    //   Peak:         ≥ 60
    //   Good:         20–59
    //   Average:      1–19
    //   Below Average: 0

    // Sitting-Rising Test (points, 0–10) — higher is better
    //   Peak:         ≥ 10
    //   Good:         8–9
    //   Average:      6–7
    //   Below Average: < 6
}

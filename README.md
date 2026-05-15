# Fitness Level Tracking

A Windows Forms desktop application for tracking and analyzing athlete performance metrics over time. Built in C#/.NET, it demonstrates production-grade architecture patterns — interface-based service layers, dependency injection, and data visualization — in a standalone desktop context.

## What It Does

The application manages multiple athlete profiles in a single window, each surfaced as a tabbed view. For each athlete, it records fitness test results across configurable metric types (e.g., push-ups, VO2 max, sprint time), timestamps each entry, and renders trend charts so coaches or analysts can observe progression at a glance. Filtering by metric type and date range allows for focused self-study analysis.

## Architecture

The design deliberately separates concerns into three layers:

**Models** (`/Models`) define the domain: `Athlete` (profile + demographic data), `MetricRecord` (a timestamped test result), and `FitnessMetricType` (an enum-backed type descriptor). These are plain data classes with no UI coupling.

**Services** (`/Services`) own all business logic and data access behind interfaces. `IAthleteService`, `IMetricService`, and `IChartService` define contracts; the concrete implementations (`AthleteService`, `MetricService`, `ChartService`) are resolved at startup. This inversion-of-control boundary means the UI layer never touches persistence or calculation logic directly, and each service can be swapped or unit-tested independently.

**Controls** (`/Controls`) and the main form (`Form1.cs`) compose the UI, consuming services through their interfaces rather than concrete types. The chart rendering pipeline is handled through `ChartService`, keeping visualization logic out of the form.

The practical consequence of this structure is that adding a new metric type, a new data source, or replacing the charting library requires changes in exactly one place — not a refactor that ripples across the UI.

## Tech Stack

- **Language/Runtime**: C# / .NET (Windows Forms)
- **Architecture**: Service layer pattern, interface-based DI, MVC-adjacent separation
- **Visualization**: Windows Forms DataVisualization (Chart control)
- **Project format**: `.csproj` / Visual Studio solution

## Running Locally

### Prerequisites

- Visual Studio 2022 (or later) with the **.NET desktop development** workload installed
- .NET 6.0 SDK or later

### Steps

1. Clone the repository:
   ```bash
      git clone https://github.com/cgpeterson/Fitness-Level-Tracking.git
         ```
         2. Open `Fitness Level Tracking.sln` in Visual Studio.
         3. Build the solution (`Ctrl+Shift+B`). NuGet packages restore automatically.
         4. Run (`F5`). No external services or environment variables are required — all data is persisted locally.

         ### Running Tests

         A companion test project (`Fitness Level Tracking.Tests`) covers service-layer logic. Run from the Test Explorer or via:

         ```bash
         dotnet test
         ```

         ## Environment Setup

         No environment variables or secrets are required. The application uses local file storage; no database or cloud connection is needed to run.

         ## Design Notes

         The interface-first approach was a conscious choice over a simpler "everything in Form1" design. For a desktop app of this size, the overhead is small and the payoff is meaningful: service classes remain unit-testable, the UI stays thin, and the codebase is approachable to anyone familiar with enterprise C#/.NET patterns. The same architectural discipline that applies to large-scale Azure or enterprise systems scales down cleanly here.

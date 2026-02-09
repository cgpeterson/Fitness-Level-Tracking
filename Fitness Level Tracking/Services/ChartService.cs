using Fitness_Level_Tracking.Models;

namespace Fitness_Level_Tracking.Services;

/// <summary>
/// Renders line charts for fitness metric data using GDI+.
/// </summary>
public sealed class ChartService : IChartService
{
    private readonly IMetricService _metricService;

    private static readonly Color[] MetricColors =
    [
        Color.FromArgb(66, 133, 244),   // Blue
        Color.FromArgb(234, 67, 53),    // Red
        Color.FromArgb(251, 188, 5),    // Yellow
        Color.FromArgb(52, 168, 83),    // Green
        Color.FromArgb(156, 39, 176),   // Purple
        Color.FromArgb(255, 87, 34),    // Orange
        Color.FromArgb(0, 188, 212),    // Cyan
        Color.FromArgb(233, 30, 99),    // Pink
        Color.FromArgb(139, 195, 74),   // Light Green
        Color.FromArgb(121, 85, 72),    // Brown
        Color.FromArgb(63, 81, 181),    // Indigo
        Color.FromArgb(255, 193, 7)     // Amber
    ];

    private static readonly Color[] AthleteColors =
    [
        Color.FromArgb(66, 133, 244),   // Blue
        Color.FromArgb(234, 67, 53),    // Red
        Color.FromArgb(52, 168, 83),    // Green
        Color.FromArgb(156, 39, 176),   // Purple
        Color.FromArgb(255, 87, 34),    // Orange
        Color.FromArgb(0, 188, 212),    // Cyan
        Color.FromArgb(233, 30, 99),    // Pink
        Color.FromArgb(139, 195, 74)    // Light Green
    ];

    private const int Padding = 60;
    private const int LegendWidth = 150;
    private const int PointRadius = 4;

    public ChartService(IMetricService metricService)
    {
        _metricService = metricService ?? throw new ArgumentNullException(nameof(metricService));
    }

    public void RenderChart(Panel panel, IEnumerable<Athlete> athletes, ChartConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(panel);
        ArgumentNullException.ThrowIfNull(athletes);
        ArgumentNullException.ThrowIfNull(configuration);

        var bitmap = new Bitmap(panel.Width, panel.Height);
        using var graphics = Graphics.FromImage(bitmap);
        graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        graphics.Clear(Color.FromArgb(30, 30, 30));

        var athleteList = athletes.ToList();
        if (athleteList.Count == 0 || configuration.SelectedMetrics.Count == 0)
        {
            DrawNoDataMessage(graphics, panel);
            UpdatePanelBackground(panel, bitmap);
            return;
        }

        var chartArea = CalculateChartArea(panel, configuration.ShowLegend);
        var dataPoints = CollectDataPoints(athleteList, configuration);

        if (dataPoints.Count == 0)
        {
            DrawNoDataMessage(graphics, panel);
            UpdatePanelBackground(panel, bitmap);
            return;
        }

        var (minValue, maxValue, quarters) = CalculateScales(dataPoints);

        if (configuration.ShowGrid)
        {
            DrawGrid(graphics, chartArea, minValue, maxValue, quarters);
        }

        DrawAxes(graphics, chartArea, minValue, maxValue, quarters);
        DrawDataLines(graphics, chartArea, dataPoints, minValue, maxValue, quarters, configuration);

        if (configuration.ShowLegend)
        {
            DrawLegend(graphics, panel, dataPoints, configuration);
        }

        UpdatePanelBackground(panel, bitmap);
    }

    public Color GetMetricColor(FitnessMetricType metricType)
    {
        return MetricColors[(int)metricType % MetricColors.Length];
    }

    public Color GetAthleteColor(int athleteIndex)
    {
        return AthleteColors[athleteIndex % AthleteColors.Length];
    }

    public void RenderChartFromRecords(Panel panel, string athleteName, IEnumerable<MetricRecord> records)
    {
        ArgumentNullException.ThrowIfNull(panel);
        ArgumentNullException.ThrowIfNull(records);

        var bitmap = new Bitmap(panel.Width, panel.Height);
        using var graphics = Graphics.FromImage(bitmap);
        graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        graphics.Clear(Color.FromArgb(30, 30, 30));

        var recordList = records.ToList();
        if (recordList.Count == 0)
        {
            DrawNoDataMessage(graphics, panel);
            UpdatePanelBackground(panel, bitmap);
            return;
        }

        var chartArea = CalculateChartArea(panel, showLegend: true);

        // Group records by metric type
        var series = recordList
            .GroupBy(r => r.MetricType)
            .Select(g => new ChartDataSeries
            {
                AthleteId = Guid.Empty,
                AthleteName = athleteName,
                MetricType = g.Key,
                MetricName = _metricService.GetMetricDisplayName(g.Key),
                Records = g.OrderBy(r => r.Year).ThenBy(r => r.Quarter).ToList(),
                Color = GetMetricColor(g.Key)
            })
            .ToList();

        var (minValue, maxValue, quarters) = CalculateScales(series);

        DrawGrid(graphics, chartArea, minValue, maxValue, quarters);
        DrawAxes(graphics, chartArea, minValue, maxValue, quarters);

        // Use a simple configuration for drawing
        var config = new ChartConfiguration { OverlayAthletes = false };
        DrawDataLines(graphics, chartArea, series, minValue, maxValue, quarters, config);
        DrawLegend(graphics, panel, series, config);

        UpdatePanelBackground(panel, bitmap);
    }

    private static Rectangle CalculateChartArea(Panel panel, bool showLegend)
    {
        var legendSpace = showLegend ? LegendWidth : 0;
        return new Rectangle(
            Padding,
            Padding / 2,
            panel.Width - Padding * 2 - legendSpace,
            panel.Height - Padding - Padding / 2);
    }

    private List<ChartDataSeries> CollectDataPoints(List<Athlete> athletes, ChartConfiguration configuration)
    {
        var series = new List<ChartDataSeries>();

        var athletesToProcess = configuration.SelectedAthleteId.HasValue
            ? athletes.Where(a => a.Id == configuration.SelectedAthleteId.Value)
            : configuration.OverlayAthletes ? athletes : athletes.Take(1);

        var athleteIndex = 0;
        foreach (var athlete in athletesToProcess)
        {
            foreach (var metricType in configuration.SelectedMetrics)
            {
                var records = athlete.GetRecordsForMetric(metricType).ToList();
                if (records.Count > 0)
                {
                    series.Add(new ChartDataSeries
                    {
                        AthleteId = athlete.Id,
                        AthleteName = athlete.Name,
                        MetricType = metricType,
                        MetricName = _metricService.GetMetricDisplayName(metricType),
                        Records = records,
                        Color = configuration.OverlayAthletes
                            ? GetAthleteColor(athleteIndex)
                            : GetMetricColor(metricType)
                    });
                }
            }
            athleteIndex++;
        }

        return series;
    }

    private static (double Min, double Max, List<string> Quarters) CalculateScales(List<ChartDataSeries> series)
    {
        var allValues = series.SelectMany(s => s.Records.Select(r => r.Value)).ToList();
        var minValue = allValues.Min();
        var maxValue = allValues.Max();

        // Add padding to the range
        var range = maxValue - minValue;
        if (range == 0)
        {
            range = maxValue * 0.2;
        }
        minValue -= range * 0.1;
        maxValue += range * 0.1;

        // Ensure we don't go below 0 for most metrics
        if (minValue < 0)
        {
            minValue = 0;
        }

        var quarters = series
            .SelectMany(s => s.Records)
            .Select(r => r.QuarterLabel)
            .Distinct()
            .OrderBy(q => q)
            .ToList();

        return (minValue, maxValue, quarters);
    }

    private static void DrawGrid(Graphics graphics, Rectangle chartArea, double minValue, double maxValue, List<string> quarters)
    {
        using var gridPen = new Pen(Color.FromArgb(60, 60, 60), 1);

        // Horizontal grid lines
        const int gridLines = 5;
        for (var i = 0; i <= gridLines; i++)
        {
            var y = chartArea.Bottom - (int)(chartArea.Height * i / (double)gridLines);
            graphics.DrawLine(gridPen, chartArea.Left, y, chartArea.Right, y);
        }

        // Vertical grid lines
        for (var i = 0; i < quarters.Count; i++)
        {
            var x = chartArea.Left + (int)(chartArea.Width * i / (double)Math.Max(quarters.Count - 1, 1));
            graphics.DrawLine(gridPen, x, chartArea.Top, x, chartArea.Bottom);
        }
    }

    private static void DrawAxes(Graphics graphics, Rectangle chartArea, double minValue, double maxValue, List<string> quarters)
    {
        using var axisPen = new Pen(Color.White, 2);
        using var labelFont = new Font("Segoe UI", 9);
        using var labelBrush = new SolidBrush(Color.LightGray);

        // Draw axes
        graphics.DrawLine(axisPen, chartArea.Left, chartArea.Bottom, chartArea.Right, chartArea.Bottom);
        graphics.DrawLine(axisPen, chartArea.Left, chartArea.Top, chartArea.Left, chartArea.Bottom);

        // Y-axis labels
        const int gridLines = 5;
        for (var i = 0; i <= gridLines; i++)
        {
            var value = minValue + (maxValue - minValue) * i / gridLines;
            var y = chartArea.Bottom - (int)(chartArea.Height * i / (double)gridLines);
            graphics.DrawString(value.ToString("N0"), labelFont, labelBrush, 5, y - 8);
        }

        // X-axis labels
        for (var i = 0; i < quarters.Count; i++)
        {
            var x = chartArea.Left + (int)(chartArea.Width * i / (double)Math.Max(quarters.Count - 1, 1));
            var labelSize = graphics.MeasureString(quarters[i], labelFont);
            graphics.DrawString(quarters[i], labelFont, labelBrush, x - labelSize.Width / 2, chartArea.Bottom + 5);
        }
    }

    private void DrawDataLines(Graphics graphics, Rectangle chartArea, List<ChartDataSeries> series,
        double minValue, double maxValue, List<string> quarters, ChartConfiguration configuration)
    {
        foreach (var dataSeries in series)
        {
            using var linePen = new Pen(dataSeries.Color, 2);
            using var pointBrush = new SolidBrush(dataSeries.Color);

            var points = new List<Point>();

            foreach (var record in dataSeries.Records)
            {
                var quarterIndex = quarters.IndexOf(record.QuarterLabel);
                if (quarterIndex < 0) continue;

                var x = chartArea.Left + (int)(chartArea.Width * quarterIndex / (double)Math.Max(quarters.Count - 1, 1));
                var y = chartArea.Bottom - (int)(chartArea.Height * (record.Value - minValue) / (maxValue - minValue));
                points.Add(new Point(x, y));
            }

            // Sort points by X coordinate to ensure lines connect chronologically
            points.Sort((a, b) => a.X.CompareTo(b.X));

            // Draw lines
            if (points.Count > 1)
            {
                graphics.DrawLines(linePen, points.ToArray());
            }

            // Draw points
            foreach (var point in points)
            {
                graphics.FillEllipse(pointBrush, point.X - PointRadius, point.Y - PointRadius, 
                    PointRadius * 2, PointRadius * 2);
            }
        }
    }

    private void DrawLegend(Graphics graphics, Panel panel, List<ChartDataSeries> series, ChartConfiguration configuration)
    {
        using var legendFont = new Font("Segoe UI", 9);
        using var legendBrush = new SolidBrush(Color.LightGray);

        var legendX = panel.Width - LegendWidth + 10;
        var legendY = Padding / 2;

        foreach (var dataSeries in series)
        {
            using var colorBrush = new SolidBrush(dataSeries.Color);

            graphics.FillRectangle(colorBrush, legendX, legendY + 4, 12, 12);

            var label = configuration.OverlayAthletes
                ? $"{dataSeries.AthleteName}"
                : $"{dataSeries.MetricName}";

            graphics.DrawString(label, legendFont, legendBrush, legendX + 18, legendY);
            legendY += 20;
        }
    }

    private static void DrawNoDataMessage(Graphics graphics, Panel panel)
    {
        using var font = new Font("Segoe UI", 12);
        using var brush = new SolidBrush(Color.Gray);
        
        const string message = "Select metrics and add data to view chart";
        var size = graphics.MeasureString(message, font);
        graphics.DrawString(message, font, brush,
            (panel.Width - size.Width) / 2,
            (panel.Height - size.Height) / 2);
    }

    private static void UpdatePanelBackground(Panel panel, Bitmap bitmap)
    {
        panel.BackgroundImage?.Dispose();
        panel.BackgroundImage = bitmap;
        panel.BackgroundImageLayout = ImageLayout.None;
    }

    private sealed class ChartDataSeries
    {
        public Guid AthleteId { get; init; }
        public required string AthleteName { get; init; }
        public FitnessMetricType MetricType { get; init; }
        public required string MetricName { get; init; }
        public required List<MetricRecord> Records { get; init; }
        public Color Color { get; init; }
    }
}

using System.ComponentModel;
using System.Drawing.Drawing2D;
using Fitness_Level_Tracking.Models;
using Fitness_Level_Tracking.Services;

namespace Fitness_Level_Tracking.Controls;

/// <summary>
/// A Power Platform Model-Driven App style Subgrid control with Galaxy Luxury design system.
/// Features a command bar header with title and action buttons, plus a styled data grid.
/// </summary>
public sealed class GalaxySubgridControl : UserControl
{
    // Galaxy Theme Colors
    private static readonly Color GalaxyGreenDeep = Color.FromArgb(2, 10, 8);
    private static readonly Color GridBackground = Color.FromArgb(5, 14, 12);
    private static readonly Color VelvetPurpleStart = Color.FromArgb(26, 10, 46);
    private static readonly Color VelvetPurpleEnd = Color.FromArgb(49, 0, 73);
    private static readonly Color AccentGold = Color.FromArgb(197, 160, 89);
    private static readonly Color TextWhite = Color.FromArgb(240, 240, 240);
    private static readonly Color SubtleBorder = Color.FromArgb(20, 255, 255, 255);
    private static readonly Color RowHoverColor = Color.FromArgb(40, 4, 77, 58);
    private static readonly Color GridLineColor = Color.FromArgb(30, 45, 40); // Solid dark green-gray for grid lines

    private readonly Panel _headerPanel;
    private readonly Label _titleLabel;
    private readonly GalaxyButton _newButton;
    private readonly GalaxyButton _refreshButton;
    private readonly GalaxyButton _deleteButton;
    private readonly DataGridView _dataGrid;
    private readonly Panel _contentPanel;

    private int _hoveredRowIndex = -1;

    public event EventHandler? NewButtonClicked;
    public event EventHandler? RefreshButtonClicked;
    public event EventHandler? DeleteButtonClicked;
    public event EventHandler<DataGridViewCellEventArgs>? RowClicked;
    public event EventHandler<DataGridViewCellEventArgs>? RowDoubleClicked;
    public event EventHandler? SelectionChanged;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    [Browsable(false)]
    public string Title
    {
        get => _titleLabel.Text;
        set => _titleLabel.Text = value;
    }

    [Browsable(false)]
    public DataGridView DataGrid => _dataGrid;

    public GalaxySubgridControl()
    {
        SetStyle(
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.UserPaint |
            ControlStyles.OptimizedDoubleBuffer |
            ControlStyles.ResizeRedraw,
            true);

        BackColor = GalaxyGreenDeep;
        Padding = new Padding(1);
        MinimumSize = new Size(200, 150);

        // Create main content panel (holds header + grid)
        _contentPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = GridBackground,
            Padding = new Padding(0)
        };

        // Create header panel with gradient
        _headerPanel = new GradientPanel
        {
            Dock = DockStyle.Top,
            Height = 44,
            Padding = new Padding(12, 8, 12, 8)
        };

        // Title label
        _titleLabel = new Label
        {
            Text = "Active Records",
            Font = new Font("Segoe UI", 12f, FontStyle.Bold),
            ForeColor = AccentGold,
            AutoSize = true,
            Location = new Point(12, 11)
        };

        // Button container (right-aligned)
        var buttonPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Right,
            FlowDirection = FlowDirection.RightToLeft,
            AutoSize = true,
            BackColor = Color.Transparent,
            Padding = new Padding(0),
            Margin = new Padding(0)
        };

        // New button with plus icon
        _newButton = new GalaxyButton
        {
            Text = "+ New",
            Size = new Size(70, 28),
            Margin = new Padding(0, 0, 0, 0)
        };
        _newButton.Click += (s, e) => NewButtonClicked?.Invoke(this, EventArgs.Empty);

        // Delete button
        _deleteButton = new GalaxyButton
        {
            Text = "✕ Delete",
            Size = new Size(75, 28),
            Margin = new Padding(0, 0, 8, 0)
        };
        _deleteButton.Click += (s, e) => DeleteButtonClicked?.Invoke(this, EventArgs.Empty);

        // Refresh button
        _refreshButton = new GalaxyButton
        {
            Text = "↻",
            Size = new Size(32, 28),
            Font = new Font("Segoe UI", 11f, FontStyle.Regular),
            Margin = new Padding(0, 0, 8, 0)
        };
        _refreshButton.Click += (s, e) => RefreshButtonClicked?.Invoke(this, EventArgs.Empty);

        buttonPanel.Controls.Add(_newButton);
        buttonPanel.Controls.Add(_deleteButton);
        buttonPanel.Controls.Add(_refreshButton);

        _headerPanel.Controls.Add(_titleLabel);
        _headerPanel.Controls.Add(buttonPanel);

        // Create and style the DataGridView
        _dataGrid = CreateStyledDataGrid();

        _contentPanel.Controls.Add(_dataGrid);
        _contentPanel.Controls.Add(_headerPanel);

        Controls.Add(_contentPanel);
    }

    private DataGridView CreateStyledDataGrid()
    {
        var grid = new DataGridView
        {
            Dock = DockStyle.Fill,
            BackgroundColor = GridBackground,
            GridColor = GridLineColor,
            BorderStyle = BorderStyle.None,
            CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
            RowHeadersVisible = false,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AllowUserToResizeRows = false,
            ReadOnly = true,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            EnableHeadersVisualStyles = false,
            ColumnHeadersHeight = 36,
            RowTemplate = { Height = 40 },
            ScrollBars = ScrollBars.Vertical
        };

        // Column header styling
        grid.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = VelvetPurpleStart,
            ForeColor = AccentGold,
            Font = new Font("Segoe UI", 9f, FontStyle.Bold),
            Alignment = DataGridViewContentAlignment.MiddleLeft,
            Padding = new Padding(8, 0, 0, 0),
            SelectionBackColor = VelvetPurpleStart,
            SelectionForeColor = AccentGold
        };
        grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;

        // Default cell styling
        grid.DefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = GridBackground,
            ForeColor = TextWhite,
            Font = new Font("Segoe UI", 9f, FontStyle.Regular),
            SelectionBackColor = AccentGold,
            SelectionForeColor = GalaxyGreenDeep,
            Alignment = DataGridViewContentAlignment.MiddleLeft,
            Padding = new Padding(8, 0, 0, 0)
        };

        // Alternating row colors for subtle effect
        grid.AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = Color.FromArgb(8, 18, 14),
            ForeColor = TextWhite,
            SelectionBackColor = AccentGold,
            SelectionForeColor = GalaxyGreenDeep
        };

        // Event handlers for hover effect
        grid.CellMouseEnter += OnCellMouseEnter;
        grid.CellMouseLeave += OnCellMouseLeave;
        grid.CellClick += (s, e) => RowClicked?.Invoke(this, e);
        grid.CellDoubleClick += (s, e) =>
        {
            if (e.RowIndex >= 0) RowDoubleClicked?.Invoke(this, e);
        };
        grid.SelectionChanged += (s, e) => SelectionChanged?.Invoke(this, EventArgs.Empty);

        return grid;
    }

    private void OnCellMouseEnter(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex >= 0 && e.RowIndex < _dataGrid.Rows.Count)
        {
            _hoveredRowIndex = e.RowIndex;
            var row = _dataGrid.Rows[e.RowIndex];
            if (!row.Selected)
            {
                row.DefaultCellStyle.BackColor = RowHoverColor;
            }
        }
    }

    private void OnCellMouseLeave(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex >= 0 && e.RowIndex < _dataGrid.Rows.Count)
        {
            _hoveredRowIndex = -1;
            var row = _dataGrid.Rows[e.RowIndex];
            if (!row.Selected)
            {
                // Reset to default or alternating color
                row.DefaultCellStyle.BackColor = e.RowIndex % 2 == 0
                    ? GridBackground
                    : Color.FromArgb(8, 18, 14);
            }
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        // Draw rounded border
        var rect = new Rectangle(0, 0, Width - 1, Height - 1);
        using var path = CreateRoundedRectangle(rect, 8);
        using var borderPen = new Pen(SubtleBorder, 1);
        g.DrawPath(borderPen, path);
    }

    private static GraphicsPath CreateRoundedRectangle(Rectangle rect, int radius)
    {
        var path = new GraphicsPath();
        var diameter = radius * 2;

        path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
        path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
        path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
        path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
        path.CloseFigure();

        return path;
    }

    /// <summary>
    /// Configures the grid columns for metric records display.
    /// </summary>
    public void ConfigureMetricColumns()
    {
        _dataGrid.Columns.Clear();

        _dataGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "RecordId",
            HeaderText = "ID",
            Visible = false
        });

        _dataGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Group",
            HeaderText = "Group",
            Width = 60,
            MinimumWidth = 50
        });

        _dataGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Metric",
            HeaderText = "Metric",
            FillWeight = 120
        });

        _dataGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Value",
            HeaderText = "Value",
            Width = 80,
            MinimumWidth = 60
        });

        _dataGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Tier",
            HeaderText = "Tier",
            Width = 90,
            MinimumWidth = 70
        });

        _dataGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Quarter",
            HeaderText = "Quarter",
            Width = 80,
            MinimumWidth = 70
        });

        _dataGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Date",
            HeaderText = "Date",
            Width = 90,
            MinimumWidth = 80
        });
    }

    /// <summary>
    /// Loads mock data for preview/testing purposes.
    /// </summary>
    public void LoadMockData()
    {
        ConfigureMetricColumns();
        _dataGrid.Rows.Clear();

        var mockData = new[]
        {
            (Id: Guid.NewGuid(), Group: "G1", Metric: "Resting Heart Rate", Value: "58 BPM", Tier: "Good", Quarter: "Q1 2025", Date: "01/15/2025"),
            (Id: Guid.NewGuid(), Group: "G1", Metric: "12-Minute Run", Value: "1.75 mi", Tier: "Peak", Quarter: "Q1 2025", Date: "01/15/2025"),
            (Id: Guid.NewGuid(), Group: "G2", Metric: "Deadlift 5RM", Value: "315 lbs", Tier: "Good", Quarter: "Q1 2025", Date: "01/18/2025"),
            (Id: Guid.NewGuid(), Group: "G3", Metric: "Dead Hang Time", Value: "75 sec", Tier: "Average", Quarter: "Q4 2024", Date: "12/20/2024")
        };

        foreach (var item in mockData)
        {
            var rowIndex = _dataGrid.Rows.Add(
                item.Id.ToString(),
                item.Group,
                item.Metric,
                item.Value,
                item.Tier,
                item.Quarter,
                item.Date
            );

            // Color-code tier
            var tierCell = _dataGrid.Rows[rowIndex].Cells["Tier"];
            tierCell.Style.ForeColor = item.Tier switch
            {
                "Peak" => Color.FromArgb(52, 168, 83),
                "Good" => Color.FromArgb(66, 133, 244),
                "Average" => Color.FromArgb(251, 188, 5),
                _ => Color.FromArgb(234, 67, 53)
            };
        }
    }

    /// <summary>
    /// Displays metric records in the grid with proper formatting and tier coloring.
    /// </summary>
    public void DisplayRecords(
        IEnumerable<MetricRecord> records,
        IMetricService metricService,
        double? bodyweightLbs,
        bool? isMale)
    {
        _dataGrid.Rows.Clear();

        foreach (var record in records)
        {
            var shortGroupName = $"G{(int)record.Group}";
            var unit = metricService.GetMetricUnit(record.MetricType);
            var valueText = FormatMetricValue(record.MetricType, record.Value, unit);

            var tier = metricService.EvaluateTier(
                record.MetricType,
                record.Value,
                bodyweightLbs,
                isMale);

            var rowIndex = _dataGrid.Rows.Add(
                record.Id.ToString(),
                shortGroupName,
                metricService.GetMetricDisplayName(record.MetricType),
                valueText,
                tier.ToString(),
                record.QuarterLabel,
                record.RecordedDate.ToString("d")
            );

            // Color-code tier
            var tierCell = _dataGrid.Rows[rowIndex].Cells["Tier"];
            tierCell.Style.ForeColor = tier switch
            {
                PerformanceTier.Peak => Color.FromArgb(52, 168, 83),
                PerformanceTier.Good => Color.FromArgb(66, 133, 244),
                PerformanceTier.Average => Color.FromArgb(251, 188, 5),
                _ => Color.FromArgb(234, 67, 53)
            };
        }

        // Update title with record count
        Title = $"Active Records ({_dataGrid.Rows.Count})";
    }

    private static string FormatMetricValue(FitnessMetricType metricType, double value, string unit)
    {
        return metricType switch
        {
            FitnessMetricType.WaistToHeightRatio => $"{value:F2}",
            FitnessMetricType.TwelveMinuteRun => $"{value:F2} {unit}",
            FitnessMetricType.ShoeAndSockBalance => value switch
            {
                >= 2 => "Smooth",
                >= 1 => "Struggle",
                _ => "Fail"
            },
            FitnessMetricType.DeepSquatHold => value switch
            {
                >= 2 => "Resting",
                >= 1 => "Heels Down",
                _ => "Heels Up"
            },
            _ => $"{value:N0} {unit}"
        };
    }

    /// <summary>
    /// Gets the selected record ID, or null if nothing is selected.
    /// </summary>
    public Guid? GetSelectedRecordId()
    {
        if (_dataGrid.SelectedRows.Count == 0)
            return null;

        var recordIdString = _dataGrid.SelectedRows[0].Cells["RecordId"]?.Value?.ToString();
        return Guid.TryParse(recordIdString, out var recordId) ? recordId : null;
    }

    /// <summary>
    /// Gradient panel for the velvet purple header effect.
    /// </summary>
    private sealed class GradientPanel : Panel
    {
        protected override void OnPaint(PaintEventArgs e)
        {
            using var brush = new LinearGradientBrush(
                ClientRectangle,
                VelvetPurpleStart,
                VelvetPurpleEnd,
                LinearGradientMode.Horizontal);

            e.Graphics.FillRectangle(brush, ClientRectangle);
            base.OnPaint(e);
        }

        public GradientPanel()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
        }
    }
}

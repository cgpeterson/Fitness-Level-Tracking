using System.Drawing.Drawing2D;
using Fitness_Level_Tracking.Models;
using Fitness_Level_Tracking.Services;

namespace Fitness_Level_Tracking.Controls;

/// <summary>
/// Galaxy-styled popup form for entering new metric records.
/// </summary>
public sealed class GalaxyRecordEntryForm : Form
{
    // Galaxy Theme Colors
    private static readonly Color GalaxyGreenDeep = Color.FromArgb(2, 10, 8);
    private static readonly Color VelvetPurpleStart = Color.FromArgb(26, 10, 46);
    private static readonly Color VelvetPurpleEnd = Color.FromArgb(49, 0, 73);
    private static readonly Color AccentGold = Color.FromArgb(197, 160, 89);
    private static readonly Color TextWhite = Color.FromArgb(240, 240, 240);
    private static readonly Color InputBackground = Color.FromArgb(15, 30, 25);

    private readonly IMetricService _metricService;
    private readonly bool _isMale;

    private ComboBox _groupCombo = null!;
    private DateTimePicker _recordDate = null!;
    private Panel _metricsPanel = null!;
    private readonly Dictionary<FitnessMetricType, NumericUpDown> _metricInputs = [];

    public FitnessGroup SelectedGroup { get; private set; }
    public DateOnly RecordDate { get; private set; }
    public Dictionary<FitnessMetricType, double> MetricValues { get; } = [];

    public GalaxyRecordEntryForm(IMetricService metricService, bool isMale)
    {
        _metricService = metricService ?? throw new ArgumentNullException(nameof(metricService));
        _isMale = isMale;

        InitializeForm();
        InitializeComponents();
        UpdateMetricsPanel();
    }

    private void InitializeForm()
    {
        Text = "Record New Metrics";
        Size = new Size(480, 700);
        MinimumSize = new Size(450, 600);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.Sizable;
        MaximizeBox = false;
        MinimizeBox = false;
        BackColor = GalaxyGreenDeep;
        ForeColor = TextWhite;
        Font = new Font("Segoe UI", 9f);
    }

    private void InitializeComponents()
    {
        var mainLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 4,
            ColumnCount = 1,
            Padding = new Padding(20),
            BackColor = GalaxyGreenDeep
        };
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));  // Header
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 80));  // Group & Date
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); // Metrics
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));  // Buttons

        // Header
        var headerPanel = new GradientHeaderPanel
        {
            Dock = DockStyle.Fill
        };
        var titleLabel = new Label
        {
            Text = "📊 Record Group Test Results",
            Font = new Font("Segoe UI", 14f, FontStyle.Bold),
            ForeColor = AccentGold,
            AutoSize = false,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(10, 0, 0, 0)
        };
        headerPanel.Controls.Add(titleLabel);
        mainLayout.Controls.Add(headerPanel, 0, 0);

        // Group & Date selection
        var selectionPanel = CreateSelectionPanel();
        mainLayout.Controls.Add(selectionPanel, 0, 1);

        // Metrics input panel (scrollable)
        var metricsContainer = new Panel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            BackColor = Color.FromArgb(5, 15, 12)
        };
        _metricsPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoSize = true,
            Padding = new Padding(10)
        };
        metricsContainer.Controls.Add(_metricsPanel);
        mainLayout.Controls.Add(metricsContainer, 0, 2);

        // Buttons
        var buttonPanel = CreateButtonPanel();
        mainLayout.Controls.Add(buttonPanel, 0, 3);

        Controls.Add(mainLayout);
    }

    private Panel CreateSelectionPanel()
    {
        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 2,
            BackColor = GalaxyGreenDeep
        };
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

        // Group selection
        var groupLabel = CreateLabel("Select Group:");
        panel.Controls.Add(groupLabel, 0, 0);

        _groupCombo = new ComboBox
        {
            Dock = DockStyle.Fill,
            DropDownStyle = ComboBoxStyle.DropDownList,
            BackColor = InputBackground,
            ForeColor = TextWhite,
            FlatStyle = FlatStyle.Flat,
            Margin = new Padding(0, 0, 10, 5)
        };
        foreach (var group in _metricService.GetAllGroups())
        {
            _groupCombo.Items.Add(new GroupItem(group, _metricService.GetGroupDisplayName(group)));
        }
        _groupCombo.SelectedIndex = 0;
        _groupCombo.SelectedIndexChanged += (s, e) => UpdateMetricsPanel();
        panel.Controls.Add(_groupCombo, 0, 1);

        // Date selection
        var dateLabel = CreateLabel("Test Date:");
        panel.Controls.Add(dateLabel, 1, 0);

        _recordDate = new DateTimePicker
        {
            Dock = DockStyle.Fill,
            Format = DateTimePickerFormat.Short,
            Value = DateTime.Today,
            Margin = new Padding(10, 0, 0, 5)
        };
        panel.Controls.Add(_recordDate, 1, 1);

        return panel;
    }

    private Panel CreateButtonPanel()
    {
        var panel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            BackColor = GalaxyGreenDeep,
            Padding = new Padding(0, 10, 0, 0)
        };

        // Cancel button
        var cancelButton = new GalaxyButton
        {
            Text = "Cancel",
            Size = new Size(90, 32),
            Margin = new Padding(0, 0, 0, 0)
        };
        cancelButton.Click += (s, e) =>
        {
            DialogResult = DialogResult.Cancel;
            Close();
        };

        // Save button (with filled gold background)
        var saveButton = new Button
        {
            Text = "💾 Save Record",
            Size = new Size(120, 32),
            BackColor = AccentGold,
            ForeColor = GalaxyGreenDeep,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9f, FontStyle.Bold),
            Margin = new Padding(0, 0, 10, 0),
            Cursor = Cursors.Hand
        };
        saveButton.FlatAppearance.BorderSize = 0;
        saveButton.Click += OnSaveClicked;

        panel.Controls.Add(cancelButton);
        panel.Controls.Add(saveButton);

        return panel;
    }

    private void UpdateMetricsPanel()
    {
        _metricsPanel.Controls.Clear();
        _metricInputs.Clear();

        if (_groupCombo.SelectedItem is not GroupItem selectedGroup)
            return;

        var metrics = _metricService.GetMetricsForGroup(selectedGroup.Group);

        // Group title
        var groupTitle = new Label
        {
            Text = selectedGroup.DisplayName,
            Font = new Font("Segoe UI", 11f, FontStyle.Bold),
            ForeColor = AccentGold,
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 15)
        };
        _metricsPanel.Controls.Add(groupTitle);

        foreach (var metricType in metrics)
        {
            var metricName = _metricService.GetMetricDisplayName(metricType);
            var unit = _metricService.GetMetricUnit(metricType);
            var thresholds = _metricService.GetTierThresholds(metricType, _isMale);

            // Container for each metric
            var metricContainer = new Panel
            {
                Width = 420,
                Height = 75,
                Margin = new Padding(0, 0, 0, 8),
                BackColor = Color.FromArgb(8, 20, 16),
                Padding = new Padding(10, 8, 10, 8)
            };

            // Metric label
            var label = new Label
            {
                Text = $"{metricName} ({unit})",
                Font = new Font("Segoe UI", 9.5f, FontStyle.Regular),
                ForeColor = TextWhite,
                AutoSize = true,
                Location = new Point(10, 8)
            };
            metricContainer.Controls.Add(label);

            // Input control
            var (min, max, decimals) = GetInputRangeForMetric(metricType);
            var input = new NumericUpDown
            {
                Width = 130,
                Height = 26,
                Minimum = min,
                Maximum = max,
                DecimalPlaces = decimals,
                Value = min,
                BackColor = InputBackground,
                ForeColor = TextWhite,
                Font = new Font("Segoe UI", 10f),
                Location = new Point(10, 32)
            };
            _metricInputs[metricType] = input;
            metricContainer.Controls.Add(input);

            // Threshold hints
            var hintLabel = new Label
            {
                Text = $"Avg: {thresholds.Average}  |  Good: {thresholds.Good}  |  Peak: {thresholds.Peak}",
                Font = new Font("Segoe UI", 8f),
                ForeColor = Color.FromArgb(150, 150, 150),
                AutoSize = true,
                Location = new Point(10, 56)
            };
            metricContainer.Controls.Add(hintLabel);

            // Good target indicator
            var targetLabel = new Label
            {
                Text = $"Target: {thresholds.Good}",
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 168, 83),
                AutoSize = true,
                Location = new Point(150, 36)
            };
            metricContainer.Controls.Add(targetLabel);

            _metricsPanel.Controls.Add(metricContainer);
        }
    }

    private void OnSaveClicked(object? sender, EventArgs e)
    {
        if (_groupCombo.SelectedItem is not GroupItem selectedGroup)
        {
            MessageBox.Show("Please select a group.", "Validation Error",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        SelectedGroup = selectedGroup.Group;
        RecordDate = DateOnly.FromDateTime(_recordDate.Value);

        MetricValues.Clear();
        foreach (var (metricType, input) in _metricInputs)
        {
            MetricValues[metricType] = (double)input.Value;
        }

        DialogResult = DialogResult.OK;
        Close();
    }

    private Label CreateLabel(string text)
    {
        return new Label
        {
            Text = text,
            ForeColor = AccentGold,
            AutoSize = true,
            Font = new Font("Segoe UI", 9f),
            Margin = new Padding(0, 5, 0, 2)
        };
    }

    private static (decimal min, decimal max, int decimals) GetInputRangeForMetric(FitnessMetricType metricType)
    {
        return metricType switch
        {
            FitnessMetricType.RestingHeartRate => (30, 120, 0),
            FitnessMetricType.WaistToHeightRatio => (0.3m, 0.8m, 2),
            FitnessMetricType.TwelveMinuteRun => (0.5m, 3.0m, 2),
            FitnessMetricType.HeartRateRecovery => (0, 100, 0),
            FitnessMetricType.DeadliftFiveRepMax => (0, 1000, 0),
            FitnessMetricType.NeutralPressRepMax => (0, 500, 0),
            FitnessMetricType.MaxPushUps => (0, 200, 0),
            FitnessMetricType.DeadHangTime => (0, 600, 0),
            FitnessMetricType.ShoeAndSockBalance => (0, 1, 0),
            FitnessMetricType.DeepSquatHold => (0, 2, 0),
            FitnessMetricType.FarmerCarryDistance => (0, 200, 0),
            FitnessMetricType.SittingRisingTest => (0, 10, 0),
            _ => (0, 10000, 2)
        };
    }

    private sealed record GroupItem(FitnessGroup Group, string DisplayName)
    {
        public override string ToString() => DisplayName;
    }

    /// <summary>
    /// Gradient header panel for the velvet purple effect.
    /// </summary>
    private sealed class GradientHeaderPanel : Panel
    {
        public GradientHeaderPanel()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
        }

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
    }
}

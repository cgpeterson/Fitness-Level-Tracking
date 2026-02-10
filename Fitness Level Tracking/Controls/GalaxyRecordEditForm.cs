using System.Drawing.Drawing2D;
using Fitness_Level_Tracking.Models;
using Fitness_Level_Tracking.Services;

namespace Fitness_Level_Tracking.Controls;

/// <summary>
/// Galaxy-styled popup form for editing an existing metric record.
/// </summary>
public sealed class GalaxyRecordEditForm : Form
{
    // Galaxy Theme Colors
    private static readonly Color GalaxyGreenDeep = Color.FromArgb(2, 10, 8);
    private static readonly Color VelvetPurpleStart = Color.FromArgb(26, 10, 46);
    private static readonly Color VelvetPurpleEnd = Color.FromArgb(49, 0, 73);
    private static readonly Color AccentGold = Color.FromArgb(197, 160, 89);
    private static readonly Color TextWhite = Color.FromArgb(240, 240, 240);
    private static readonly Color InputBackground = Color.FromArgb(15, 30, 25);

    private NumericUpDown _valueInput = null!;
    private DateTimePicker _datePicker = null!;

    public double NewValue { get; private set; }
    public DateOnly NewDate { get; private set; }

    public GalaxyRecordEditForm(MetricRecord record, IMetricService metricService, bool isMale)
    {
        ArgumentNullException.ThrowIfNull(record);
        ArgumentNullException.ThrowIfNull(metricService);

        InitializeForm();
        BuildLayout(record, metricService, isMale);
    }

    private void InitializeForm()
    {
        Text = "Edit Record";
        Size = new Size(400, 340);
        MinimumSize = new Size(380, 320);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        BackColor = GalaxyGreenDeep;
        ForeColor = TextWhite;
        Font = new Font("Segoe UI", 9f);
    }

    private void BuildLayout(MetricRecord record, IMetricService metricService, bool isMale)
    {
        var mainLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 4,
            ColumnCount = 1,
            Padding = new Padding(20),
            BackColor = GalaxyGreenDeep
        };
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));   // Header
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));   // Content
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));   // Hints
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));   // Buttons

        // Header
        var headerPanel = new GradientHeaderPanel { Dock = DockStyle.Fill };
        var metricName = metricService.GetMetricDisplayName(record.MetricType);
        var unit = metricService.GetMetricUnit(record.MetricType);
        var titleLabel = new Label
        {
            Text = $"Edit: {metricName}",
            Font = new Font("Segoe UI", 13f, FontStyle.Bold),
            ForeColor = AccentGold,
            AutoSize = false,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(10, 0, 0, 0)
        };
        headerPanel.Controls.Add(titleLabel);
        mainLayout.Controls.Add(headerPanel, 0, 0);

        // Content: value + date
        var contentPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 4,
            BackColor = Color.FromArgb(5, 15, 12),
            Padding = new Padding(10)
        };
        contentPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        contentPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        // Value row
        contentPanel.Controls.Add(CreateLabel("Value:"), 0, 0);
        var (min, max, decimals) = GetInputRangeForMetric(record.MetricType);
        _valueInput = new NumericUpDown
        {
            Dock = DockStyle.Fill,
            Minimum = min,
            Maximum = max,
            DecimalPlaces = decimals,
            Value = Math.Clamp((decimal)record.Value, min, max),
            BackColor = InputBackground,
            ForeColor = TextWhite,
            Font = new Font("Segoe UI", 10f)
        };
        contentPanel.Controls.Add(_valueInput, 1, 0);

        // Unit label
        contentPanel.Controls.Add(CreateLabel("Unit:"), 0, 1);
        var unitValueLabel = CreateLabel(unit);
        unitValueLabel.ForeColor = TextWhite;
        contentPanel.Controls.Add(unitValueLabel, 1, 1);

        // Date row
        contentPanel.Controls.Add(CreateLabel("Date:"), 0, 2);
        _datePicker = new DateTimePicker
        {
            Dock = DockStyle.Fill,
            Format = DateTimePickerFormat.Short,
            Value = record.RecordedDate.ToDateTime(TimeOnly.MinValue)
        };
        contentPanel.Controls.Add(_datePicker, 1, 2);

        // Group label
        contentPanel.Controls.Add(CreateLabel("Group:"), 0, 3);
        var groupName = metricService.GetGroupDisplayName(record.Group);
        var groupValueLabel = CreateLabel(groupName);
        groupValueLabel.ForeColor = TextWhite;
        contentPanel.Controls.Add(groupValueLabel, 1, 3);

        mainLayout.Controls.Add(contentPanel, 0, 1);

        // Threshold hints
        var thresholds = metricService.GetTierThresholds(record.MetricType, isMale);
        var hintLabel = new Label
        {
            Text = $"Avg: {thresholds.Average}  |  Good: {thresholds.Good}  |  Peak: {thresholds.Peak}",
            Font = new Font("Segoe UI", 8f),
            ForeColor = Color.FromArgb(150, 150, 150),
            AutoSize = false,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(10, 0, 0, 0)
        };
        mainLayout.Controls.Add(hintLabel, 0, 2);

        // Buttons
        var buttonPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            BackColor = GalaxyGreenDeep,
            Padding = new Padding(0, 10, 0, 0)
        };

        var cancelButton = new GalaxyButton
        {
            Text = "Cancel",
            Size = new Size(90, 32)
        };
        cancelButton.Click += (s, e) =>
        {
            DialogResult = DialogResult.Cancel;
            Close();
        };

        var saveButton = new Button
        {
            Text = "Save",
            Size = new Size(90, 32),
            BackColor = AccentGold,
            ForeColor = GalaxyGreenDeep,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9f, FontStyle.Bold),
            Margin = new Padding(0, 0, 10, 0),
            Cursor = Cursors.Hand
        };
        saveButton.FlatAppearance.BorderSize = 0;
        saveButton.Click += OnSaveClicked;

        buttonPanel.Controls.Add(cancelButton);
        buttonPanel.Controls.Add(saveButton);
        mainLayout.Controls.Add(buttonPanel, 0, 3);

        Controls.Add(mainLayout);
    }

    private void OnSaveClicked(object? sender, EventArgs e)
    {
        NewValue = (double)_valueInput.Value;
        NewDate = DateOnly.FromDateTime(_datePicker.Value.Date);
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
            Margin = new Padding(0, 8, 10, 2)
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
            FitnessMetricType.ShoeAndSockBalance => (0, 2, 0),
            FitnessMetricType.DeepSquatHold => (0, 2, 0),
            FitnessMetricType.FarmerCarryDistance => (0, 200, 0),
            FitnessMetricType.SittingRisingTest => (0, 10, 0),
            _ => (0, 10000, 2)
        };
    }

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

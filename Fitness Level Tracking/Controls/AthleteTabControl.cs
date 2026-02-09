using Fitness_Level_Tracking.Models;
using Fitness_Level_Tracking.Services;

namespace Fitness_Level_Tracking.Controls;

/// <summary>
/// User control for displaying and editing athlete details and group-based metrics.
/// </summary>
public sealed class AthleteTabControl : UserControl
{
    // Galaxy Theme Colors
    private static readonly Color GalaxyGreenDeep = Color.FromArgb(2, 10, 8);
    private static readonly Color AccentGold = Color.FromArgb(197, 160, 89);
    private static readonly Color TextWhite = Color.FromArgb(240, 240, 240);
    private static readonly Color InputBackground = Color.FromArgb(15, 30, 25);

    private readonly IAthleteService _athleteService;
    private readonly IMetricService _metricService;
    private readonly Athlete _athlete;

    // Athlete info controls (compact header)
    private TextBox _nameTextBox = null!;
    private NumericUpDown _bodyweightInput = null!;
    private NumericUpDown _heightInput = null!;
    private ComboBox _sexCombo = null!;

    // Results subgrid and filters
    private GalaxySubgridControl _metricsSubgrid = null!;
    private ComboBox _filterGroup = null!;
    private ComboBox _filterMetric = null!;
    private ComboBox _filterTier = null!;
    private ComboBox _filterQuarter = null!;
    private ComboBox _filterCurrentTier = null!;
    private List<MetricRecord> _allRecords = [];

    public event EventHandler? DataChanged;
    public event EventHandler? AthleteRemoved;

    public AthleteTabControl(Athlete athlete, IAthleteService athleteService, IMetricService metricService)
    {
        _athlete = athlete ?? throw new ArgumentNullException(nameof(athlete));
        _athleteService = athleteService ?? throw new ArgumentNullException(nameof(athleteService));
        _metricService = metricService ?? throw new ArgumentNullException(nameof(metricService));

        InitializeComponents();
        LoadAthleteData();
    }

    public Guid AthleteId => _athlete.Id;
    public string AthleteName => _athlete.Name;

    private void InitializeComponents()
    {
        Dock = DockStyle.Fill;
        BackColor = GalaxyGreenDeep;

        var mainLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Padding = new Padding(10),
            BackColor = GalaxyGreenDeep
        };
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 70));   // Athlete info header
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));   // Filter row
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));   // Subgrid

        mainLayout.Controls.Add(CreateAthleteInfoPanel(), 0, 0);
        mainLayout.Controls.Add(CreateFilterPanel(), 0, 1);
        mainLayout.Controls.Add(CreateSubgridPanel(), 0, 2);

        Controls.Add(mainLayout);
    }

    private Panel CreateAthleteInfoPanel()
    {
        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 9,
            RowCount = 2,
            BackColor = Color.FromArgb(5, 15, 12),
            Padding = new Padding(10, 5, 10, 5)
        };

        // Column styles for responsive layout
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));  // Name label
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 180)); // Name input
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 20)); // Spacer
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));  // Weight label
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80)); // Weight input
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));  // Height label
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 70)); // Height input
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));  // Sex label
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); // Sex input + Remove button

        // Row 1: Labels
        panel.Controls.Add(CreateCompactLabel("Name:"), 0, 0);
        panel.Controls.Add(CreateCompactLabel("Weight (lbs):"), 3, 0);
        panel.Controls.Add(CreateCompactLabel("Height (in):"), 5, 0);
        panel.Controls.Add(CreateCompactLabel("Sex:"), 7, 0);

        // Row 2: Inputs
        _nameTextBox = new TextBox
        {
            Dock = DockStyle.Fill,
            BackColor = InputBackground,
            ForeColor = TextWhite,
            BorderStyle = BorderStyle.FixedSingle
        };
        _nameTextBox.TextChanged += OnAthleteInfoChanged;
        panel.Controls.Add(_nameTextBox, 1, 1);

        _bodyweightInput = new NumericUpDown
        {
            Dock = DockStyle.Fill,
            Minimum = 0,
            Maximum = 500,
            DecimalPlaces = 1,
            BackColor = InputBackground,
            ForeColor = TextWhite
        };
        _bodyweightInput.ValueChanged += OnAthleteInfoChanged;
        panel.Controls.Add(_bodyweightInput, 4, 1);

        _heightInput = new NumericUpDown
        {
            Dock = DockStyle.Fill,
            Minimum = 0,
            Maximum = 100,
            DecimalPlaces = 1,
            BackColor = InputBackground,
            ForeColor = TextWhite
        };
        _heightInput.ValueChanged += OnAthleteInfoChanged;
        panel.Controls.Add(_heightInput, 6, 1);

        // Sex combo + Remove button container
        var sexContainer = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            BackColor = Color.Transparent,
            Margin = new Padding(0)
        };

        _sexCombo = new ComboBox
        {
            Width = 80,
            DropDownStyle = ComboBoxStyle.DropDownList,
            BackColor = InputBackground,
            ForeColor = TextWhite,
            FlatStyle = FlatStyle.Flat
        };
        _sexCombo.Items.AddRange(["Male", "Female"]);
        _sexCombo.SelectedIndex = 0;
        _sexCombo.SelectedIndexChanged += OnAthleteInfoChanged;
        sexContainer.Controls.Add(_sexCombo);

        // Remove athlete button (compact)
        var removeButton = new GalaxyButton
        {
            Text = "✕ Remove",
            Size = new Size(85, 26),
            Margin = new Padding(20, 0, 0, 0)
        };
        removeButton.Click += OnRemoveAthlete;
        sexContainer.Controls.Add(removeButton);

        panel.Controls.Add(sexContainer, 8, 1);

        return panel;
    }

    private Panel CreateFilterPanel()
    {
        var filterPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            BackColor = Color.FromArgb(10, 20, 16),
            Padding = new Padding(8, 6, 8, 6)
        };

        // Group filter
        filterPanel.Controls.Add(CreateFilterLabel("Group:"));
        _filterGroup = CreateFilterCombo(60);
        _filterGroup.SelectedIndexChanged += OnFilterChanged;
        filterPanel.Controls.Add(_filterGroup);

        // Metric filter
        filterPanel.Controls.Add(CreateFilterLabel("Metric:"));
        _filterMetric = CreateFilterCombo(100);
        _filterMetric.SelectedIndexChanged += OnFilterChanged;
        filterPanel.Controls.Add(_filterMetric);

        // Tier filter
        filterPanel.Controls.Add(CreateFilterLabel("Tier:"));
        _filterTier = CreateFilterCombo(85);
        _filterTier.SelectedIndexChanged += OnFilterChanged;
        filterPanel.Controls.Add(_filterTier);

        // Quarter filter
        filterPanel.Controls.Add(CreateFilterLabel("Quarter:"));
        _filterQuarter = CreateFilterCombo(75);
        _filterQuarter.SelectedIndexChanged += OnFilterChanged;
        filterPanel.Controls.Add(_filterQuarter);

        // Current Tier filter
        filterPanel.Controls.Add(CreateFilterLabel("Current:"));
        _filterCurrentTier = CreateFilterCombo(85);
        _filterCurrentTier.SelectedIndexChanged += OnFilterChanged;
        var tooltip = new ToolTip();
        tooltip.SetToolTip(_filterCurrentTier, "Filter by most recent tier for each metric");
        filterPanel.Controls.Add(_filterCurrentTier);

        // Clear filters button
        var clearButton = new GalaxyButton
        {
            Text = "Clear",
            Size = new Size(55, 24),
            Margin = new Padding(10, 2, 0, 0)
        };
        clearButton.Click += OnClearFilters;
        filterPanel.Controls.Add(clearButton);

        return filterPanel;
    }

    private Panel CreateSubgridPanel()
    {
        var panel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = GalaxyGreenDeep,
            Padding = new Padding(0, 5, 0, 0)
        };

        _metricsSubgrid = new GalaxySubgridControl
        {
            Dock = DockStyle.Fill,
            Title = "Metric Records"
        };
        _metricsSubgrid.ConfigureMetricColumns();

        // Wire up subgrid events
        _metricsSubgrid.NewButtonClicked += OnNewRecordClicked;
        _metricsSubgrid.DeleteButtonClicked += OnDeleteRecord;
        _metricsSubgrid.RefreshButtonClicked += (s, e) => RefreshMetricsGrid();
        _metricsSubgrid.SelectionChanged += (s, e) => DataChanged?.Invoke(this, EventArgs.Empty);

        panel.Controls.Add(_metricsSubgrid);
        return panel;
    }

    private void OnNewRecordClicked(object? sender, EventArgs e)
    {
        var isMale = _sexCombo.SelectedIndex == 0;
        using var form = new GalaxyRecordEntryForm(_metricService, isMale);

        if (form.ShowDialog(this) == DialogResult.OK)
        {
            _athleteService.RecordGroupMetrics(
                _athlete.Id,
                form.SelectedGroup,
                form.MetricValues,
                form.RecordDate);

            RefreshMetricsGrid();
            DataChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private static Label CreateCompactLabel(string text)
    {
        return new Label
        {
            Text = text,
            ForeColor = AccentGold,
            AutoSize = true,
            Font = new Font("Segoe UI", 8f, FontStyle.Regular),
            Margin = new Padding(0, 0, 5, 0),
            Anchor = AnchorStyles.Left | AnchorStyles.Bottom
        };
    }

    private static Label CreateFilterLabel(string text)
    {
        return new Label
        {
            Text = text,
            ForeColor = AccentGold,
            AutoSize = true,
            Margin = new Padding(5, 6, 2, 0),
            Font = new Font("Segoe UI", 8, FontStyle.Regular)
        };
    }

    private static ComboBox CreateFilterCombo(int width)
    {
        return new ComboBox
        {
            Width = width,
            DropDownStyle = ComboBoxStyle.DropDownList,
            BackColor = InputBackground,
            ForeColor = TextWhite,
            FlatStyle = FlatStyle.Flat,
            Margin = new Padding(0, 3, 8, 0)
        };
    }

    private void PopulateFilterOptions()
    {
        var currentGroup = _filterGroup.SelectedItem?.ToString();
        var currentMetric = _filterMetric.SelectedItem?.ToString();
        var currentTier = _filterTier.SelectedItem?.ToString();
        var currentQuarter = _filterQuarter.SelectedItem?.ToString();
        var currentCurrentTier = _filterCurrentTier.SelectedItem?.ToString();

        _filterGroup.Items.Clear();
        _filterGroup.Items.Add("All");
        foreach (var group in _allRecords.Select(r => r.Group).Distinct().OrderBy(g => g))
        {
            _filterGroup.Items.Add($"G{(int)group}");
        }
        _filterGroup.SelectedIndex = currentGroup != null && _filterGroup.Items.Contains(currentGroup)
            ? _filterGroup.Items.IndexOf(currentGroup) : 0;

        _filterMetric.Items.Clear();
        _filterMetric.Items.Add("All");
        foreach (var metric in _allRecords.Select(r => r.MetricType).Distinct().OrderBy(m => m))
        {
            _filterMetric.Items.Add(_metricService.GetMetricDisplayName(metric));
        }
        _filterMetric.SelectedIndex = currentMetric != null && _filterMetric.Items.Contains(currentMetric)
            ? _filterMetric.Items.IndexOf(currentMetric) : 0;

        _filterTier.Items.Clear();
        _filterTier.Items.Add("All");
        foreach (var tier in Enum.GetValues<PerformanceTier>())
        {
            _filterTier.Items.Add(tier.ToString());
        }
        _filterTier.SelectedIndex = currentTier != null && _filterTier.Items.Contains(currentTier)
            ? _filterTier.Items.IndexOf(currentTier) : 0;

        _filterQuarter.Items.Clear();
        _filterQuarter.Items.Add("All");
        foreach (var quarter in _allRecords.Select(r => r.QuarterLabel).Distinct().OrderBy(q => q))
        {
            _filterQuarter.Items.Add(quarter);
        }
        _filterQuarter.SelectedIndex = currentQuarter != null && _filterQuarter.Items.Contains(currentQuarter)
            ? _filterQuarter.Items.IndexOf(currentQuarter) : 0;

        _filterCurrentTier.Items.Clear();
        _filterCurrentTier.Items.Add("All");
        foreach (var tier in Enum.GetValues<PerformanceTier>())
        {
            _filterCurrentTier.Items.Add(tier.ToString());
        }
        _filterCurrentTier.SelectedIndex = currentCurrentTier != null && _filterCurrentTier.Items.Contains(currentCurrentTier)
            ? _filterCurrentTier.Items.IndexOf(currentCurrentTier) : 0;
    }

    private void OnFilterChanged(object? sender, EventArgs e) => ApplyFilters();

    private void OnClearFilters(object? sender, EventArgs e)
    {
        _filterGroup.SelectedIndex = 0;
        _filterMetric.SelectedIndex = 0;
        _filterTier.SelectedIndex = 0;
        _filterQuarter.SelectedIndex = 0;
        _filterCurrentTier.SelectedIndex = 0;
        ApplyFilters();
    }

    private void ApplyFilters()
    {
        var filteredRecords = GetFilteredRecords();
        _metricsSubgrid.DisplayRecords(filteredRecords, _metricService, _athlete.BodyweightLbs, _athlete.IsMale);
        // Note: Don't trigger DataChanged here - filtering is a view-only operation, not a data change
    }

    public IReadOnlyList<MetricRecord> GetFilteredRecords()
    {
        var records = _allRecords.AsEnumerable();

        if (_filterCurrentTier.SelectedIndex > 0)
        {
            var tierText = _filterCurrentTier.SelectedItem?.ToString();
            if (tierText != null && Enum.TryParse<PerformanceTier>(tierText, out var targetTier))
            {
                var metricsMatchingCurrentTier = _allRecords
                    .GroupBy(r => r.MetricType)
                    .Where(g =>
                    {
                        var mostRecent = g.OrderByDescending(r => r.Year).ThenByDescending(r => r.Quarter).First();
                        var currentTier = _metricService.EvaluateTier(mostRecent.MetricType, mostRecent.Value, _athlete.BodyweightLbs, _athlete.IsMale);
                        return currentTier == targetTier;
                    })
                    .Select(g => g.Key)
                    .ToHashSet();

                records = records.Where(r => metricsMatchingCurrentTier.Contains(r.MetricType));
            }
        }

        if (_filterGroup.SelectedIndex > 0)
        {
            var groupText = _filterGroup.SelectedItem?.ToString();
            if (groupText != null && int.TryParse(groupText.Replace("G", ""), out var groupNum))
            {
                records = records.Where(r => (int)r.Group == groupNum);
            }
        }

        if (_filterMetric.SelectedIndex > 0)
        {
            var metricText = _filterMetric.SelectedItem?.ToString();
            if (metricText != null)
            {
                records = records.Where(r => _metricService.GetMetricDisplayName(r.MetricType) == metricText);
            }
        }

        if (_filterTier.SelectedIndex > 0)
        {
            var tierText = _filterTier.SelectedItem?.ToString();
            if (tierText != null && Enum.TryParse<PerformanceTier>(tierText, out var tier))
            {
                records = records.Where(r =>
                    _metricService.EvaluateTier(r.MetricType, r.Value, _athlete.BodyweightLbs, _athlete.IsMale) == tier);
            }
        }

        if (_filterQuarter.SelectedIndex > 0)
        {
            var quarterText = _filterQuarter.SelectedItem?.ToString();
            if (quarterText != null)
            {
                records = records.Where(r => r.QuarterLabel == quarterText);
            }
        }

        return records
            .OrderByDescending(r => r.Year)
            .ThenByDescending(r => r.Quarter)
            .ThenBy(r => r.Group)
            .ThenBy(r => r.MetricType)
            .ToList();
    }

    private void LoadAthleteData()
    {
        _nameTextBox.Text = _athlete.Name;
        _bodyweightInput.Value = (decimal)(_athlete.BodyweightLbs ?? 180);
        _heightInput.Value = (decimal)(_athlete.HeightInches ?? 70);
        _sexCombo.SelectedIndex = _athlete.IsMale ?? true ? 0 : 1;

        RefreshMetricsGrid();
    }

    public void RefreshMetricsGrid()
    {
        _allRecords = _athlete.MetricRecords.ToList();
        PopulateFilterOptions();
        var filteredRecords = GetFilteredRecords();
        _metricsSubgrid.DisplayRecords(filteredRecords, _metricService, _athlete.BodyweightLbs, _athlete.IsMale);
    }

    private void OnAthleteInfoChanged(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_nameTextBox.Text)) return;

        _athleteService.UpdateAthlete(
            _athlete.Id,
            _nameTextBox.Text,
            _athlete.DateOfBirth,
            (double)_bodyweightInput.Value,
            (double)_heightInput.Value,
            _sexCombo.SelectedIndex == 0);

        DataChanged?.Invoke(this, EventArgs.Empty);
    }

    private void OnDeleteRecord(object? sender, EventArgs e)
    {
        var recordId = _metricsSubgrid.GetSelectedRecordId();
        if (recordId.HasValue)
        {
            _athleteService.RemoveMetricRecord(_athlete.Id, recordId.Value);
            RefreshMetricsGrid();
            DataChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private void OnRemoveAthlete(object? sender, EventArgs e)
    {
        var result = MessageBox.Show(
            $"Are you sure you want to remove {_athlete.Name}?",
            "Confirm Remove",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);

        if (result == DialogResult.Yes)
        {
            AthleteRemoved?.Invoke(this, EventArgs.Empty);
        }
    }
}

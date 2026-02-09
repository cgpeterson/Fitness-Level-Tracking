using Fitness_Level_Tracking.Controls;
using Fitness_Level_Tracking.Models;
using Fitness_Level_Tracking.Services;

namespace Fitness_Level_Tracking;

public partial class FormMain : Form
{
    private readonly IAthleteService _athleteService;
    private readonly IMetricService _metricService;
    private readonly IChartService _chartService;

    public FormMain() : this(null, null, null)
    {
    }

    public FormMain(IAthleteService? athleteService, IMetricService? metricService, IChartService? chartService)
    {
        _metricService = metricService ?? new MetricService();
        _athleteService = athleteService ?? new AthleteService(_metricService);
        _chartService = chartService ?? new ChartService(_metricService);

        InitializeComponent();
        InitializeChartControls();
        SetupEventHandlers();
    }

    protected override async void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        await LoadDataAsync();
    }

    protected override async void OnFormClosing(FormClosingEventArgs e)
    {
        base.OnFormClosing(e);
        await SaveDataAsync();
    }

    private async Task LoadDataAsync()
    {
        try
        {
            await _athleteService.LoadAsync();
            RefreshAthleteTabs();
            RefreshChart();
        }
        catch (Exception ex)
        {
            ShowError("Failed to load data", ex);
        }
    }

    private async Task SaveDataAsync()
    {
        try
        {
            await _athleteService.SaveAsync();
        }
        catch (Exception ex)
        {
            ShowError("Failed to save data", ex);
        }
    }

    private void InitializeChartControls()
    {
        // Create a simple header panel for the chart
        var chartHeaderPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 30,
            BackColor = Color.FromArgb(40, 40, 40),
            Padding = new Padding(10, 5, 10, 5)
        };

        var chartLabel = new Label
        {
            Text = "Chart shows filtered data from the table below. Use filters to update the chart.",
            ForeColor = Color.LightGray,
            AutoSize = true,
            Font = new Font("Segoe UI", 9),
            Location = new Point(10, 7)
        };
        chartHeaderPanel.Controls.Add(chartLabel);

        panelChart.Controls.Add(chartHeaderPanel);
        panelChart.Resize += (s, e) => RefreshChart();
    }

    private void SetupEventHandlers()
    {
        tabControlAthleteDetails.SelectedIndexChanged += OnTabSelectionChanged;
    }

    private void OnTabSelectionChanged(object? sender, EventArgs e)
    {
        RefreshChart();
    }

    private void RefreshAthleteTabs()
    {
        tabControlAthleteDetails.SuspendLayout();
        tabControlAthleteDetails.TabPages.Clear();

        foreach (var athlete in _athleteService.GetAllAthletes())
        {
            AddAthleteTab(athlete);
        }

        // Add the "+" tab for adding new athletes
        AddNewAthleteTab();

        tabControlAthleteDetails.ResumeLayout();

        if (tabControlAthleteDetails.TabPages.Count > 1)
        {
            tabControlAthleteDetails.SelectedIndex = 0;
        }
    }

    private void AddAthleteTab(Athlete athlete)
    {
        var tabPage = new TabPage(athlete.Name)
        {
            Tag = athlete.Id,
            BackColor = Color.FromArgb(45, 45, 45)
        };

        var control = new AthleteTabControl(athlete, _athleteService, _metricService)
        {
            Dock = DockStyle.Fill
        };

        control.DataChanged += async (s, e) =>
        {
            // Update tab text if name changed
            tabPage.Text = athlete.Name;
            RefreshChart();
            await SaveDataAsync();
        };

        control.AthleteRemoved += async (s, e) =>
        {
            _athleteService.RemoveAthlete(athlete.Id);
            RefreshAthleteTabs();
            RefreshChart();
            await SaveDataAsync();
        };

        tabPage.Controls.Add(control);
        tabControlAthleteDetails.TabPages.Insert(tabControlAthleteDetails.TabPages.Count, tabPage);
    }

    private void AddNewAthleteTab()
    {
        var addTab = new TabPage("+")
        {
            BackColor = Color.FromArgb(45, 45, 45),
            ToolTipText = "Add new athlete"
        };

        var addPanel = new Panel { Dock = DockStyle.Fill };

        var layout = new FlowLayoutPanel
        {
            Dock = DockStyle.None,
            FlowDirection = FlowDirection.TopDown,
            AutoSize = true,
            Location = new Point(20, 20)
        };

        var nameLabel = new Label
        {
            Text = "New Athlete Name:",
            ForeColor = Color.White,
            AutoSize = true
        };
        layout.Controls.Add(nameLabel);

        var nameTextBox = new TextBox
        {
            Width = 200,
            BackColor = Color.FromArgb(60, 60, 60),
            ForeColor = Color.White
        };
        layout.Controls.Add(nameTextBox);

        var addButton = new Button
        {
            Text = "Add Athlete",
            Width = 200,
            Height = 30,
            BackColor = Color.FromArgb(52, 168, 83),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Margin = new Padding(0, 10, 0, 0)
        };

        addButton.Click += async (s, e) =>
        {
            if (string.IsNullOrWhiteSpace(nameTextBox.Text))
            {
                MessageBox.Show("Please enter a name for the athlete.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var newAthlete = _athleteService.AddAthlete(nameTextBox.Text.Trim());
            nameTextBox.Clear();
            RefreshAthleteTabs();

            // Select the new athlete's tab
            for (var i = 0; i < tabControlAthleteDetails.TabPages.Count; i++)
            {
                if (tabControlAthleteDetails.TabPages[i].Tag is Guid id && id == newAthlete.Id)
                {
                    tabControlAthleteDetails.SelectedIndex = i;
                    break;
                }
            }

            await SaveDataAsync();
        };
        layout.Controls.Add(addButton);

        // Add test athlete button - only visible when debugging in Visual Studio
#if DEBUG
        if (System.Diagnostics.Debugger.IsAttached)
        {
            var testButton = new Button
            {
                Text = "Create Test Athlete (Demo Data)",
                Width = 200,
                Height = 30,
                BackColor = Color.FromArgb(66, 133, 244),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(0, 20, 0, 0)
            };

            testButton.Click += async (s, e) =>
            {
                var testAthlete = _athleteService.CreateTestAthlete();
                RefreshAthleteTabs();

                // Select the test athlete's tab
                for (var i = 0; i < tabControlAthleteDetails.TabPages.Count; i++)
                {
                    if (tabControlAthleteDetails.TabPages[i].Tag is Guid id && id == testAthlete.Id)
                    {
                        tabControlAthleteDetails.SelectedIndex = i;
                        break;
                    }
                }

                await SaveDataAsync();
            };
            layout.Controls.Add(testButton);
        }
#endif

        addPanel.Controls.Add(layout);
        addTab.Controls.Add(addPanel);

        tabControlAthleteDetails.TabPages.Add(addTab);
    }

    private void RefreshChart()
    {
        if (panelChart.Width <= 0 || panelChart.Height <= 0)
        {
            return;
        }

        var chartArea = new Panel
        {
            Location = new Point(0, 35),
            Size = new Size(panelChart.Width - 4, panelChart.Height - 39),
            BackColor = Color.FromArgb(30, 30, 30)
        };

        // Get filtered records from the current athlete tab
        var currentTab = tabControlAthleteDetails.SelectedTab;
        if (currentTab?.Controls.Count > 0 && currentTab.Controls[0] is AthleteTabControl athleteTab)
        {
            var filteredRecords = athleteTab.GetFilteredRecords();
            _chartService.RenderChartFromRecords(chartArea, athleteTab.AthleteName, filteredRecords);
        }
        else
        {
            // No athlete selected, render empty chart
            _chartService.RenderChartFromRecords(chartArea, "", []);
        }

        // Find and replace existing chart area
        Panel? existingChartArea = null;
        foreach (Control control in panelChart.Controls)
        {
            if (control is Panel panel && panel.Location.Y > 0)
            {
                existingChartArea = panel;
                break;
            }
        }

        if (existingChartArea != null)
        {
            existingChartArea.BackgroundImage?.Dispose();
            existingChartArea.BackgroundImage = chartArea.BackgroundImage;
            chartArea.BackgroundImage = null;
            chartArea.Dispose();
        }
        else
        {
            panelChart.Controls.Add(chartArea);
        }
    }

    private static void ShowError(string message, Exception ex)
    {
        MessageBox.Show($"{message}: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}

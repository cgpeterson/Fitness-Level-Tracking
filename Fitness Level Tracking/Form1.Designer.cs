namespace Fitness_Level_Tracking;

partial class FormMain
{
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    ///  Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        panelChart = new Panel();
        panelTests = new Panel();
        tabControlAthleteDetails = new TabControl();
        panelTests.SuspendLayout();
        SuspendLayout();
        // 
        // panelChart
        // 
        panelChart.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        panelChart.BorderStyle = BorderStyle.Fixed3D;
        panelChart.Location = new Point(12, 12);
        panelChart.Name = "panelChart";
        panelChart.Size = new Size(776, 217);
        panelChart.TabIndex = 0;
        // 
        // panelTests
        // 
        panelTests.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        panelTests.BorderStyle = BorderStyle.FixedSingle;
        panelTests.Controls.Add(tabControlAthleteDetails);
        panelTests.Location = new Point(12, 235);
        panelTests.Name = "panelTests";
        panelTests.Size = new Size(776, 203);
        panelTests.TabIndex = 1;
        // 
        // tabControlAthleteDetails
        // 
        tabControlAthleteDetails.Dock = DockStyle.Fill;
        tabControlAthleteDetails.Name = "tabControlAthleteDetails";
        tabControlAthleteDetails.SelectedIndex = 0;
        tabControlAthleteDetails.TabIndex = 0;
        // 
        // FormMain
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = SystemColors.Desktop;
        ClientSize = new Size(800, 450);
        Controls.Add(panelTests);
        Controls.Add(panelChart);
        ForeColor = SystemColors.ButtonHighlight;
        MaximizeBox = false;
        MinimizeBox = false;
        Name = "FormMain";
        Text = "Fitness Level Tracking";
        WindowState = FormWindowState.Maximized;
        panelTests.ResumeLayout(false);
        ResumeLayout(false);
    }

    #endregion

    private Panel panelChart;
    private Panel panelTests;
    private TabControl tabControlAthleteDetails;
}

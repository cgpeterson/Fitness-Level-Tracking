using System.Drawing.Drawing2D;

namespace Fitness_Level_Tracking.Controls;

/// <summary>
/// Custom button styled with the Galaxy Luxury design system.
/// Features gold accent colors with hover state transitions.
/// </summary>
public sealed class GalaxyButton : Button
{
    // Galaxy Theme Colors
    private static readonly Color AccentGold = Color.FromArgb(197, 160, 89);
    private static readonly Color GalaxyGreenDeep = Color.FromArgb(2, 10, 8);
    private static readonly Color TransparentBackground = Color.FromArgb(0, 0, 0, 0);

    private bool _isHovered;
    private bool _isPressed;

    public GalaxyButton()
    {
        SetStyle(
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.UserPaint |
            ControlStyles.OptimizedDoubleBuffer,
            true);

        FlatStyle = FlatStyle.Flat;
        FlatAppearance.BorderSize = 0;
        BackColor = TransparentBackground;
        ForeColor = AccentGold;
        Font = new Font("Segoe UI", 9f, FontStyle.Regular);
        Cursor = Cursors.Hand;
        Size = new Size(80, 28);
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        base.OnMouseEnter(e);
        _isHovered = true;
        Invalidate();
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        _isHovered = false;
        _isPressed = false;
        Invalidate();
    }

    protected override void OnMouseDown(MouseEventArgs mevent)
    {
        base.OnMouseDown(mevent);
        _isPressed = true;
        Invalidate();
    }

    protected override void OnMouseUp(MouseEventArgs mevent)
    {
        base.OnMouseUp(mevent);
        _isPressed = false;
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs pevent)
    {
        var g = pevent.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

        // Clear the background first to prevent text overlay issues
        g.Clear(Parent?.BackColor ?? GalaxyGreenDeep);

        var rect = new Rectangle(0, 0, Width - 1, Height - 1);
        var path = CreateRoundedRectangle(rect, 4);

        // Determine colors based on state
        Color bgColor;
        Color fgColor;
        Color borderColor;

        if (_isPressed)
        {
            bgColor = Color.FromArgb(180, AccentGold.R, AccentGold.G, AccentGold.B);
            fgColor = GalaxyGreenDeep;
            borderColor = AccentGold;
        }
        else if (_isHovered)
        {
            bgColor = AccentGold;
            fgColor = GalaxyGreenDeep;
            borderColor = AccentGold;
        }
        else
        {
            bgColor = Color.Transparent;
            fgColor = AccentGold;
            borderColor = AccentGold;
        }

        // Draw background
        if (bgColor != Color.Transparent)
        {
            using var bgBrush = new SolidBrush(bgColor);
            g.FillPath(bgBrush, path);
        }

        // Draw border
        using var borderPen = new Pen(borderColor, 1);
        g.DrawPath(borderPen, path);

        // Draw text with icon
        using var textBrush = new SolidBrush(fgColor);
        var textRect = new Rectangle(0, 0, Width, Height);
        
        var format = new StringFormat
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center
        };

        g.DrawString(Text, Font, textBrush, textRect, format);

        path.Dispose();
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
}

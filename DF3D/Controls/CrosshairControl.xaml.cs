using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using DF3D.Models;

namespace DF3D.Controls;

/// <summary>
/// Renders the center crosshair overlay.
/// Supports Circle, Cross, and Diamond shapes with configurable size, thickness, color, and opacity.
/// </summary>
public partial class CrosshairControl : UserControl
{
    private CrosshairSettings? _settings;

    public CrosshairControl()
    {
        InitializeComponent();
        SizeChanged += (_, _) => Render();
    }

    /// <summary>
    /// Applies new settings and triggers a re-render.
    /// </summary>
    public void ApplySettings(CrosshairSettings settings)
    {
        if (_settings is not null)
            _settings.PropertyChanged -= OnSettingsPropertyChanged;

        _settings = settings;
        _settings.PropertyChanged += OnSettingsPropertyChanged;
        Render();
    }

    private void OnSettingsPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        Render();
    }

    /// <summary>
    /// Rebuilds the crosshair visual from current settings and canvas size.
    /// </summary>
    public void Render()
    {
        DrawCanvas.Children.Clear();
        if (_settings is null || ActualWidth <= 0 || ActualHeight <= 0) return;

        var w = ActualWidth;
        var h = ActualHeight;

        // Center point with offset
        var cx = w / 2 + _settings.OffsetX;
        var cy = h / 2 + _settings.OffsetY;

        // Adjust for split screen mode
        if (_settings.SplitScreen == SplitScreenMode.Vertical)
            cx = w / 4 + _settings.OffsetX; // Place in left half
        else if (_settings.SplitScreen == SplitScreenMode.Horizontal)
            cy = h / 4 + _settings.OffsetY; // Place in top half

        var radius = _settings.GetRadiusDip();
        var thickness = _settings.Thickness;
        var brush = _settings.GetBrush();
        brush.Opacity = _settings.Opacity;

        switch (_settings.Shape)
        {
            case CrosshairShape.Circle:
                RenderCircle(cx, cy, radius, thickness, brush);
                break;
            case CrosshairShape.Cross:
                RenderCross(cx, cy, radius, thickness, brush);
                break;
            case CrosshairShape.Diamond:
                RenderDiamond(cx, cy, radius, thickness, brush);
                break;
        }

        // If split screen, also render a second crosshair in the other half
        if (_settings.SplitScreen != SplitScreenMode.None)
        {
            double cx2, cy2;
            if (_settings.SplitScreen == SplitScreenMode.Vertical)
            {
                cx2 = w * 3 / 4 + _settings.OffsetX;
                cy2 = cy;
            }
            else
            {
                cx2 = cx;
                cy2 = h * 3 / 4 + _settings.OffsetY;
            }

            switch (_settings.Shape)
            {
                case CrosshairShape.Circle:
                    RenderCircle(cx2, cy2, radius, thickness, brush);
                    break;
                case CrosshairShape.Cross:
                    RenderCross(cx2, cy2, radius, thickness, brush);
                    break;
                case CrosshairShape.Diamond:
                    RenderDiamond(cx2, cy2, radius, thickness, brush);
                    break;
            }
        }
    }

    /// <summary>
    /// Circle: a ring with a center dot.
    /// </summary>
    private void RenderCircle(double cx, double cy, double radius, double thickness, Brush brush)
    {
        // Outer ring
        var ring = new Ellipse
        {
            Width = radius * 2,
            Height = radius * 2,
            Stroke = brush,
            StrokeThickness = thickness,
            Fill = Brushes.Transparent
        };
        Canvas.SetLeft(ring, cx - radius);
        Canvas.SetTop(ring, cy - radius);
        DrawCanvas.Children.Add(ring);

        // Center dot
        var dotSize = Math.Max(2, thickness);
        var dot = new Ellipse
        {
            Width = dotSize,
            Height = dotSize,
            Fill = brush
        };
        Canvas.SetLeft(dot, cx - dotSize / 2);
        Canvas.SetTop(dot, cy - dotSize / 2);
        DrawCanvas.Children.Add(dot);
    }

    /// <summary>
    /// Cross: two perpendicular lines with a center gap.
    /// </summary>
    private void RenderCross(double cx, double cy, double radius, double thickness, Brush brush)
    {
        var gap = thickness * 1.5; // Small gap at center

        // Top line
        AddLine(cx, cy - radius, cx, cy - gap, thickness, brush);
        // Bottom line
        AddLine(cx, cy + gap, cx, cy + radius, thickness, brush);
        // Left line
        AddLine(cx - radius, cy, cx - gap, cy, thickness, brush);
        // Right line
        AddLine(cx + gap, cy, cx + radius, cy, thickness, brush);

        // Center dot
        var dotSize = Math.Max(2, thickness * 0.8);
        var dot = new Ellipse
        {
            Width = dotSize,
            Height = dotSize,
            Fill = brush
        };
        Canvas.SetLeft(dot, cx - dotSize / 2);
        Canvas.SetTop(dot, cy - dotSize / 2);
        DrawCanvas.Children.Add(dot);
    }

    /// <summary>
    /// Diamond: a rotated square outline with a center dot.
    /// </summary>
    private void RenderDiamond(double cx, double cy, double radius, double thickness, Brush brush)
    {
        var diamond = new Polygon
        {
            Stroke = brush,
            StrokeThickness = thickness,
            Fill = Brushes.Transparent,
            Points = new PointCollection
            {
                new(cx, cy - radius),      // Top
                new(cx + radius, cy),      // Right
                new(cx, cy + radius),      // Bottom
                new(cx - radius, cy)       // Left
            }
        };
        DrawCanvas.Children.Add(diamond);

        // Center dot
        var dotSize = Math.Max(2, thickness);
        var dot = new Ellipse
        {
            Width = dotSize,
            Height = dotSize,
            Fill = brush
        };
        Canvas.SetLeft(dot, cx - dotSize / 2);
        Canvas.SetTop(dot, cy - dotSize / 2);
        DrawCanvas.Children.Add(dot);
    }

    private void AddLine(double x1, double y1, double x2, double y2, double thickness, Brush brush)
    {
        var line = new Line
        {
            X1 = x1, Y1 = y1,
            X2 = x2, Y2 = y2,
            Stroke = brush,
            StrokeThickness = thickness,
            StrokeStartLineCap = PenLineCap.Round,
            StrokeEndLineCap = PenLineCap.Round
        };
        DrawCanvas.Children.Add(line);
    }
}

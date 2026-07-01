using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using DF3D.Models;

namespace DF3D.Controls;

/// <summary>
/// Renders the vignette (edge mask) overlay using WPF shapes with gradient brushes.
/// Supports Box, Dome, and Flag shapes with configurable size, length, color, and opacity.
/// </summary>
public partial class VignetteControl : UserControl
{
    private OverlaySettings? _settings;

    public VignetteControl()
    {
        InitializeComponent();
        SizeChanged += (_, _) => Render();
    }

    /// <summary>
    /// Applies new settings and triggers a re-render.
    /// </summary>
    public void ApplySettings(OverlaySettings settings)
    {
        // Unsubscribe from old settings
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
    /// Rebuilds the vignette visual from current settings and canvas size.
    /// </summary>
    public void Render()
    {
        DrawCanvas.Children.Clear();
        if (_settings is null || ActualWidth <= 0 || ActualHeight <= 0) return;

        var w = ActualWidth;
        var h = ActualHeight;
        var brush = _settings.GetBrush();
        brush.Opacity = _settings.Opacity;

        var inset = Math.Min(w, h) * _settings.GetInsetFraction();
        var spread = inset * _settings.GetSpreadFraction();
        var innerInset = inset - spread;

        switch (_settings.Shape)
        {
            case VignetteShape.Box:
                RenderBoxVignette(w, h, brush, innerInset, spread);
                break;
            case VignetteShape.Dome:
                RenderDomeVignette(w, h, brush, innerInset, spread);
                break;
            case VignetteShape.Flag:
                RenderFlagVignette(w, h, brush, innerInset, spread);
                break;
        }

        // Apply split screen masking
        if (_settings.SplitScreen != SplitScreenMode.None)
        {
            ApplySplitScreenMask(w, h, _settings.SplitScreen);
        }
    }

    /// <summary>
    /// Box vignette: four gradient rectangles on each edge, plus four gradient corners.
    /// </summary>
    private void RenderBoxVignette(double w, double h, Brush baseBrush, double innerInset, double spread)
    {
        var color = GetSolidColor(baseBrush);
        var opacity = _settings!.Opacity;

        // Full edge gradients (solid at edge -> transparent inward)
        // Top edge
        AddRect(0, 0, w, innerInset + spread,
            CreateVerticalGradient(color, opacity, 0, 1, innerInset / (innerInset + spread)));

        // Bottom edge
        AddRect(0, h - innerInset - spread, w, innerInset + spread,
            CreateVerticalGradient(color, opacity, 1, 0, innerInset / (innerInset + spread)));

        // Left edge
        AddRect(0, 0, innerInset + spread, h,
            CreateHorizontalGradient(color, opacity, 0, 1, innerInset / (innerInset + spread)));

        // Right edge
        AddRect(w - innerInset - spread, 0, innerInset + spread, h,
            CreateHorizontalGradient(color, opacity, 1, 0, innerInset / (innerInset + spread)));

        // Corner fills (solid)
        var cornerBrush = new SolidColorBrush(color) { Opacity = opacity };
        // Top-left
        AddRect(0, 0, innerInset, innerInset, cornerBrush);
        // Top-right
        AddRect(w - innerInset, 0, innerInset, innerInset, cornerBrush);
        // Bottom-left
        AddRect(0, h - innerInset, innerInset, innerInset, cornerBrush);
        // Bottom-right
        AddRect(w - innerInset, h - innerInset, innerInset, innerInset, cornerBrush);
    }

    /// <summary>
    /// Dome vignette: an elliptical radial gradient from center (transparent) to edges (opaque).
    /// </summary>
    private void RenderDomeVignette(double w, double h, Brush baseBrush, double innerInset, double spread)
    {
        var color = GetSolidColor(baseBrush);
        var opacity = _settings!.Opacity;

        // The gradient goes from transparent at center to opaque at the ellipse edge
        var gradient = new RadialGradientBrush
        {
            Center = new Point(0.5, 0.5),
            RadiusX = 0.5,
            RadiusY = 0.5,
            GradientStops = new GradientStopCollection
            {
                new GradientStop(Color.FromArgb(0, color.R, color.G, color.B), 0.0),
                new GradientStop(Color.FromArgb(0, color.R, color.G, color.B),
                    Math.Max(0, 1.0 - (innerInset + spread) / (Math.Min(w, h) * 0.5))),
                new GradientStop(Color.FromArgb((byte)(opacity * 255), color.R, color.G, color.B), 1.0)
            }
        };

        var rect = new Rectangle
        {
            Width = w,
            Height = h,
            Fill = gradient
        };
        Canvas.SetLeft(rect, 0);
        Canvas.SetTop(rect, 0);
        DrawCanvas.Children.Add(rect);
    }

    /// <summary>
    /// Flag vignette: similar to box but with tapered/pointed corners for a "flag" feel.
    /// Top and bottom edges are wider, side edges taper inward.
    /// </summary>
    private void RenderFlagVignette(double w, double h, Brush baseBrush, double innerInset, double spread)
    {
        var color = GetSolidColor(baseBrush);
        var opacity = _settings!.Opacity;

        // Top edge (full width, tapered)
        AddRect(0, 0, w, innerInset + spread,
            CreateVerticalGradient(color, opacity, 0, 1, innerInset / (innerInset + spread)));

        // Bottom edge (full width, tapered)
        AddRect(0, h - innerInset - spread, w, innerInset + spread,
            CreateVerticalGradient(color, opacity, 1, 0, innerInset / (innerInset + spread)));

        // Left edge (shorter, tapered from center)
        var sideTop = h * 0.2;
        var sideHeight = h * 0.6;
        AddRect(0, sideTop, innerInset + spread, sideHeight,
            CreateHorizontalGradient(color, opacity, 0, 1, innerInset / (innerInset + spread)));

        // Right edge (shorter, tapered from center)
        AddRect(w - innerInset - spread, sideTop, innerInset + spread, sideHeight,
            CreateHorizontalGradient(color, opacity, 1, 0, innerInset / (innerInset + spread)));

        // Corner fills (top and bottom full, sides partial)
        var cornerBrush = new SolidColorBrush(color) { Opacity = opacity };
        AddRect(0, 0, innerInset, innerInset * 0.6, cornerBrush);
        AddRect(w - innerInset, 0, innerInset, innerInset * 0.6, cornerBrush);
        AddRect(0, h - innerInset * 0.6, innerInset, innerInset * 0.6, cornerBrush);
        AddRect(w - innerInset, h - innerInset * 0.6, innerInset, innerInset * 0.6, cornerBrush);
    }

    private void ApplySplitScreenMask(double w, double h, SplitScreenMode mode)
    {
        // Add a thin transparent gap in the center to create a "split screen" reference line
        var gapThickness = 2.0;
        var gapBrush = new SolidColorBrush(Colors.Transparent);

        if (mode == SplitScreenMode.Vertical)
        {
            // Vertical split: clear a vertical strip at center
            var gap = new Rectangle
            {
                Width = gapThickness,
                Height = h,
                Fill = gapBrush
            };
            Canvas.SetLeft(gap, w / 2 - gapThickness / 2);
            Canvas.SetTop(gap, 0);
            DrawCanvas.Children.Add(gap);
        }
        else if (mode == SplitScreenMode.Horizontal)
        {
            // Horizontal split: clear a horizontal strip at center
            var gap = new Rectangle
            {
                Width = w,
                Height = gapThickness,
                Fill = gapBrush
            };
            Canvas.SetLeft(gap, 0);
            Canvas.SetTop(gap, h / 2 - gapThickness / 2);
            DrawCanvas.Children.Add(gap);
        }
    }

    #region Helpers

    private void AddRect(double x, double y, double width, double height, Brush brush)
    {
        if (width <= 0 || height <= 0) return;
        var rect = new Rectangle
        {
            Width = width,
            Height = height,
            Fill = brush
        };
        Canvas.SetLeft(rect, x);
        Canvas.SetTop(rect, y);
        DrawCanvas.Children.Add(rect);
    }

    private static System.Windows.Media.Color GetSolidColor(Brush brush)
    {
        return brush is SolidColorBrush scb ? scb.Color : Colors.Black;
    }

    private static LinearGradientBrush CreateVerticalGradient(
        System.Windows.Media.Color color, double opacity,
        double startOffset, double endOffset, double transparentStop)
    {
        return new LinearGradientBrush
        {
            StartPoint = new Point(0, startOffset),
            EndPoint = new Point(0, endOffset),
            GradientStops = new GradientStopCollection
            {
                new(Color.FromArgb((byte)(opacity * 255), color.R, color.G, color.B), 0),
                new(Color.FromArgb((byte)(opacity * 255), color.R, color.G, color.B), transparentStop),
                new(Color.FromArgb(0, color.R, color.G, color.B), 1)
            }
        };
    }

    private static LinearGradientBrush CreateHorizontalGradient(
        System.Windows.Media.Color color, double opacity,
        double startOffset, double endOffset, double transparentStop)
    {
        return new LinearGradientBrush
        {
            StartPoint = new Point(startOffset, 0),
            EndPoint = new Point(endOffset, 0),
            GradientStops = new GradientStopCollection
            {
                new(Color.FromArgb((byte)(opacity * 255), color.R, color.G, color.B), 0),
                new(Color.FromArgb((byte)(opacity * 255), color.R, color.G, color.B), transparentStop),
                new(Color.FromArgb(0, color.R, color.G, color.B), 1)
            }
        };
    }

    #endregion
}

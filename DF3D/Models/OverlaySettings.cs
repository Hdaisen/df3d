using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace DF3D.Models;

/// <summary>
/// Settings for the vignette (edge mask) overlay.
/// </summary>
public class OverlaySettings : INotifyPropertyChanged
{
    private bool _isVisible = true;
    private VignetteShape _shape = VignetteShape.Box;
    private AspectRatio _aspectRatio = AspectRatio.R16x9;
    private VignetteSize _size = VignetteSize.M;
    private VignetteLength _length = VignetteLength.Plus2;
    private DisplayMode _displayMode = DisplayMode.Stretch;
    private SplitScreenMode _splitScreen = SplitScreenMode.None;
    private OverlayColor _color = OverlayColor.Black;
    private string _customColorHex = "#000000";
    private double _opacity = 0.7;

    public bool IsVisible
    {
        get => _isVisible;
        set => SetField(ref _isVisible, value);
    }

    public VignetteShape Shape
    {
        get => _shape;
        set => SetField(ref _shape, value);
    }

    public AspectRatio AspectRatio
    {
        get => _aspectRatio;
        set => SetField(ref _aspectRatio, value);
    }

    public VignetteSize Size
    {
        get => _size;
        set => SetField(ref _size, value);
    }

    public VignetteLength Length
    {
        get => _length;
        set => SetField(ref _length, value);
    }

    public DisplayMode DisplayMode
    {
        get => _displayMode;
        set => SetField(ref _displayMode, value);
    }

    public SplitScreenMode SplitScreen
    {
        get => _splitScreen;
        set => SetField(ref _splitScreen, value);
    }

    public OverlayColor Color
    {
        get => _color;
        set => SetField(ref _color, value);
    }

    public string CustomColorHex
    {
        get => _customColorHex;
        set => SetField(ref _customColorHex, value);
    }

    public double Opacity
    {
        get => _opacity;
        set => SetField(ref _opacity, Math.Clamp(value, 0, 1));
    }

    /// <summary>
    /// Gets the resolved color brush based on the current color setting.
    /// </summary>
    public SolidColorBrush GetBrush()
    {
        return Color switch
        {
            OverlayColor.Black   => new SolidColorBrush(Colors.Black),
            OverlayColor.Red     => new SolidColorBrush(Colors.Red),
            OverlayColor.Green   => new SolidColorBrush(ColorFromHex("#00AA00")),
            OverlayColor.Yellow  => new SolidColorBrush(ColorFromHex("#FFD700")),
            OverlayColor.Custom  => new SolidColorBrush(ColorFromHex(_customColorHex)),
            _ => new SolidColorBrush(Colors.Black)
        };
    }

    private static System.Windows.Media.Color ColorFromHex(string hex)
    {
        try
        {
            return (System.Windows.Media.Color)ColorConverter.ConvertFromString(hex);
        }
        catch
        {
            return Colors.Black;
        }
    }

    /// <summary>
    /// Returns the vignette inset as a fraction of the shorter screen dimension.
    /// </summary>
    public double GetInsetFraction()
    {
        return Size switch
        {
            VignetteSize.XXS  => 0.02,
            VignetteSize.XS   => 0.05,
            VignetteSize.S    => 0.10,
            VignetteSize.M    => 0.15,
            VignetteSize.L    => 0.22,
            VignetteSize.XL   => 0.30,
            VignetteSize.XXL  => 0.40,
            _ => 0.15
        };
    }

    /// <summary>
    /// Returns the vignette gradient spread as a fraction of the inset.
    /// </summary>
    public double GetSpreadFraction()
    {
        return Length switch
        {
            VignetteLength.Plus0 => 0.0,
            VignetteLength.Plus1 => 0.15,
            VignetteLength.Plus2 => 0.30,
            VignetteLength.Plus3 => 0.50,
            VignetteLength.Plus4 => 0.70,
            VignetteLength.Plus5 => 0.90,
            VignetteLength.Plus6 => 1.0,
            _ => 0.30
        };
    }

    public OverlaySettings Clone()
    {
        return new OverlaySettings
        {
            IsVisible = IsVisible,
            Shape = Shape,
            AspectRatio = AspectRatio,
            Size = Size,
            Length = Length,
            DisplayMode = DisplayMode,
            SplitScreen = SplitScreen,
            Color = Color,
            CustomColorHex = CustomColorHex,
            Opacity = Opacity
        };
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (!EqualityComparer<T>.Default.Equals(field, value))
        {
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

public enum VignetteShape { Box, Dome, Flag }
public enum AspectRatio { R16x9, R21x9 }
public enum VignetteSize { XXS, XS, S, M, L, XL, XXL }
public enum VignetteLength { Plus0, Plus1, Plus2, Plus3, Plus4, Plus5, Plus6 }
public enum DisplayMode { Window, Stretch }
public enum SplitScreenMode { None, Vertical, Horizontal }
public enum OverlayColor { Black, Red, Green, Yellow, Custom }

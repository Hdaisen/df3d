using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace DF3D.Models;

/// <summary>
/// Settings for the center crosshair overlay.
/// </summary>
public class CrosshairSettings : INotifyPropertyChanged
{
    private bool _isVisible = true;
    private CrosshairShape _shape = CrosshairShape.Cross;
    private AspectRatio _aspectRatio = AspectRatio.R16x9;
    private CrosshairSize _size = CrosshairSize.M;
    private int _thickness = 2;
    private double _offsetX;
    private double _offsetY;
    private SplitScreenMode _splitScreen = SplitScreenMode.None;
    private OverlayColor _color = OverlayColor.Green;
    private string _customColorHex = "#00FF00";
    private double _opacity = 0.8;

    public bool IsVisible
    {
        get => _isVisible;
        set => SetField(ref _isVisible, value);
    }

    public CrosshairShape Shape
    {
        get => _shape;
        set => SetField(ref _shape, value);
    }

    public AspectRatio AspectRatio
    {
        get => _aspectRatio;
        set => SetField(ref _aspectRatio, value);
    }

    public CrosshairSize Size
    {
        get => _size;
        set => SetField(ref _size, value);
    }

    public int Thickness
    {
        get => _thickness;
        set => SetField(ref _thickness, Math.Clamp(value, 1, 8));
    }

    public double OffsetX
    {
        get => _offsetX;
        set => SetField(ref _offsetX, value);
    }

    public double OffsetY
    {
        get => _offsetY;
        set => SetField(ref _offsetY, value);
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

    public SolidColorBrush GetBrush()
    {
        return Color switch
        {
            OverlayColor.Black  => new SolidColorBrush(Colors.White),
            OverlayColor.Red    => new SolidColorBrush(Colors.Red),
            OverlayColor.Green  => new SolidColorBrush(ColorFromHex("#00FF00")),
            OverlayColor.Yellow => new SolidColorBrush(ColorFromHex("#FFD700")),
            OverlayColor.Custom => new SolidColorBrush(ColorFromHex(_customColorHex)),
            _ => new SolidColorBrush(Colors.Green)
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
            return Colors.Green;
        }
    }

    /// <summary>
    /// Returns the crosshair radius in device-independent pixels.
    /// </summary>
    public double GetRadiusDip()
    {
        return Size switch
        {
            CrosshairSize.XXS => 4,
            CrosshairSize.XS  => 8,
            CrosshairSize.S   => 14,
            CrosshairSize.M   => 20,
            CrosshairSize.L   => 30,
            CrosshairSize.XL  => 42,
            CrosshairSize.XXL => 56,
            _ => 20
        };
    }

    public CrosshairSettings Clone()
    {
        return new CrosshairSettings
        {
            IsVisible = IsVisible,
            Shape = Shape,
            AspectRatio = AspectRatio,
            Size = Size,
            Thickness = Thickness,
            OffsetX = OffsetX,
            OffsetY = OffsetY,
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

public enum CrosshairShape { Circle, Cross, Diamond }
public enum CrosshairSize { XXS, XS, S, M, L, XL, XXL }

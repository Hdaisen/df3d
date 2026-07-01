using System.Windows;
using System.Windows.Interop;
using DF3D.Helpers;
using DF3D.Models;
using DF3D.Services;

namespace DF3D.Windows;

/// <summary>
/// The transparent fullscreen overlay window that renders vignette and crosshair.
/// </summary>
public partial class OverlayWindow : Window
{
    private readonly SettingsService _settingsService;
    private readonly HotkeyService _hotkeyService;
    private readonly TopmostService _topmostService;
    private readonly MonitorService _monitorService;
    private readonly TrayIconService _trayIconService;

    public OverlayWindow(
        SettingsService settingsService,
        HotkeyService hotkeyService,
        TopmostService topmostService,
        MonitorService monitorService,
        TrayIconService trayIconService)
    {
        _settingsService = settingsService;
        _hotkeyService = hotkeyService;
        _topmostService = topmostService;
        _monitorService = monitorService;
        _trayIconService = trayIconService;

        InitializeComponent();

        // Set initial window position to cover the entire primary screen
        Left = 0;
        Top = 0;
        Width = SystemParameters.PrimaryScreenWidth;
        Height = SystemParameters.PrimaryScreenHeight;

        // Wire up settings changes to re-render
        _settingsService.SettingsChanged += OnSettingsChanged;
        _settingsService.Settings.Overlay.PropertyChanged += (_, _) => ApplyOverlaySettings();
        _settingsService.Settings.Crosshair.PropertyChanged += (_, _) => ApplyCrosshairSettings();

        // Wire up hotkey actions
        _hotkeyService.ToggleOverlayRequested     += ToggleOverlay;
        _hotkeyService.ToggleCrosshairRequested   += ToggleCrosshair;
        _hotkeyService.ToggleSplitScreenRequested += ToggleSplitScreen;
        _hotkeyService.ToggleDisplayModeRequested += ToggleDisplayMode;
        _hotkeyService.CycleOverlaySizeRequested  += CycleOverlaySize;
        _hotkeyService.CycleProfileRequested      += CycleProfile;

        // Monitor changes
        _monitorService.MonitorChanged += () => ApplyOverlaySettings();
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);

        // Set click-through and tool window styles
        Win32Helper.SetOverlayStyle(this);

        // Initialize services that need the window handle
        _hotkeyService.Initialize(this);
        _hotkeyService.RegisterAll(this, _settingsService.Settings.Hotkeys);
        _topmostService.Start(this);
        _monitorService.Start(this);
        _monitorService.FitToMonitor();

        // Initial render
        ApplyOverlaySettings();
        ApplyCrosshairSettings();

        // Set initial visibility
        Vignette.Visibility = _settingsService.Settings.Overlay.IsVisible
            ? Visibility.Visible : Visibility.Collapsed;
        Crosshair.Visibility = _settingsService.Settings.Crosshair.IsVisible
            ? Visibility.Visible : Visibility.Collapsed;
    }

    private void OnSettingsChanged()
    {
        ApplyOverlaySettings();
        ApplyCrosshairSettings();
    }

    private void ApplyOverlaySettings()
    {
        var settings = _settingsService.Settings.Overlay;
        Vignette.ApplySettings(settings);
        Vignette.Visibility = settings.IsVisible ? Visibility.Visible : Visibility.Collapsed;
    }

    private void ApplyCrosshairSettings()
    {
        var settings = _settingsService.Settings.Crosshair;
        Crosshair.ApplySettings(settings);
        Crosshair.Visibility = settings.IsVisible ? Visibility.Visible : Visibility.Collapsed;
    }

    #region Hotkey Actions

    private void ToggleOverlay()
    {
        var s = _settingsService.Settings.Overlay;
        s.IsVisible = !s.IsVisible;
        _settingsService.Save();
        _trayIconService.ShowNotification("DF3D",
            s.IsVisible ? "叠加层已显示" : "叠加层已隐藏", 1500);
    }

    private void ToggleCrosshair()
    {
        var s = _settingsService.Settings.Crosshair;
        s.IsVisible = !s.IsVisible;
        _settingsService.Save();
        _trayIconService.ShowNotification("DF3D",
            s.IsVisible ? "十字线已显示" : "十字线已隐藏", 1500);
    }

    private void ToggleSplitScreen()
    {
        var current = _settingsService.Settings.Overlay.SplitScreen;
        var next = current switch
        {
            SplitScreenMode.None       => SplitScreenMode.Vertical,
            SplitScreenMode.Vertical   => SplitScreenMode.Horizontal,
            SplitScreenMode.Horizontal => SplitScreenMode.None,
            _ => SplitScreenMode.None
        };
        _settingsService.Settings.Overlay.SplitScreen = next;
        _settingsService.Settings.Crosshair.SplitScreen = next;
        _settingsService.Save();

        var label = next switch
        {
            SplitScreenMode.None       => "关闭",
            SplitScreenMode.Vertical   => "垂直",
            SplitScreenMode.Horizontal => "水平",
            _ => "关闭"
        };
        _trayIconService.ShowNotification("DF3D", $"分屏模式: {label}", 1500);
    }

    private void ToggleDisplayMode()
    {
        var s = _settingsService.Settings.Overlay;
        s.DisplayMode = s.DisplayMode == DisplayMode.Window
            ? DisplayMode.Stretch
            : DisplayMode.Window;
        _settingsService.Save();

        _trayIconService.ShowNotification("DF3D",
            s.DisplayMode == DisplayMode.Window ? "窗口模式" : "拉伸模式", 1500);
    }

    private void CycleOverlaySize()
    {
        var s = _settingsService.Settings.Overlay;
        var values = Enum.GetValues<VignetteSize>();
        var index = Array.IndexOf(values, s.Size);
        s.Size = values[(index + 1) % values.Length];
        _settingsService.Save();

        _trayIconService.ShowNotification("DF3D", $"遮罩大小: {s.Size}", 1500);
    }

    private void CycleProfile()
    {
        // This will be handled by ProfileService via App.xaml.cs
        // For now, just notify
    }

    #endregion

    protected override void OnClosed(EventArgs e)
    {
        _settingsService.SettingsChanged -= OnSettingsChanged;
        _hotkeyService.ToggleOverlayRequested     -= ToggleOverlay;
        _hotkeyService.ToggleCrosshairRequested   -= ToggleCrosshair;
        _hotkeyService.ToggleSplitScreenRequested -= ToggleSplitScreen;
        _hotkeyService.ToggleDisplayModeRequested -= ToggleDisplayMode;
        _hotkeyService.CycleOverlaySizeRequested  -= CycleOverlaySize;
        _hotkeyService.CycleProfileRequested      -= CycleProfile;

        _hotkeyService.UnregisterAll(this);
        _topmostService.Stop();

        base.OnClosed(e);
    }
}

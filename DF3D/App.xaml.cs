using System.Windows;
using System.Windows.Controls;
using DF3D.Helpers;
using DF3D.Services;
using DF3D.Windows;

namespace DF3D;

public partial class App : Application
{
    private SingleInstanceHelper? _singleInstance;
    private SettingsService _settingsService = null!;
    private ProfileService _profileService = null!;
    private HotkeyService _hotkeyService = null!;
    private TopmostService _topmostService = null!;
    private MonitorService _monitorService = null!;
    private TrayIconService _trayIconService = null!;

    private OverlayWindow? _overlayWindow;
    private SettingsWindow? _settingsWindow;

    private void OnStartup(object sender, StartupEventArgs e)
    {
        // Single instance check
        _singleInstance = new SingleInstanceHelper();
        if (!_singleInstance.IsFirstInstance)
        {
            MessageBox.Show("DF3D 已经在运行中。请检查系统托盘。", "DF3D",
                MessageBoxButton.OK, MessageBoxImage.Information);
            Shutdown();
            return;
        }

        // Initialize services
        _settingsService = new SettingsService();
        _settingsService.Load();

        _profileService = new ProfileService(_settingsService);
        _hotkeyService = new HotkeyService();
        _topmostService = new TopmostService();
        _monitorService = new MonitorService();
        _trayIconService = new TrayIconService();

        // Initialize tray icon
        var trayIcon = (H.NotifyIcon.TaskbarIcon)FindResource("TrayIcon");
        trayIcon.ForceCreate(); // Ensure the tray icon is created before use
        _trayIconService.Initialize(trayIcon);

        // Set initial tray menu state
        UpdateTrayMenuState();

        // Create and show the overlay window
        _overlayWindow = new OverlayWindow(
            _settingsService,
            _hotkeyService,
            _topmostService,
            _monitorService,
            _trayIconService);

        // Show overlay based on settings
        if (_settingsService.Settings.ShowOverlayOnStartup ||
            _settingsService.Settings.ShowCrosshairOnStartup)
        {
            _overlayWindow.Show();
        }

        // Delay notification to ensure tray icon is fully ready
        Dispatcher.BeginInvoke(() =>
        {
            try { _trayIconService.ShowNotification("DF3D", "已启动，可在托盘图标中管理。", 2000); }
            catch { /* Tray icon may not be ready yet */ }
        }, System.Windows.Threading.DispatcherPriority.ApplicationIdle);
    }

    #region Tray Menu Handlers

    private void OnTrayShowSettings(object sender, RoutedEventArgs e)
    {
        ShowSettingsWindow();
    }

    private void OnTrayToggleOverlay(object sender, RoutedEventArgs e)
    {
        var s = _settingsService.Settings.Overlay;
        s.IsVisible = !s.IsVisible;
        _settingsService.Save();
        UpdateTrayMenuState();
        _trayIconService.ShowNotification("DF3D",
            s.IsVisible ? "叠加层已显示" : "叠加层已隐藏", 1500);
    }

    private void OnTrayToggleCrosshair(object sender, RoutedEventArgs e)
    {
        var s = _settingsService.Settings.Crosshair;
        s.IsVisible = !s.IsVisible;
        _settingsService.Save();
        UpdateTrayMenuState();
        _trayIconService.ShowNotification("DF3D",
            s.IsVisible ? "十字线已显示" : "十字线已隐藏", 1500);
    }

    private void OnTrayExit(object sender, RoutedEventArgs e)
    {
        PerformShutdown();
    }

    #endregion

    private void ShowSettingsWindow()
    {
        if (_settingsWindow is { IsVisible: true })
        {
            _settingsWindow.Activate();
            return;
        }

        _settingsWindow = new SettingsWindow(_settingsService, _profileService);
        _settingsWindow.Show();
        _settingsWindow.Activate();
    }

    private void UpdateTrayMenuState()
    {
        if (FindResource("TrayIcon") is H.NotifyIcon.TaskbarIcon trayIcon &&
            trayIcon.ContextMenu is { } menu)
        {
            var overlayItem = menu.Items[2] as MenuItem;  // TrayMenuOverlay
            var crosshairItem = menu.Items[3] as MenuItem; // TrayMenuCrosshair

            if (overlayItem is not null)
                overlayItem.Header = _settingsService.Settings.Overlay.IsVisible ? "✅ 叠加层" : "⬜ 叠加层";
            if (crosshairItem is not null)
                crosshairItem.Header = _settingsService.Settings.Crosshair.IsVisible ? "✅ 十字线" : "⬜ 十字线";
        }
    }

    private void PerformShutdown()
    {
        // Save settings immediately
        _settingsService.SaveImmediate();

        // Dispose services
        _hotkeyService.Dispose();
        _topmostService.Dispose();
        _monitorService.Dispose();
        _trayIconService.Dispose();
        _singleInstance?.Dispose();

        // Close windows
        _overlayWindow?.Close();
        _settingsWindow?.Close();

        Shutdown();
    }
}

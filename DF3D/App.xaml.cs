using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using DF3D.Helpers;
using DF3D.Services;
using DF3D.Windows;
using H.NotifyIcon;

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

    private TaskbarIcon? _taskbarIcon;
    private OverlayWindow? _overlayWindow;
    private SettingsWindow? _settingsWindow;

    public App()
    {
        // Global exception handler
        DispatcherUnhandledException += (_, e) =>
        {
            MessageBox.Show(e.Exception.ToString(), "DF3D 未处理异常",
                MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        };

        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            var ex = e.ExceptionObject as Exception;
            MessageBox.Show(ex?.ToString() ?? "Unknown error", "DF3D 域异常",
                MessageBoxButton.OK, MessageBoxImage.Error);
        };

        TaskScheduler.UnobservedTaskException += (_, e) =>
        {
            MessageBox.Show(e.Exception.ToString(), "DF3D 任务异常",
                MessageBoxButton.OK, MessageBoxImage.Error);
            e.SetObserved();
        };
    }

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

        // Create tray icon programmatically
        CreateTrayIcon();

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
    }

    private void CreateTrayIcon()
    {
        _taskbarIcon = new TaskbarIcon();

        // Load icon from embedded resource
        try
        {
            var iconUri = new Uri("pack://application:,,,/Resources/Assets/tray-icon.ico", UriKind.Absolute);
            var iconStream = GetResourceStream(iconUri)?.Stream;
            if (iconStream != null)
            {
                _taskbarIcon.Icon = new System.Drawing.Icon(iconStream);
            }
            else
            {
                // Fallback: use system icon
                _taskbarIcon.Icon = System.Drawing.SystemIcons.Application;
            }
        }
        catch
        {
            _taskbarIcon.Icon = System.Drawing.SystemIcons.Application;
        }

        _taskbarIcon.ToolTipText = "DF3D - 3D 晕动症缓解工具";

        // Build context menu
        var menu = new ContextMenu();

        var showSettingsItem = new MenuItem { Header = "打开设置" };
        showSettingsItem.Click += (_, _) =>
        {
            try { ShowSettingsWindow(); }
            catch (Exception ex) { MessageBox.Show(ex.ToString(), "DF3D 错误", MessageBoxButton.OK, MessageBoxImage.Error); }
        };
        menu.Items.Add(showSettingsItem);

        menu.Items.Add(new Separator());

        var overlayItem = new MenuItem
        {
            Header = _settingsService.Settings.Overlay.IsVisible ? "✅ 叠加层" : "⬜ 叠加层"
        };
        overlayItem.Click += (_, _) =>
        {
            var s = _settingsService.Settings.Overlay;
            s.IsVisible = !s.IsVisible;
            _settingsService.Save();
            overlayItem.Header = s.IsVisible ? "✅ 叠加层" : "⬜ 叠加层";
        };
        menu.Items.Add(overlayItem);

        var crosshairItem = new MenuItem
        {
            Header = _settingsService.Settings.Crosshair.IsVisible ? "✅ 十字线" : "⬜ 十字线"
        };
        crosshairItem.Click += (_, _) =>
        {
            var s = _settingsService.Settings.Crosshair;
            s.IsVisible = !s.IsVisible;
            _settingsService.Save();
            crosshairItem.Header = s.IsVisible ? "✅ 十字线" : "⬜ 十字线";
        };
        menu.Items.Add(crosshairItem);

        menu.Items.Add(new Separator());

        var exitItem = new MenuItem { Header = "退出" };
        exitItem.Click += (_, _) => PerformShutdown();
        menu.Items.Add(exitItem);

        _taskbarIcon.ContextMenu = menu;

        // Double-click to show settings
        _taskbarIcon.DoubleClickCommand = new RelayCommand(() => ShowSettingsWindow());

        // Initialize (show the icon)
        _taskbarIcon.ForceCreate();

        _trayIconService.Initialize(_taskbarIcon);
    }

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

    private void PerformShutdown()
    {
        // Save settings immediately
        _settingsService.SaveImmediate();

        // Dispose services
        _hotkeyService.Dispose();
        _topmostService.Dispose();
        _monitorService.Dispose();
        _trayIconService.Dispose();
        _taskbarIcon?.Dispose();
        _singleInstance?.Dispose();

        // Close windows
        _overlayWindow?.Close();
        _settingsWindow?.Close();

        Shutdown();
    }
}

/// <summary>
/// Simple relay command for tray icon double-click.
/// </summary>
public class RelayCommand : System.Windows.Input.ICommand
{
    private readonly Action _execute;

    public RelayCommand(Action execute) => _execute = execute;

#pragma warning disable CS0067 // Event is never used
    public event EventHandler? CanExecuteChanged;
#pragma warning restore CS0067
    public bool CanExecute(object? parameter) => true;
    public void Execute(object? parameter) => _execute();
}

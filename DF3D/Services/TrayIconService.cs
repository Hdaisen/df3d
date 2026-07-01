using System.Windows;

namespace DF3D.Services;

/// <summary>
/// Manages the system tray icon lifecycle.
/// The actual H.NotifyIcon TaskbarIcon is declared in XAML (App.xaml).
/// This service provides methods to show notifications and control the icon.
/// </summary>
public sealed class TrayIconService
{
    private H.NotifyIcon.TaskbarIcon? _trayIcon;

    /// <summary>
    /// Binds the TrayIcon from XAML to this service.
    /// Called from App.xaml.cs after InitializeComponent.
    /// </summary>
    public void Initialize(H.NotifyIcon.TaskbarIcon trayIcon)
    {
        _trayIcon = trayIcon;
    }

    /// <summary>
    /// Shows a balloon notification in the system tray.
    /// </summary>
    public void ShowNotification(string title, string message, int timeoutMs = 2000)
    {
        _trayIcon?.ShowNotification(title, message);
    }

    /// <summary>
    /// Disposes the tray icon. Must be called on application exit.
    /// </summary>
    public void Dispose()
    {
        _trayIcon?.Dispose();
    }
}

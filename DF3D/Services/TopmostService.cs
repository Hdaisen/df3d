using System.Windows;
using System.Windows.Threading;
using DF3D.Helpers;
using Microsoft.Win32;

namespace DF3D.Services;

/// <summary>
/// Periodically re-asserts the overlay window as topmost,
/// and recovers from session switch / display change events.
/// </summary>
public sealed class TopmostService : IDisposable
{
    private readonly DispatcherTimer _timer;
    private Window? _window;

    public TopmostService()
    {
        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
        _timer.Tick += OnTick;

        SystemEvents.SessionSwitch += OnSessionSwitch;
        SystemEvents.DisplaySettingsChanged += OnDisplaySettingsChanged;
    }

    /// <summary>
    /// Starts the topmost maintenance for the given window.
    /// </summary>
    public void Start(Window window)
    {
        _window = window;
        _timer.Start();
    }

    /// <summary>
    /// Stops the topmost maintenance.
    /// </summary>
    public void Stop()
    {
        _timer.Stop();
    }

    private void OnTick(object? sender, EventArgs e)
    {
        if (_window is { IsLoaded: true })
        {
            Win32Helper.AssertTopmost(_window);
        }
    }

    private void OnSessionSwitch(object? sender, SessionSwitchEventArgs e)
    {
        if (e.Reason == SessionSwitchReason.SessionUnlock && _window is not null)
        {
            // Re-assert after unlock
            Win32Helper.AssertTopmost(_window);
        }
    }

    private void OnDisplaySettingsChanged(object? sender, EventArgs e)
    {
        if (_window is not null)
        {
            // Re-assert after display change (resolution, monitor connect/disconnect)
            Win32Helper.AssertTopmost(_window);
        }
    }

    public void Dispose()
    {
        _timer.Stop();
        SystemEvents.SessionSwitch -= OnSessionSwitch;
        SystemEvents.DisplaySettingsChanged -= OnDisplaySettingsChanged;
    }
}

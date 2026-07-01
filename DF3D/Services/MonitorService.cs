using System.Windows;
using System.Windows.Interop;
using DF3D.Helpers;

namespace DF3D.Services;

/// <summary>
/// Tracks monitor configuration and handles DPI / resolution changes.
/// </summary>
public sealed class MonitorService : IDisposable
{
    private Window? _window;

    public event Action? MonitorChanged;

    /// <summary>
    /// Starts monitoring the given overlay window for DPI and display changes.
    /// </summary>
    public void Start(Window window)
    {
        _window = window;
        var source = HwndSource.FromHwnd(new WindowInteropHelper(window).Handle);
        source?.AddHook(WndProc);
    }

    /// <summary>
    /// Gets the current monitor bounds for the overlay window.
    /// </summary>
    public Rect GetMonitorBounds()
    {
        return _window is not null
            ? Win32Helper.GetMonitorBounds(_window)
            : new Rect(0, 0, SystemParameters.PrimaryScreenWidth, SystemParameters.PrimaryScreenHeight);
    }

    /// <summary>
    /// Gets the current DPI scale factor.
    /// </summary>
    public double GetDpiScale()
    {
        return _window is not null
            ? Win32Helper.GetDpiScale(_window)
            : 1.0;
    }

    /// <summary>
    /// Positions and resizes the overlay window to cover the entire monitor.
    /// </summary>
    public void FitToMonitor()
    {
        if (_window is null) return;
        var bounds = GetMonitorBounds();
        _window.Left = bounds.Left;
        _window.Top = bounds.Top;
        _window.Width = bounds.Width;
        _window.Height = bounds.Height;
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == Win32Helper.WM_DPICHANGED)
        {
            // Use the suggested rectangle from lParam to reposition
            var rect = System.Runtime.InteropServices.Marshal.PtrToStructure<Win32Helper.RECT>(lParam);
            if (_window is not null)
            {
                _window.Left = rect.Left;
                _window.Top = rect.Top;
                _window.Width = rect.Right - rect.Left;
                _window.Height = rect.Bottom - rect.Top;
            }
            MonitorChanged?.Invoke();
            handled = true;
        }
        return IntPtr.Zero;
    }

    public void Dispose()
    {
        // HwndSource hook is removed when the window is closed
    }
}

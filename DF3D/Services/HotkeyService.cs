using System.Windows;
using System.Windows.Interop;
using DF3D.Helpers;
using DF3D.Models;

namespace DF3D.Services;

/// <summary>
/// Manages global hotkey registration and dispatch.
/// </summary>
public sealed class HotkeyService : IDisposable
{
    private const int ID_TOGGLE_OVERLAY     = 1;
    private const int ID_TOGGLE_CROSSHAIR   = 2;
    private const int ID_TOGGLE_SPLITSCREEN = 3;
    private const int ID_TOGGLE_DISPLAYMODE = 4;
    private const int ID_CYCLE_OVERLAY_SIZE = 5;
    private const int ID_CYCLE_PROFILE      = 6;

    private HwndSource? _source;
    private bool _isRegistered;

    public event Action? ToggleOverlayRequested;
    public event Action? ToggleCrosshairRequested;
    public event Action? ToggleSplitScreenRequested;
    public event Action? ToggleDisplayModeRequested;
    public event Action? CycleOverlaySizeRequested;
    public event Action? CycleProfileRequested;

    /// <summary>
    /// Initializes hotkey hooks on the given window.
    /// Must be called after the window handle is created.
    /// </summary>
    public void Initialize(Window window)
    {
        var helper = new WindowInteropHelper(window);
        _source = HwndSource.FromHwnd(helper.Handle);
        _source?.AddHook(WndProc);
    }

    /// <summary>
    /// Registers all hotkeys from the given settings.
    /// Returns a list of hotkeys that failed to register (conflicts).
    /// </summary>
    public List<HotkeyBinding> RegisterAll(Window window, HotkeySettings settings)
    {
        UnregisterAll(window);
        var failures = new List<HotkeyBinding>();
        var hwnd = new WindowInteropHelper(window).Handle;

        if (!TryRegister(hwnd, ID_TOGGLE_OVERLAY, settings.ToggleOverlay))
            failures.Add(settings.ToggleOverlay);
        if (!TryRegister(hwnd, ID_TOGGLE_CROSSHAIR, settings.ToggleCrosshair))
            failures.Add(settings.ToggleCrosshair);
        if (!TryRegister(hwnd, ID_TOGGLE_SPLITSCREEN, settings.ToggleSplitScreen))
            failures.Add(settings.ToggleSplitScreen);
        if (!TryRegister(hwnd, ID_TOGGLE_DISPLAYMODE, settings.ToggleDisplayMode))
            failures.Add(settings.ToggleDisplayMode);
        if (!TryRegister(hwnd, ID_CYCLE_OVERLAY_SIZE, settings.CycleOverlaySize))
            failures.Add(settings.CycleOverlaySize);
        if (!TryRegister(hwnd, ID_CYCLE_PROFILE, settings.CycleProfile))
            failures.Add(settings.CycleProfile);

        _isRegistered = true;
        return failures;
    }

    /// <summary>
    /// Unregisters all hotkeys.
    /// </summary>
    public void UnregisterAll(Window window)
    {
        if (!_isRegistered) return;
        var hwnd = new WindowInteropHelper(window).Handle;

        Win32Helper.UnregisterHotKey(hwnd, ID_TOGGLE_OVERLAY);
        Win32Helper.UnregisterHotKey(hwnd, ID_TOGGLE_CROSSHAIR);
        Win32Helper.UnregisterHotKey(hwnd, ID_TOGGLE_SPLITSCREEN);
        Win32Helper.UnregisterHotKey(hwnd, ID_TOGGLE_DISPLAYMODE);
        Win32Helper.UnregisterHotKey(hwnd, ID_CYCLE_OVERLAY_SIZE);
        Win32Helper.UnregisterHotKey(hwnd, ID_CYCLE_PROFILE);

        _isRegistered = false;
    }

    private bool TryRegister(IntPtr hwnd, int id, HotkeyBinding binding)
    {
        if (binding.IsNone) return true;
        return Win32Helper.RegisterHotKey(hwnd, id, binding.Modifiers | Win32Helper.MOD_NOREPEAT, binding.VirtualKey);
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == Win32Helper.WM_HOTKEY)
        {
            switch (wParam.ToInt32())
            {
                case ID_TOGGLE_OVERLAY:     ToggleOverlayRequested?.Invoke();     break;
                case ID_TOGGLE_CROSSHAIR:   ToggleCrosshairRequested?.Invoke();   break;
                case ID_TOGGLE_SPLITSCREEN: ToggleSplitScreenRequested?.Invoke(); break;
                case ID_TOGGLE_DISPLAYMODE: ToggleDisplayModeRequested?.Invoke(); break;
                case ID_CYCLE_OVERLAY_SIZE: CycleOverlaySizeRequested?.Invoke(); break;
                case ID_CYCLE_PROFILE:      CycleProfileRequested?.Invoke();      break;
            }
            handled = true;
        }
        return IntPtr.Zero;
    }

    public void Dispose()
    {
        _source?.RemoveHook(WndProc);
    }
}

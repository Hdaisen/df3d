using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DF3D.Models;

/// <summary>
/// User-configurable hotkey bindings for each action.
/// Each hotkey is stored as (Modifiers uint, VirtualKey uint).
/// </summary>
public class HotkeySettings : INotifyPropertyChanged
{
    private HotkeyBinding _toggleOverlay    = new(Helpers.Win32Helper.MOD_CONTROL | Helpers.Win32Helper.MOD_SHIFT, 0x70); // Ctrl+Shift+F1
    private HotkeyBinding _toggleCrosshair  = new(Helpers.Win32Helper.MOD_CONTROL | Helpers.Win32Helper.MOD_SHIFT, 0x71); // Ctrl+Shift+F2
    private HotkeyBinding _toggleSplitScreen = new(Helpers.Win32Helper.MOD_CONTROL | Helpers.Win32Helper.MOD_SHIFT, 0x72); // Ctrl+Shift+F3
    private HotkeyBinding _toggleDisplayMode = new(Helpers.Win32Helper.MOD_CONTROL | Helpers.Win32Helper.MOD_SHIFT, 0x73); // Ctrl+Shift+F4
    private HotkeyBinding _cycleOverlaySize = new(Helpers.Win32Helper.MOD_CONTROL | Helpers.Win32Helper.MOD_SHIFT, 0x74); // Ctrl+Shift+F5
    private HotkeyBinding _cycleProfile     = new(Helpers.Win32Helper.MOD_CONTROL | Helpers.Win32Helper.MOD_SHIFT, 0x75); // Ctrl+Shift+F6

    public HotkeyBinding ToggleOverlay
    {
        get => _toggleOverlay;
        set => SetField(ref _toggleOverlay, value);
    }

    public HotkeyBinding ToggleCrosshair
    {
        get => _toggleCrosshair;
        set => SetField(ref _toggleCrosshair, value);
    }

    public HotkeyBinding ToggleSplitScreen
    {
        get => _toggleSplitScreen;
        set => SetField(ref _toggleSplitScreen, value);
    }

    public HotkeyBinding ToggleDisplayMode
    {
        get => _toggleDisplayMode;
        set => SetField(ref _toggleDisplayMode, value);
    }

    public HotkeyBinding CycleOverlaySize
    {
        get => _cycleOverlaySize;
        set => SetField(ref _cycleOverlaySize, value);
    }

    public HotkeyBinding CycleProfile
    {
        get => _cycleProfile;
        set => SetField(ref _cycleProfile, value);
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

/// <summary>
/// A single hotkey binding: modifier keys + virtual key code.
/// </summary>
public record HotkeyBinding(uint Modifiers, uint VirtualKey)
{
    /// <summary>
    /// Human-readable display string, e.g. "Ctrl+Shift+F1".
    /// </summary>
    public string DisplayString
    {
        get
        {
            var parts = new List<string>();
            if ((Modifiers & Helpers.Win32Helper.MOD_CONTROL) != 0) parts.Add("Ctrl");
            if ((Modifiers & Helpers.Win32Helper.MOD_ALT) != 0)     parts.Add("Alt");
            if ((Modifiers & Helpers.Win32Helper.MOD_SHIFT) != 0)   parts.Add("Shift");
            parts.Add(VkToString(VirtualKey));
            return string.Join("+", parts);
        }
    }

    public bool IsNone => Modifiers == 0 && VirtualKey == 0;

    private static string VkToString(uint vk)
    {
        // Function keys
        if (vk >= 0x70 && vk <= 0x7B) return $"F{vk - 0x6F}";
        // Digits
        if (vk >= 0x30 && vk <= 0x39) return ((char)vk).ToString();
        // Letters
        if (vk >= 0x41 && vk <= 0x5A) return ((char)vk).ToString();
        // Special keys
        return vk switch
        {
            0x20 => "Space",
            0x1B => "Esc",
            0x0D => "Enter",
            0x09 => "Tab",
            0x2D => "Insert",
            0x2E => "Delete",
            0x24 => "Home",
            0x23 => "End",
            0x21 => "PageUp",
            0x22 => "PageDown",
            _ => $"VK(0x{vk:X2})"
        };
    }
}

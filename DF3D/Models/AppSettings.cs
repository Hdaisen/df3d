namespace DF3D.Models;

/// <summary>
/// Top-level application settings, persisted to JSON.
/// </summary>
public class AppSettings
{
    public OverlaySettings Overlay { get; set; } = new();
    public CrosshairSettings Crosshair { get; set; } = new();
    public HotkeySettings Hotkeys { get; set; } = new();
    public List<ProfileSettings> Profiles { get; set; } = new();
    public string? ActiveProfileName { get; set; }
    public bool StartWithWindows { get; set; }
    public bool MinimizeToTray { get; set; } = true;
    public bool ShowOverlayOnStartup { get; set; } = true;
    public bool ShowCrosshairOnStartup { get; set; } = true;
    public string Language { get; set; } = "zh-CN";
    public int SettingsWindowLeft { get; set; } = -1;
    public int SettingsWindowTop { get; set; } = -1;
}

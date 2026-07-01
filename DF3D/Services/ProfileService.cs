using DF3D.Models;

namespace DF3D.Services;

/// <summary>
/// Manages named presets (profiles) and the active profile.
/// </summary>
public sealed class ProfileService
{
    private readonly SettingsService _settingsService;
    private ProfileSettings? _activeProfile;

    public event Action? ActiveProfileChanged;

    public ProfileService(SettingsService settingsService)
    {
        _settingsService = settingsService;
    }

    public IReadOnlyList<ProfileSettings> Profiles => _settingsService.Settings.Profiles;

    public ProfileSettings ActiveProfile
    {
        get
        {
            if (_activeProfile is not null) return _activeProfile;

            // Try to find the saved active profile
            var name = _settingsService.Settings.ActiveProfileName;
            _activeProfile = Profiles.FirstOrDefault(p => p.Name == name) ?? Profiles.FirstOrDefault()
                             ?? new ProfileSettings();

            return _activeProfile;
        }
    }

    /// <summary>
    /// Switches to the profile with the given name.
    /// </summary>
    public void SetActiveProfile(string name)
    {
        var profile = Profiles.FirstOrDefault(p => p.Name == name);
        if (profile is null) return;

        _activeProfile = profile;
        _settingsService.Settings.ActiveProfileName = name;

        // Copy profile settings into the live settings
        CopyProfileToSettings(profile);
        _settingsService.Save();
        ActiveProfileChanged?.Invoke();
    }

    /// <summary>
    /// Cycles to the next profile in the list.
    /// </summary>
    public void CycleNext()
    {
        var list = Profiles.ToList();
        if (list.Count == 0) return;

        var currentIndex = list.IndexOf(ActiveProfile);
        var nextIndex = (currentIndex + 1) % list.Count;
        SetActiveProfile(list[nextIndex].Name);
    }

    /// <summary>
    /// Adds a new profile based on the current settings.
    /// </summary>
    public ProfileSettings AddProfile(string name)
    {
        var profile = new ProfileSettings
        {
            Name = name,
            Overlay = _settingsService.Settings.Overlay.Clone(),
            Crosshair = _settingsService.Settings.Crosshair.Clone()
        };
        _settingsService.Settings.Profiles.Add(profile);
        _settingsService.Save();
        return profile;
    }

    /// <summary>
    /// Removes a non-built-in profile.
    /// </summary>
    public bool RemoveProfile(string name)
    {
        var profile = Profiles.FirstOrDefault(p => p.Name == name);
        if (profile is null || profile.IsBuiltIn) return false;

        _settingsService.Settings.Profiles.Remove(profile);

        // If we removed the active profile, switch to the first available
        if (_activeProfile?.Name == name)
        {
            SetActiveProfile(Profiles.FirstOrDefault()?.Name ?? "默认");
        }

        _settingsService.Save();
        return true;
    }

    /// <summary>
    /// Updates the active profile with current settings.
    /// </summary>
    public void UpdateActiveProfileFromCurrentSettings()
    {
        if (_activeProfile is null) return;
        _activeProfile.Overlay = _settingsService.Settings.Overlay.Clone();
        _activeProfile.Crosshair = _settingsService.Settings.Crosshair.Clone();
        _settingsService.Save();
    }

    private void CopyProfileToSettings(ProfileSettings profile)
    {
        var s = _settingsService.Settings;
        s.Overlay.Shape       = profile.Overlay.Shape;
        s.Overlay.Size        = profile.Overlay.Size;
        s.Overlay.Length      = profile.Overlay.Length;
        s.Overlay.Color       = profile.Overlay.Color;
        s.Overlay.CustomColorHex = profile.Overlay.CustomColorHex;
        s.Overlay.Opacity     = profile.Overlay.Opacity;
        s.Overlay.AspectRatio = profile.Overlay.AspectRatio;
        s.Overlay.DisplayMode = profile.Overlay.DisplayMode;
        s.Overlay.SplitScreen = profile.Overlay.SplitScreen;

        s.Crosshair.Shape         = profile.Crosshair.Shape;
        s.Crosshair.Size          = profile.Crosshair.Size;
        s.Crosshair.Thickness     = profile.Crosshair.Thickness;
        s.Crosshair.OffsetX       = profile.Crosshair.OffsetX;
        s.Crosshair.OffsetY       = profile.Crosshair.OffsetY;
        s.Crosshair.Color         = profile.Crosshair.Color;
        s.Crosshair.CustomColorHex = profile.Crosshair.CustomColorHex;
        s.Crosshair.Opacity       = profile.Crosshair.Opacity;
        s.Crosshair.AspectRatio   = profile.Crosshair.AspectRatio;
        s.Crosshair.SplitScreen   = profile.Crosshair.SplitScreen;
    }
}

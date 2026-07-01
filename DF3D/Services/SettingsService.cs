using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using DF3D.Models;

namespace DF3D.Services;

/// <summary>
/// Manages loading, saving, and notifying of application settings.
/// </summary>
public sealed class SettingsService
{
    private static readonly string SettingsDir =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DF3D");

    private static readonly string SettingsPath = Path.Combine(SettingsDir, "settings.json");

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private AppSettings _settings = new();
    private readonly DebounceSaver _debounceSaver;

    public AppSettings Settings => _settings;

    public event Action? SettingsChanged;

    public SettingsService()
    {
        _debounceSaver = new DebounceSaver(SaveImmediate);
    }

    /// <summary>
    /// Loads settings from disk, or creates defaults if the file doesn't exist.
    /// </summary>
    public void Load()
    {
        try
        {
            if (File.Exists(SettingsPath))
            {
                var json = File.ReadAllText(SettingsPath);
                _settings = JsonSerializer.Deserialize<AppSettings>(json, JsonOptions) ?? new AppSettings();
            }
            else
            {
                _settings = new AppSettings();
                _settings.Profiles = ProfileSettings.CreateDefaults();
                _settings.ActiveProfileName = _settings.Profiles[0].Name;
            }
        }
        catch
        {
            _settings = new AppSettings();
            _settings.Profiles = ProfileSettings.CreateDefaults();
            _settings.ActiveProfileName = _settings.Profiles[0].Name;
        }
    }

    /// <summary>
    /// Schedules a debounced save (500ms delay to batch rapid changes).
    /// </summary>
    public void Save()
    {
        _debounceSaver.ScheduleSave();
        SettingsChanged?.Invoke();
    }

    /// <summary>
    /// Forces an immediate save to disk.
    /// </summary>
    public void SaveImmediate()
    {
        try
        {
            Directory.CreateDirectory(SettingsDir);
            var json = JsonSerializer.Serialize(_settings, JsonOptions);
            File.WriteAllText(SettingsPath, json);
        }
        catch
        {
            // Silently fail - settings will be saved on next attempt
        }
    }

    /// <summary>
    /// Debounces rapid save requests to avoid excessive disk writes.
    /// </summary>
    private sealed class DebounceSaver
    {
        private readonly Action _saveAction;
        private Timer? _timer;

        public DebounceSaver(Action saveAction)
        {
            _saveAction = saveAction;
        }

        public void ScheduleSave()
        {
            _timer?.Dispose();
            _timer = new Timer(_ => _saveAction(), null, 500, Timeout.Infinite);
        }
    }
}

using System.Diagnostics;
using System.IO;
using System.Text.Json;
using caffeine.Models;

namespace caffeine.Services;

/// <summary>
/// Service responsible for persisting and loading user settings from local storage.
/// Settings are stored in %APPDATA%\caffeine\settings.json.
/// </summary>
public class SettingsService
{
    private readonly string _settingsPath;
    private readonly string _settingsDirectory;

    /// <summary>
    /// Initializes a new instance of the SettingsService class.
    /// </summary>
    public SettingsService()
    {
        _settingsDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "caffeine");

        _settingsPath = Path.Combine(_settingsDirectory, "settings.json");
    }

    /// <summary>
    /// Loads user settings from local storage. Returns default settings if file doesn't exist or on error.
    /// </summary>
    /// <returns>The loaded UserSettings or default settings on failure.</returns>
    public UserSettings Load()
    {
        try
        {
            if (!File.Exists(_settingsPath))
            {
                Debug.WriteLine($"[SettingsService] Settings file not found at {_settingsPath}. Using defaults.");
                return new UserSettings();
            }

            string json = File.ReadAllText(_settingsPath);
            var settings = JsonSerializer.Deserialize<UserSettings>(json);

            if (settings == null)
            {
                Debug.WriteLine("[SettingsService] Failed to deserialize settings. Using defaults.");
                return new UserSettings();
            }

            Debug.WriteLine("[SettingsService] Settings loaded successfully.");
            return settings;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[SettingsService] Error loading settings: {ex.Message}. Using defaults.");
            return new UserSettings();
        }
    }

    /// <summary>
    /// Saves user settings to local storage.
    /// </summary>
    /// <param name="settings">The settings to save.</param>
    public void Save(UserSettings settings)
    {
        try
        {
            // Create directory if it doesn't exist
            Directory.CreateDirectory(_settingsDirectory);

            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            string json = JsonSerializer.Serialize(settings, options);
            File.WriteAllText(_settingsPath, json);

            Debug.WriteLine($"[SettingsService] Settings saved successfully to {_settingsPath}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[SettingsService] Error saving settings: {ex.Message}");
            // Suppress exception - settings not saving is not critical enough to crash the app
        }
    }

    /// <summary>
    /// Gets the full path to the settings file.
    /// </summary>
    public string SettingsPath => _settingsPath;
}

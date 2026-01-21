using System.Text.Json.Serialization;

namespace caffeine.Models;

/// <summary>
/// User preferences persisted to local storage.
/// </summary>
public class UserSettings
{
    /// <summary>
    /// Whether to keep the display on when active (Mode 1) or allow it to turn off (Mode 2).
    /// </summary>
    [JsonPropertyName("keepDisplayOn")]
    public bool KeepDisplayOn { get; set; } = true;

    /// <summary>
    /// Notification behavior when timers expire.
    /// </summary>
    [JsonPropertyName("notificationMode")]
    public NotificationMode NotificationMode { get; set; } = NotificationMode.NotificationAndSound;

    /// <summary>
    /// Whether to launch Caffeine at Windows startup.
    /// </summary>
    [JsonPropertyName("runAtStartup")]
    public bool RunAtStartup { get; set; } = false;

    /// <summary>
    /// Whether to show remaining time in the tray icon tooltip.
    /// </summary>
    [JsonPropertyName("showRemainingTime")]
    public bool ShowRemainingTime { get; set; } = true;

    /// <summary>
    /// Last used duration in minutes for "Active for" feature (for user convenience).
    /// </summary>
    [JsonPropertyName("lastUsedDurationMinutes")]
    public int LastUsedDurationMinutes { get; set; } = 60;
}

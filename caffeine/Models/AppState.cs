namespace caffeine.Models;

/// <summary>
/// Represents the current operational state of the Caffeine application.
/// </summary>
public enum AppState
{
    /// <summary>
    /// Application is actively keeping the PC awake.
    /// </summary>
    Active,

    /// <summary>
    /// Application is inactive, allowing normal power behavior.
    /// </summary>
    Inactive,

    /// <summary>
    /// Application is temporarily inactive and will resume automatically.
    /// </summary>
    Paused
}

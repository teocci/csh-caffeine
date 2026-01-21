namespace caffeine.Models;

/// <summary>
/// Defines notification behavior when timers expire.
/// </summary>
public enum NotificationMode
{
    /// <summary>
    /// Show toast notification and play sound (default).
    /// </summary>
    NotificationAndSound,

    /// <summary>
    /// Show toast notification only, no sound.
    /// </summary>
    NotificationOnly,

    /// <summary>
    /// Play sound only, no visual notification.
    /// </summary>
    SoundOnly,

    /// <summary>
    /// Silent mode - no notifications or sounds.
    /// </summary>
    Silent
}

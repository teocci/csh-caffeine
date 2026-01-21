using System.Diagnostics;
using System.IO;
using System.Media;
using System.Reflection;
using caffeine.Models;
using Microsoft.Toolkit.Uwp.Notifications;

namespace caffeine.Services;

/// <summary>
/// Service responsible for showing notifications and playing sounds when timers expire.
/// Uses Windows toast notifications and System.Media.SoundPlayer.
/// </summary>
public class NotificationService
{
    private readonly SoundPlayer? _soundPlayer;

    /// <summary>
    /// Initializes a new instance of the NotificationService class.
    /// </summary>
    public NotificationService()
    {
        // Try to load notification sound from embedded resource (for single-file compatibility)
        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            var stream = assembly.GetManifestResourceStream("notification.wav");

            if (stream != null)
            {
                _soundPlayer = new SoundPlayer(stream);
                _soundPlayer.Load();
                Debug.WriteLine("[NotificationService] Notification sound loaded from embedded resource");
            }
            else
            {
                Debug.WriteLine("[NotificationService] Notification sound not found in embedded resources. Sound notifications will be silent.");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[NotificationService] Failed to load sound: {ex.Message}");
        }
    }

    /// <summary>
    /// Shows a notification when a timer expires, respecting the user's notification mode setting.
    /// </summary>
    /// <param name="message">The notification message to display.</param>
    /// <param name="mode">The notification mode determining how to notify the user.</param>
    public void ShowTimerExpiredNotification(string message, NotificationMode mode)
    {
        Debug.WriteLine($"[NotificationService] Showing notification: {message} (Mode: {mode})");

        // Show toast notification if enabled
        if (mode == NotificationMode.NotificationAndSound ||
            mode == NotificationMode.NotificationOnly)
        {
            ShowToast("Caffeine", message);
        }

        // Play sound if enabled
        if (mode == NotificationMode.NotificationAndSound ||
            mode == NotificationMode.SoundOnly)
        {
            PlaySound();
        }
    }

    /// <summary>
    /// Shows a Windows toast notification.
    /// </summary>
    /// <param name="title">The notification title.</param>
    /// <param name="message">The notification message.</param>
    private void ShowToast(string title, string message)
    {
        try
        {
            new ToastContentBuilder()
                .AddText(title)
                .AddText(message)
                .Show();

            Debug.WriteLine($"[NotificationService] Toast notification shown: {title} - {message}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[NotificationService] Failed to show toast: {ex.Message}");
        }
    }

    /// <summary>
    /// Plays the notification sound.
    /// </summary>
    private void PlaySound()
    {
        try
        {
            _soundPlayer?.Play();
            Debug.WriteLine("[NotificationService] Notification sound played");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[NotificationService] Failed to play sound: {ex.Message}");
        }
    }
}

using System.Diagnostics;
using System.Reflection;
using System.Security;
using Microsoft.Toolkit.Uwp.Notifications;
using Microsoft.Win32;

namespace caffeine.Services;

/// <summary>
/// Service responsible for managing Windows startup behavior.
/// Adds or removes registry entry to launch the application at Windows startup.
/// </summary>
public class StartupService
{
    private const string AppName = "Caffeine";
    private const string RegistryPath = @"Software\Microsoft\Windows\CurrentVersion\Run";

    /// <summary>
    /// Sets whether the application should start automatically with Windows.
    /// </summary>
    /// <param name="enabled">True to enable startup, false to disable.</param>
    public void SetStartupEnabled(bool enabled)
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RegistryPath, true);

            if (key == null)
            {
                Debug.WriteLine("[StartupService] Unable to open registry key");
                ShowRegistryAccessError();
                return;
            }

            if (enabled)
            {
                string exePath = GetExecutablePath();
                key.SetValue(AppName, $"\"{exePath}\"");
                Debug.WriteLine($"[StartupService] Startup enabled: {exePath}");
            }
            else
            {
                key.DeleteValue(AppName, false);
                Debug.WriteLine("[StartupService] Startup disabled");
            }
        }
        catch (SecurityException ex)
        {
            Debug.WriteLine($"[StartupService] Security exception: {ex.Message}");
            ShowRegistryAccessError();
        }
        catch (UnauthorizedAccessException ex)
        {
            Debug.WriteLine($"[StartupService] Unauthorized access: {ex.Message}");
            ShowRegistryAccessError();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[StartupService] Unexpected error: {ex.Message}");
        }
    }

    /// <summary>
    /// Checks whether the application is configured to start with Windows.
    /// </summary>
    /// <returns>True if startup is enabled, false otherwise.</returns>
    public bool IsStartupEnabled()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RegistryPath, false);

            if (key == null)
            {
                return false;
            }

            var value = key.GetValue(AppName);
            return value != null;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[StartupService] Error checking startup status: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Gets the path to the application executable.
    /// </summary>
    /// <returns>The full path to the executable.</returns>
    private static string GetExecutablePath()
    {
        // Environment.ProcessPath works for both single-file and normal deployments
        if (Environment.ProcessPath is { } processPath)
        {
            return processPath;
        }

        // Fallback to main module path
        return Process.GetCurrentProcess().MainModule?.FileName
            ?? throw new InvalidOperationException("Unable to determine executable path.");
    }

    /// <summary>
    /// Shows a toast notification when registry access fails.
    /// </summary>
    private void ShowRegistryAccessError()
    {
        try
        {
            new ToastContentBuilder()
                .AddText("Caffeine")
                .AddText("Unable to modify startup settings. Check permissions.")
                .Show();
        }
        catch
        {
            // Suppress - if toast also fails, we can't do much
        }
    }
}

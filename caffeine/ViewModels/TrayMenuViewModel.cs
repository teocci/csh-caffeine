using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using caffeine.Helpers;
using caffeine.Models;

namespace caffeine.ViewModels;

/// <summary>
/// ViewModel for the system tray menu. Manages application state and commands.
/// </summary>
public sealed class TrayMenuViewModel : INotifyPropertyChanged
{
    private AppState _currentState;
    private bool _isAwake;
    private string _tooltipText;
    private bool _keepDisplayOn;
    private NotificationMode _notificationMode;
    private bool _runAtStartup;
    private bool _showRemainingTime;
    private TimeSpan? _remainingTime;

    /// <summary>
    /// Occurs when a property value changes.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Gets or sets the current application state.
    /// </summary>
    public AppState CurrentState
    {
        get => _currentState;
        set
        {
            if (_currentState == value) return;
            
            _currentState = value;
            OnPropertyChanged();
            UpdateTooltipText();
        }
    }

    /// <summary>
    /// Gets or sets whether the system is being kept awake.
    /// </summary>
    public bool IsAwake
    {
        get => _isAwake;
        set
        {
            if (_isAwake == value) return;
            
            _isAwake = value;
            OnPropertyChanged();
            UpdateTooltipText();
        }
    }

    /// <summary>
    /// Gets or sets the tooltip text displayed when hovering over the tray icon.
    /// </summary>
    public string TooltipText
    {
        get => _tooltipText;
        set
        {
            if (_tooltipText == value) return;
            
            _tooltipText = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets whether to keep the display on when active (Mode 1 vs Mode 2).
    /// </summary>
    public bool KeepDisplayOn
    {
        get => _keepDisplayOn;
        set
        {
            if (_keepDisplayOn == value) return;
            
            _keepDisplayOn = value;
            OnPropertyChanged();
            UpdateTooltipText();
        }
    }

    /// <summary>
    /// Gets or sets the notification mode for timer expiration events.
    /// </summary>
    public NotificationMode NotificationMode
    {
        get => _notificationMode;
        set
        {
            if (_notificationMode == value) return;
            
            _notificationMode = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets whether the application should run at Windows startup.
    /// </summary>
    public bool RunAtStartup
    {
        get => _runAtStartup;
        set
        {
            if (_runAtStartup == value) return;
            
            _runAtStartup = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets whether to show remaining time in the tooltip.
    /// </summary>
    public bool ShowRemainingTime
    {
        get => _showRemainingTime;
        set
        {
            if (_showRemainingTime == value) return;
            
            _showRemainingTime = value;
            OnPropertyChanged();
            UpdateTooltipText();
        }
    }

    /// <summary>
    /// Gets or sets the remaining time on the current timer.
    /// </summary>
    public TimeSpan? RemainingTime
    {
        get => _remainingTime;
        set
        {
            if (_remainingTime == value) return;
            
            _remainingTime = value;
            OnPropertyChanged();
            UpdateTooltipText();
        }
    }

    /// <summary>
    /// Command to toggle the keep-awake state.
    /// </summary>
    public ICommand ToggleCommand { get; init; }

    /// <summary>
    /// Command to set "Active for" with a specific duration.
    /// Parameter: int? minutes (null = indefinitely)
    /// </summary>
    public ICommand SetActiveForCommand { get; init; }

    /// <summary>
    /// Command to set "Inactive for" with a specific duration.
    /// Parameter: int minutes
    /// </summary>
    public ICommand SetInactiveForCommand { get; init; }

    /// <summary>
    /// Command to show the About dialog.
    /// </summary>
    public ICommand ShowAboutCommand { get; init; }

    /// <summary>
    /// Command to exit the application.
    /// </summary>
    public ICommand ExitCommand { get; init; }

    /// <summary>
    /// Initializes a new instance of the TrayMenuViewModel class.
    /// </summary>
    /// <param name="settings">Initial user settings.</param>
    public TrayMenuViewModel(UserSettings settings)
    {
        // Initialize from settings
        _currentState = AppState.Active;
        _isAwake = true;
        _tooltipText = "Caffeine: Active";
        _keepDisplayOn = settings.KeepDisplayOn;
        _notificationMode = settings.NotificationMode;
        _runAtStartup = settings.RunAtStartup;
        _showRemainingTime = settings.ShowRemainingTime;

        // Initialize commands with null placeholders (will be wired up by TrayIconService)
        ToggleCommand = null!;
        SetActiveForCommand = null!;
        SetInactiveForCommand = null!;
        ShowAboutCommand = null!;
        ExitCommand = null!;
    }

    /// <summary>
    /// Updates the tooltip text based on current state.
    /// </summary>
    private void UpdateTooltipText()
    {
        switch (CurrentState)
        {
            case AppState.Inactive:
                TooltipText = "Caffeine: Inactive";
                return;
            case AppState.Paused when RemainingTime.HasValue:
            {
                var timeText = FormatTimeSpan(RemainingTime.Value);
                TooltipText = $"Caffeine: Paused (resumes in {timeText})";
                return;
            }
            case AppState.Active:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        if (CurrentState == AppState.Active)
        {
            if (RemainingTime.HasValue && ShowRemainingTime)
            {
                var timeText = FormatTimeSpan(RemainingTime.Value);
                TooltipText = $"Caffeine: Active ({timeText} remaining)";
            }
            else if (!KeepDisplayOn)
            {
                TooltipText = "Caffeine: Active (display can sleep)";
            }
            else
            {
                TooltipText = "Caffeine: Active";
            }
            return;
        }

        TooltipText = "Caffeine";
    }

    /// <summary>
    /// Formats a TimeSpan for display in the tooltip.
    /// </summary>
    /// <param name="time">The time to format.</param>
    /// <returns>Formatted time string (e.g., "1h 23m" or "5m").</returns>
    private static string FormatTimeSpan(TimeSpan time)
    {
        return time.TotalHours >= 1 ? $"{(int)time.TotalHours}h {time.Minutes}m" : $"{time.Minutes}m";
    }

    /// <summary>
    /// Raises the PropertyChanged event.
    /// </summary>
    /// <param name="propertyName">The name of the property that changed.</param>
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

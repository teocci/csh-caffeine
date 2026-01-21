using System.Diagnostics;
using System.Windows.Threading;

namespace caffeine.Services;

/// <summary>
/// Represents the type of timer currently running.
/// </summary>
public enum TimerType
{
    None,
    ActiveFor,
    InactiveFor
}

/// <summary>
/// Service responsible for managing countdown timers for "Active for" and "Inactive for" features.
/// Uses DispatcherTimer to ensure timer callbacks run on the UI thread.
/// </summary>
public class TimerService
{
    private readonly DispatcherTimer _timer;
    private DateTime _endTime;
    private TimerType _currentTimerType;

    /// <summary>
    /// Occurs when an "Active for" timer expires.
    /// </summary>
    public event EventHandler? ActiveTimerExpired;

    /// <summary>
    /// Occurs when an "Inactive for" (pause) timer expires.
    /// </summary>
    public event EventHandler? PauseTimerExpired;

    /// <summary>
    /// Occurs when the remaining time changes (every second).
    /// </summary>
    public event EventHandler? RemainingTimeChanged;

    /// <summary>
    /// Gets the remaining time on the current timer, or null if no timer is running.
    /// </summary>
    public TimeSpan? RemainingTime { get; private set; }

    /// <summary>
    /// Gets the type of timer currently running.
    /// </summary>
    public TimerType CurrentTimerType => _currentTimerType;

    /// <summary>
    /// Gets whether a timer is currently running.
    /// </summary>
    public bool IsRunning => _timer.IsEnabled;

    /// <summary>
    /// Initializes a new instance of the TimerService class.
    /// </summary>
    public TimerService()
    {
        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _timer.Tick += OnTimerTick;
        _currentTimerType = TimerType.None;
    }

    /// <summary>
    /// Starts an "Active for" timer that will expire after the specified duration.
    /// </summary>
    /// <param name="minutes">The duration in minutes.</param>
    public void StartActiveForTimer(int minutes)
    {
        _currentTimerType = TimerType.ActiveFor;
        _endTime = DateTime.Now.AddMinutes(minutes);
        _timer.Start();

        Debug.WriteLine($"[TimerService] Started ActiveFor timer for {minutes} minutes");
    }

    /// <summary>
    /// Starts an "Inactive for" (pause) timer that will expire after the specified duration.
    /// </summary>
    /// <param name="minutes">The duration in minutes.</param>
    public void StartPauseTimer(int minutes)
    {
        _currentTimerType = TimerType.InactiveFor;
        _endTime = DateTime.Now.AddMinutes(minutes);
        _timer.Start();

        Debug.WriteLine($"[TimerService] Started Pause timer for {minutes} minutes");
    }

    /// <summary>
    /// Stops the current timer.
    /// </summary>
    public void Stop()
    {
        _timer.Stop();
        RemainingTime = null;
        _currentTimerType = TimerType.None;

        Debug.WriteLine("[TimerService] Timer stopped");

        // Notify listeners that remaining time changed (to null)
        RemainingTimeChanged?.Invoke(this, EventArgs.Empty);
    }

    private void OnTimerTick(object? sender, EventArgs e)
    {
        RemainingTime = _endTime - DateTime.Now;

        // Notify listeners of time change
        RemainingTimeChanged?.Invoke(this, EventArgs.Empty);

        if (RemainingTime <= TimeSpan.Zero)
        {
            _timer.Stop();
            RemainingTime = null;

            var timerType = _currentTimerType;
            _currentTimerType = TimerType.None;

            Debug.WriteLine($"[TimerService] Timer expired: {timerType}");

            // Fire appropriate event
            if (timerType == TimerType.ActiveFor)
            {
                ActiveTimerExpired?.Invoke(this, EventArgs.Empty);
            }
            else if (timerType == TimerType.InactiveFor)
            {
                PauseTimerExpired?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}

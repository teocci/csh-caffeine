using System.Windows;
using caffeine.Models;
using caffeine.Services;
using caffeine.ViewModels;

namespace caffeine;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private static Mutex? _instanceMutex;
    private const string MutexName = "CaffeineAppSingleInstance";

    // Services
    private SettingsService? _settingsService;
    private PowerService? _powerService;
    private TimerService? _timerService;
    private NotificationService? _notificationService;
    private StartupService? _startupService;
    private TrayIconService? _trayIconService;
    private TrayMenuViewModel? _viewModel;

    /// <summary>
    /// Handles application startup - enforces single instance and initializes services.
    /// </summary>
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Single instance enforcement
        _instanceMutex = new Mutex(true, MutexName, out bool createdNew);

        if (!createdNew)
        {
            // Another instance is already running
            MessageBox.Show(
                "Caffeine is already running. Check the system tray.",
                "Caffeine",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            Shutdown();
            return;
        }

        try
        {
            // Initialize services in dependency order
            _settingsService = new SettingsService();
            var settings = _settingsService.Load();

            _powerService = new PowerService();
            _timerService = new TimerService();
            _notificationService = new NotificationService();
            _startupService = new StartupService();

            _viewModel = new TrayMenuViewModel(settings);
            _trayIconService = new TrayIconService(
                _viewModel,
                _powerService,
                _timerService,
                _notificationService,
                _settingsService,
                _startupService);

            // Wire up timer events
            _timerService.ActiveTimerExpired += OnActiveTimerExpired;
            _timerService.PauseTimerExpired += OnPauseTimerExpired;

            // Set initial power state (app starts Active per PRD)
            _powerService.SetAwakeState(true, settings.KeepDisplayOn);
            _viewModel.IsAwake = true;
            _viewModel.CurrentState = AppState.Active;
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Failed to initialize Caffeine:\n\n{ex.Message}\n\nStack Trace:\n{ex.StackTrace}",
                "Caffeine Initialization Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            Shutdown(1);
        }
    }

    /// <summary>
    /// Handles the Active timer expiration event.
    /// </summary>
    private void OnActiveTimerExpired(object? sender, EventArgs e)
    {
        // Deactivate and notify
        _powerService?.SetAwakeState(false, false);
        _notificationService?.ShowTimerExpiredNotification(
            "Active timer expired. PC can now sleep.",
            _viewModel?.NotificationMode ?? NotificationMode.NotificationAndSound);

        if (_viewModel == null) return;
        
        _viewModel.IsAwake = false;
        _viewModel.CurrentState = AppState.Inactive;
        _viewModel.RemainingTime = null;
    }

    /// <summary>
    /// Handles the Pause timer expiration event.
    /// </summary>
    private void OnPauseTimerExpired(object? sender, EventArgs e)
    {
        // Resume active state
        var settings = _settingsService?.Load() ?? new UserSettings();
        _powerService?.SetAwakeState(true, settings.KeepDisplayOn);
        _notificationService?.ShowTimerExpiredNotification(
            "Pause ended. Keeping PC awake again.",
            _viewModel?.NotificationMode ?? NotificationMode.NotificationAndSound);

        if (_viewModel == null) return;
        
        _viewModel.IsAwake = true;
        _viewModel.CurrentState = AppState.Active;
        _viewModel.RemainingTime = null;
    }

    /// <summary>
    /// Handles application exit - releases the single instance mutex.
    /// </summary>
    protected override void OnExit(ExitEventArgs e)
    {
        // Clean up services
        _trayIconService?.Dispose();
        _powerService?.Reset(); // Reset power state so PC can sleep normally
        _instanceMutex?.ReleaseMutex();
        _instanceMutex?.Dispose();

        base.OnExit(e);
    }
}
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using caffeine.Helpers;
using caffeine.Models;
using caffeine.ViewModels;
using caffeine.Views;
using H.NotifyIcon;

namespace caffeine.Services;

/// <summary>
/// Service responsible for managing the system tray icon, context menu, and user interaction.
/// Orchestrates all other services and coordinates application behavior.
/// </summary>
public class TrayIconService : IDisposable
{
    private readonly TaskbarIcon _taskbarIcon;
    private readonly TrayMenuViewModel _viewModel;
    private readonly PowerService _powerService;
    private readonly TimerService _timerService;
    private readonly NotificationService _notificationService;
    private readonly SettingsService _settingsService;
    private readonly StartupService _startupService;

    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the TrayIconService class.
    /// </summary>
    public TrayIconService(
        TrayMenuViewModel viewModel,
        PowerService powerService,
        TimerService timerService,
        NotificationService notificationService,
        SettingsService settingsService,
        StartupService startupService)
    {
        _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        _powerService = powerService ?? throw new ArgumentNullException(nameof(powerService));
        _timerService = timerService ?? throw new ArgumentNullException(nameof(timerService));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
        _startupService = startupService ?? throw new ArgumentNullException(nameof(startupService));

        // Get TaskbarIcon from Application Resources

        var app = Application.Current;
        if (app == null) throw new NullReferenceException("Application.Current is null");
        
        _taskbarIcon = (TaskbarIcon)app.FindResource("CaffeineTrayIcon") ??
                       throw new InvalidOperationException("CaffeineTrayIcon not found in Application.Resources");

        // Force create the icon (required for windowless apps)
        _taskbarIcon.ForceCreate();

        // Build context menu
        _taskbarIcon.ContextMenu = CreateContextMenu();

        // Subscribe to ViewModel property changes
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;

        // Subscribe to timer events
        _timerService.RemainingTimeChanged += OnTimerRemainingTimeChanged;

        // Wire up double-click event
        _taskbarIcon.TrayMouseDoubleClick += OnTrayMouseDoubleClick;

        // Wire up commands
        WireUpCommands();

        // Sync icon with initial state (XAML defaults to inactive, but app starts active)
        UpdateIcon(_viewModel.CurrentState);

        Debug.WriteLine("[TrayIconService] Initialized");
    }

    /// <summary>
    /// Wires up ViewModel commands to actual implementations.
    /// </summary>
    private void WireUpCommands()
    {
        // Use reflection to set command implementations
        var toggleCommand = new RelayCommand(OnToggleAwake);
        var setActiveForCommand = new RelayCommand<int?>(OnSetActiveFor);
        var setInactiveForCommand = new RelayCommand<int>(OnSetInactiveFor);
        var showAboutCommand = new RelayCommand(OnShowAbout);
        var exitCommand = new RelayCommand(OnExit);

        // Replace the placeholder commands in ViewModel
        typeof(TrayMenuViewModel).GetProperty(nameof(TrayMenuViewModel.ToggleCommand))!
            .SetValue(_viewModel, toggleCommand);
        typeof(TrayMenuViewModel).GetProperty(nameof(TrayMenuViewModel.SetActiveForCommand))!
            .SetValue(_viewModel, setActiveForCommand);
        typeof(TrayMenuViewModel).GetProperty(nameof(TrayMenuViewModel.SetInactiveForCommand))!
            .SetValue(_viewModel, setInactiveForCommand);
        typeof(TrayMenuViewModel).GetProperty(nameof(TrayMenuViewModel.ShowAboutCommand))!
            .SetValue(_viewModel, showAboutCommand);
        typeof(TrayMenuViewModel).GetProperty(nameof(TrayMenuViewModel.ExitCommand))!
            .SetValue(_viewModel, exitCommand);
    }

    /// <summary>
    /// Creates the context menu structure.
    /// </summary>
    private ContextMenu CreateContextMenu()
    {
        var menu = new ContextMenu();
        menu.DataContext = _viewModel;

        // Keep awake toggle
        var toggleItem = new MenuItem
        {
            Header = "Keep awake",
            IsCheckable = true
        };
        toggleItem.SetBinding(MenuItem.IsCheckedProperty, nameof(TrayMenuViewModel.IsAwake));
        toggleItem.Click += (s, e) => _viewModel.ToggleCommand.Execute(null);
        menu.Items.Add(toggleItem);

        menu.Items.Add(new Separator());

        // Active for submenu
        menu.Items.Add(CreateActiveForMenu());

        // Inactive for submenu
        menu.Items.Add(CreateInactiveForMenu());

        menu.Items.Add(new Separator());

        // Settings submenu
        menu.Items.Add(CreateSettingsMenu());

        menu.Items.Add(new Separator());

        // About
        var aboutItem = new MenuItem
        {
            Header = "About"
        };
        aboutItem.Click += (s, e) => _viewModel.ShowAboutCommand.Execute(null);
        menu.Items.Add(aboutItem);

        // Exit
        var exitItem = new MenuItem
        {
            Header = "Exit"
        };
        exitItem.Click += (s, e) => _viewModel.ExitCommand.Execute(null);
        menu.Items.Add(exitItem);

        return menu;
    }

    /// <summary>
    /// Creates the "Active for..." submenu.
    /// </summary>
    private MenuItem CreateActiveForMenu()
    {
        var activeForMenu = new MenuItem
        {
            Header = "Active for..."
        };

        // Indefinitely
        var indefinitelyItem = new MenuItem
        {
            Header = "Indefinitely"
        };
        indefinitelyItem.Click += (s, e) => _viewModel.SetActiveForCommand.Execute(null);
        activeForMenu.Items.Add(indefinitelyItem);

        activeForMenu.Items.Add(new Separator());

        // Durations
        var durations = new (string Label, int Minutes)[]
        {
            ("15 minutes", 15),
            ("30 minutes", 30),
            ("45 minutes", 45),
            ("1 hour", 60),
            ("2 hours", 120),
            ("4 hours", 240),
            ("6 hours", 360),
            ("8 hours", 480),
            ("24 hours", 1440)
        };

        foreach (var (label, minutes) in durations)
        {
            var item = new MenuItem
            {
                Header = label,
                Tag = minutes
            };
            item.Click += (s, e) => _viewModel.SetActiveForCommand.Execute(minutes);
            activeForMenu.Items.Add(item);
        }

        return activeForMenu;
    }

    /// <summary>
    /// Creates the "Inactive for..." submenu.
    /// </summary>
    private MenuItem CreateInactiveForMenu()
    {
        var inactiveForMenu = new MenuItem
        {
            Header = "Inactive for..."
        };
        inactiveForMenu.SetBinding(MenuItem.IsEnabledProperty, nameof(TrayMenuViewModel.IsAwake));

        // Durations (same as Active for, minus Indefinitely)
        var durations = new (string Label, int Minutes)[]
        {
            ("15 minutes", 15),
            ("30 minutes", 30),
            ("45 minutes", 45),
            ("1 hour", 60),
            ("2 hours", 120),
            ("4 hours", 240),
            ("6 hours", 360),
            ("8 hours", 480),
            ("24 hours", 1440)
        };

        foreach (var (label, minutes) in durations)
        {
            var item = new MenuItem
            {
                Header = label,
                Tag = minutes
            };
            item.Click += (s, e) => _viewModel.SetInactiveForCommand.Execute(minutes);
            inactiveForMenu.Items.Add(item);
        }

        return inactiveForMenu;
    }

    /// <summary>
    /// Creates the "Settings" submenu.
    /// </summary>
    private MenuItem CreateSettingsMenu()
    {
        var settingsMenu = new MenuItem
        {
            Header = "Settings"
        };

        // Keep display on
        var keepDisplayOnItem = new MenuItem
        {
            Header = "Keep display on",
            IsCheckable = true
        };
        keepDisplayOnItem.SetBinding(MenuItem.IsCheckedProperty, nameof(TrayMenuViewModel.KeepDisplayOn));
        keepDisplayOnItem.Click += (s, e) => OnToggleKeepDisplayOn();
        settingsMenu.Items.Add(keepDisplayOnItem);

        // Notifications submenu
        settingsMenu.Items.Add(CreateNotificationsMenu());

        // Run at startup
        var runAtStartupItem = new MenuItem
        {
            Header = "Run at startup",
            IsCheckable = true
        };
        runAtStartupItem.SetBinding(MenuItem.IsCheckedProperty, nameof(TrayMenuViewModel.RunAtStartup));
        runAtStartupItem.Click += (s, e) => OnToggleRunAtStartup();
        settingsMenu.Items.Add(runAtStartupItem);

        // Show remaining time
        var showRemainingTimeItem = new MenuItem
        {
            Header = "Show remaining time",
            IsCheckable = true
        };
        showRemainingTimeItem.SetBinding(MenuItem.IsCheckedProperty, nameof(TrayMenuViewModel.ShowRemainingTime));
        showRemainingTimeItem.Click += (s, e) => OnToggleShowRemainingTime();
        settingsMenu.Items.Add(showRemainingTimeItem);

        return settingsMenu;
    }

    /// <summary>
    /// Creates the "Notifications" submenu.
    /// </summary>
    private MenuItem CreateNotificationsMenu()
    {
        var notificationsMenu = new MenuItem
        {
            Header = "Notifications"
        };

        var modes = new (string Label, NotificationMode Mode)[]
        {
            ("Notification + Sound", NotificationMode.NotificationAndSound),
            ("Notification only", NotificationMode.NotificationOnly),
            ("Sound only", NotificationMode.SoundOnly),
            ("Silent", NotificationMode.Silent)
        };

        foreach (var (label, mode) in modes)
        {
            var item = new MenuItem
            {
                Header = label,
                IsCheckable = true,
                Tag = mode
            };

            // Manually bind IsChecked (radio button behavior)
            item.Click += (s, e) => OnSetNotificationMode(mode);

            // Update checked state when ViewModel changes
            _viewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(TrayMenuViewModel.NotificationMode))
                {
                    item.IsChecked = _viewModel.NotificationMode == mode;
                }
            };

            // Set initial state
            item.IsChecked = _viewModel.NotificationMode == mode;

            notificationsMenu.Items.Add(item);
        }

        return notificationsMenu;
    }

    /// <summary>
    /// Handles the toggle awake command.
    /// </summary>
    private void OnToggleAwake()
    {
        _viewModel.IsAwake = !_viewModel.IsAwake;

        if (_viewModel.IsAwake)
        {
            // Activate
            _viewModel.CurrentState = AppState.Active;
            _powerService.SetAwakeState(true, _viewModel.KeepDisplayOn);
            _timerService.Stop();
            Debug.WriteLine("[TrayIconService] Toggled to Active");
        }
        else
        {
            // Deactivate
            _viewModel.CurrentState = AppState.Inactive;
            _powerService.SetAwakeState(false, false);
            _timerService.Stop();
            Debug.WriteLine("[TrayIconService] Toggled to Inactive");
        }

        SaveSettings();
    }

    /// <summary>
    /// Handles the "Active for" command.
    /// </summary>
    private void OnSetActiveFor(int? minutes)
    {
        _viewModel.IsAwake = true;
        _viewModel.CurrentState = AppState.Active;
        _powerService.SetAwakeState(true, _viewModel.KeepDisplayOn);

        if (minutes.HasValue)
        {
            _timerService.StartActiveForTimer(minutes.Value);
            Debug.WriteLine($"[TrayIconService] Active for {minutes} minutes");
        }
        else
        {
            _timerService.Stop();
            Debug.WriteLine("[TrayIconService] Active indefinitely");
        }

        SaveSettings();
    }

    /// <summary>
    /// Handles the "Inactive for" command.
    /// </summary>
    private void OnSetInactiveFor(int minutes)
    {
        _viewModel.IsAwake = false;
        _viewModel.CurrentState = AppState.Paused;
        _powerService.SetAwakeState(false, false);
        _timerService.StartPauseTimer(minutes);

        Debug.WriteLine($"[TrayIconService] Paused for {minutes} minutes");

        SaveSettings();
    }

    /// <summary>
    /// Handles toggling the "Keep display on" setting.
    /// </summary>
    private void OnToggleKeepDisplayOn()
    {
        _viewModel.KeepDisplayOn = !_viewModel.KeepDisplayOn;

        // Update power state if currently awake
        if (_viewModel.IsAwake)
        {
            _powerService.SetAwakeState(true, _viewModel.KeepDisplayOn);
        }

        SaveSettings();
    }

    /// <summary>
    /// Handles setting the notification mode.
    /// </summary>
    private void OnSetNotificationMode(NotificationMode mode)
    {
        _viewModel.NotificationMode = mode;
        SaveSettings();
    }

    /// <summary>
    /// Handles toggling the "Run at startup" setting.
    /// </summary>
    private void OnToggleRunAtStartup()
    {
        _viewModel.RunAtStartup = !_viewModel.RunAtStartup;
        _startupService.SetStartupEnabled(_viewModel.RunAtStartup);
        SaveSettings();
    }

    /// <summary>
    /// Handles toggling the "Show remaining time" setting.
    /// </summary>
    private void OnToggleShowRemainingTime()
    {
        _viewModel.ShowRemainingTime = !_viewModel.ShowRemainingTime;
        SaveSettings();
    }

    /// <summary>
    /// Handles showing the About dialog.
    /// </summary>
    private void OnShowAbout()
    {
        var aboutWindow = new AboutWindow
        {
            Owner = null
        };
        aboutWindow.ShowDialog();
    }

    /// <summary>
    /// Handles the exit command.
    /// </summary>
    private void OnExit()
    {
        Application.Current.Shutdown();
    }

    /// <summary>
    /// Handles double-click on the tray icon to toggle keep-awake state.
    /// </summary>
    private void OnTrayMouseDoubleClick(object sender, RoutedEventArgs e)
    {
        OnToggleAwake();
    }

    /// <summary>
    /// Handles ViewModel property changes to update the tray icon.
    /// </summary>
    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(TrayMenuViewModel.CurrentState))
        {
            UpdateIcon(_viewModel.CurrentState);
        }
        else if (e.PropertyName == nameof(TrayMenuViewModel.TooltipText))
        {
            _taskbarIcon.ToolTipText = _viewModel.TooltipText;
        }
    }

    /// <summary>
    /// Handles timer remaining time changes.
    /// </summary>
    private void OnTimerRemainingTimeChanged(object? sender, EventArgs e)
    {
        _viewModel.RemainingTime = _timerService.RemainingTime;
    }

    /// <summary>
    /// Updates the tray icon based on the application state.
    /// </summary>
    private void UpdateIcon(AppState state)
    {
        string iconPath = state switch
        {
            AppState.Active => "pack://application:,,,/Assets/Icons/tray-active.ico",
            AppState.Inactive => "pack://application:,,,/Assets/Icons/tray-inactive.ico",
            AppState.Paused => "pack://application:,,,/Assets/Icons/tray-paused.ico",
            _ => "pack://application:,,,/Assets/Icons/tray-active.ico"
        };

        var icon = LoadIconSafe(iconPath);
        if (icon != null)
        {
            _taskbarIcon.IconSource = icon;
            Debug.WriteLine($"[TrayIconService] Icon updated to {state}");
        }
    }

    /// <summary>
    /// Safely loads an icon from a URI, returning null if the icon cannot be loaded.
    /// </summary>
    /// <param name="iconPath">The pack:// URI to the icon resource.</param>
    /// <returns>The loaded BitmapImage or null if loading failed.</returns>
    private BitmapImage? LoadIconSafe(string iconPath)
    {
        try
        {
            var bitmap = new BitmapImage(new Uri(iconPath));
            return bitmap;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[TrayIconService] Failed to load icon {iconPath}: {ex.Message}");
            Debug.WriteLine("[TrayIconService] Icon files are missing. Please add tray-active.ico, tray-inactive.ico, and tray-paused.ico to Assets/Icons/");
            return null;
        }
    }

    /// <summary>
    /// Saves current settings to persistent storage.
    /// </summary>
    private void SaveSettings()
    {
        var settings = new UserSettings
        {
            KeepDisplayOn = _viewModel.KeepDisplayOn,
            NotificationMode = _viewModel.NotificationMode,
            RunAtStartup = _viewModel.RunAtStartup,
            ShowRemainingTime = _viewModel.ShowRemainingTime
        };

        _settingsService.Save(settings);
    }

    /// <summary>
    /// Disposes the TrayIconService and cleans up resources.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;

        _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
        _timerService.RemainingTimeChanged -= OnTimerRemainingTimeChanged;
        if (_taskbarIcon != null)
        {
            _taskbarIcon.TrayMouseDoubleClick -= OnTrayMouseDoubleClick;
            _taskbarIcon.Dispose();
        }

        _disposed = true;

        Debug.WriteLine("[TrayIconService] Disposed");
    }
}

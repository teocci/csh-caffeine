# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**Caffeine** is a Windows system tray application that prevents the PC from entering sleep mode or turning off the display. Built with C# 14 and .NET 10 (WPF), targeting Windows 10/11, designed for Microsoft Store distribution via MSIX packaging.

## Build & Development Commands

### Build
```bash
cd caffeine
dotnet build
```

### Run (Debug)
```bash
cd caffeine
dotnet run
```

### Clean Build
```bash
cd caffeine
dotnet clean
dotnet build --configuration Release
```

### Package for Release
```bash
cd caffeine
dotnet publish -c Release -r win-x64 --self-contained false
```

## High-Level Architecture

### Core Design Principle
This is a **system tray-only application** with NO main window. The app runs entirely from the Windows notification area.

### Power Management Strategy
Uses Windows `SetThreadExecutionState` API (via P/Invoke) with two operational modes:
- **Mode 1 (default)**: `ES_CONTINUOUS | ES_SYSTEM_REQUIRED | ES_DISPLAY_REQUIRED` - Prevents sleep AND keeps display on
- **Mode 2**: `ES_CONTINUOUS | ES_SYSTEM_REQUIRED` - Prevents sleep, allows display to turn off
- **Deactivate**: `SetThreadExecutionState(ES_CONTINUOUS)` - Restores normal power behavior

### Application States
1. **Active**: Keeping PC awake (indefinitely or for a set duration)
2. **Inactive**: Normal power behavior, app idle
3. **Paused**: Temporarily inactive, will auto-resume after specified duration

### Architectural Layers

#### 1. Services Layer (Core Business Logic)
- **PowerService**: Wraps `SetThreadExecutionState` P/Invoke calls, manages power state
- **TrayIconService**: System tray integration using H.NotifyIcon.Wpf, handles context menu, icon switching
- **TimerService**: Manages countdown timers for "Active for" and "Inactive for" features
- **NotificationService**: Toast notifications and sound playback when timers expire
- **SettingsService**: JSON persistence to `%APPDATA%\caffeine\settings.json`
- **StartupService**: Windows startup registry management (`HKCU\Software\Microsoft\Windows\CurrentVersion\Run`)

#### 2. Models Layer
- **AppState**: Enum for Active, Inactive, Paused states
- **NotificationMode**: Enum for notification preferences (Notification+Sound, Notification only, Sound only, Silent)
- **UserSettings**: Serializable settings class (KeepDisplayOn, NotificationMode, RunAtStartup, etc.)

#### 3. ViewModels Layer
- **TrayMenuViewModel**: Manages context menu state, binds to user actions
- **AboutViewModel**: Data for About dialog

#### 4. Views Layer
- **AboutWindow.xaml**: Minimalist about dialog with support links (Buy Me a Coffee, Patreon)

### WPF Application Lifecycle
- **App.xaml** must have `ShutdownMode="OnExplicitShutdown"` since there's no main window
- Application entry point initializes `TrayIconService` and `PowerService` in `OnStartup`
- Single instance enforcement via named Mutex (prevent multiple instances)
- Exit via tray menu triggers `Application.Current.Shutdown()`

### Key Implementation Requirements

#### System Tray (H.NotifyIcon.Wpf)
- Define `TaskbarIcon` resource in App.xaml
- Icon states: `tray-active.ico` (steaming cup), `tray-inactive.ico` (empty cup), `tray-paused.ico` (dimmed/paused)
- Context menu structure:
  - Keep awake toggle (ON/OFF with visual indicators)
  - "Active for..." submenu (Indefinitely, 15min, 30min, 45min, 1h, 2h, 4h, 6h, 8h, 24h)
  - "Inactive for..." submenu (same durations, grayed out when app is inactive)
  - Settings submenu (Keep display on, Notifications, Run at startup, Show remaining time)
  - About
  - Exit

#### Settings Persistence
- Store in `%APPDATA%\caffeine\settings.json` using `System.Text.Json`
- Settings: notification mode, run at startup, show remaining time, keep display on, last-used duration
- Load on startup, save on change

#### Notifications
- Use `Microsoft.WindowsAppSDK` for Windows toast notifications
- Four modes: Notification+Sound (default), Notification only, Sound only, Silent
- Triggered when "Active for" or "Inactive for" timers expire

#### Thread Safety
- `SetThreadExecutionState` must be called on a thread that remains alive (typically UI thread)
- Ensure PowerService operations execute on UI thread context

### Project Structure (Current State)
```
csh-caffeine/
├── csh-caffeine.sln              # Solution file
├── PRD.md                        # Product Requirements Document
├── CLAUDE.md                     # This file
└── caffeine/                     # Main project
    ├── caffeine.csproj           # .NET 10 WPF project (net10.0-windows7.0)
    ├── App.xaml                  # Application definition (needs TaskbarIcon resource)
    ├── App.xaml.cs               # Startup logic (needs OnStartup override)
    ├── MainWindow.xaml           # Default window (should be removed/hidden)
    ├── MainWindow.xaml.cs        # Default window code-behind
    └── AssemblyInfo.cs           # Assembly metadata
```

**Note**: As of initial setup, only the skeleton WPF project exists. Services, Models, ViewModels, and Views folders need to be created during implementation.

### NuGet Dependencies
```xml
<PackageReference Include="H.NotifyIcon.Wpf" Version="2.4.1" />
<PackageReference Include="Microsoft.WindowsAppSDK" Version="1.6.241114003" />
```

### Critical Implementation Notes

1. **No MainWindow**: Remove or hide MainWindow - this app should never show a main window, only About dialog when requested
2. **ShutdownMode**: Must set `ShutdownMode="OnExplicitShutdown"` in App.xaml
3. **Single Instance**: Implement Mutex-based single instance enforcement in App.xaml.cs OnStartup
4. **Icon Assets**: Need to create/add .ico files for tray icons (16x16, 24x24, 32x32, 48x48 sizes)
5. **P/Invoke Safety**: Ensure proper P/Invoke signature for `SetThreadExecutionState`:
   ```csharp
   [DllImport("kernel32.dll")]
   static extern uint SetThreadExecutionState(uint esFlags);

   const uint ES_CONTINUOUS = 0x80000000;
   const uint ES_SYSTEM_REQUIRED = 0x00000001;
   const uint ES_DISPLAY_REQUIRED = 0x00000002;
   ```

### Implementation Priority Order (from PRD.md)
1. PowerService - Core keep-awake functionality
2. TrayIconService - Basic tray icon with simple menu
3. Wire PowerService to tray menu toggle
4. TimerService - Add countdown logic for timed activation/pause
5. SettingsService - Persistence
6. NotificationService - Toast and sound
7. AboutWindow - Dialog with support links
8. StartupService - Windows startup integration
9. Icon assets and polish

### Target Platforms
- Windows 10 (version 1809+)
- Windows 11
- Distribution: Microsoft Store (MSIX packaging required)

### Known Limitations (from PRD.md)
- Cannot override Group Policy power settings (requires admin, not feasible)
- May drain battery faster on laptops when active
- Per-thread execution state must be maintained on a persistent thread

### Support Links (for About Dialog)
- Buy Me a Coffee: https://www.buymeacoffee.com/teocci
- Patreon: https://www.patreon.com/teocci
- GitHub: github.com/teocci/csh-caffeine

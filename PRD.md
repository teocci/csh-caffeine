# Caffeine - Windows Keep-Awake Application

## Project Overview

A lightweight Windows system tray application that prevents the PC from entering sleep mode or turning off the display. Designed for Microsoft Store distribution targeting Windows 10 and Windows 11.

---

## 1. Extracted Requirements

### 1.1 Core Functionality
- **Primary function**: Keep PC awake (prevent sleep and display timeout)
- **Method**: Must use efficient, non-intrusive API calls (NOT keystroke simulation or character injection)
- **Target platforms**: Windows 10 (1809+) and Windows 11
- **Distribution**: Microsoft Store (MSIX packaging required)

### 1.2 User Interface
- **No main window**: Application runs entirely from system tray
- **System tray menu** with the following items:
  - About dialog
  - "Active for" submenu (timed activation)
  - "Inactive for" submenu (pause and auto-resume)
  - Toggle button (Active/Inactive state)
  - Exit option
- **Visual indicators**:
  - Green = Active (keeping awake)
  - Red = Inactive (normal power behavior)
- **SVG icons** for all menu items

### 1.3 About Dialog
- Minimalistic design
- Support links:
  - Buy Me a Coffee: `https://www.buymeacoffee.com/teocci`
  - Patreon: `https://www.patreon.com/teocci`

### 1.4 Time Duration Options
Based on the provided screenshots:
- 15 minutes
- 30 minutes
- 45 minutes
- 1 hour
- 2 hours
- 4 hours
- 6 hours
- 8 hours
- 24 hours

---

## 2. Confirmed Decisions

| # | Decision | Details |
|---|----------|---------|
| D1 | App starts in **Active** state | User opens app to keep PC awake; immediate activation expected |
| D2 | Windows startup option included | User can opt-in to launch at login |
| D3 | Configurable notifications on timer expiry | Default: notification + sound; options for notification-only, sound-only, or silent |
| D4 | Tray icon changes visually | Steaming coffee cup (active) vs empty/no-steam cup (inactive) |
| D5 | Settings persisted between sessions | Remember notification preferences, last-used duration, startup setting |
| D6 | Single instance enforcement | Prevent multiple instances running |
| D7 | Separate "Keep display on" toggle | Default: ON; allows background tasks without screen staying on |

---

## 3. Technical Approach

### 3.1 Keep-Awake Implementation

**Recommended Method**: Windows `SetThreadExecutionState` API

**Two Modes of Operation:**

| Mode | Flags | Behavior |
|------|-------|----------|
| PC + Display ON | `ES_CONTINUOUS \| ES_SYSTEM_REQUIRED \| ES_DISPLAY_REQUIRED` | Prevents sleep AND keeps screen on |
| PC only (Display can turn off) | `ES_CONTINUOUS \| ES_SYSTEM_REQUIRED` | Prevents sleep, screen follows normal timeout |

**Default:** PC + Display ON (most common use case)

**Why this approach?**
- ✅ Official Windows API for power management
- ✅ No side effects (no fake keystrokes, no terminal pollution)
- ✅ Respects user's power settings when deactivated
- ✅ Works on both Windows 10 and 11
- ✅ Low resource usage
- ✅ Clean activation/deactivation

**Flags explained:**
- `ES_CONTINUOUS`: State remains until explicitly changed
- `ES_SYSTEM_REQUIRED`: Prevents system sleep
- `ES_DISPLAY_REQUIRED`: Prevents display timeout (optional based on user setting)

**Deactivation:**
```
SetThreadExecutionState(ES_CONTINUOUS)  // Resets to normal
```

### 3.2 Alternative Methods (Not Recommended)

| Method | Why NOT to use |
|--------|----------------|
| Keystroke simulation | Interferes with user input, terminal pollution |
| Mouse movement simulation | Can disrupt user work |
| PowerCreateRequest API | Overkill for this use case, more complex |
| Media playback tricks | Unreliable, hacky |

---

## 4. Language Evaluation

> **Full rubrics and calculations available in:** `LANGUAGE_RUBRICS.md`

### 4.1 Summary Results

| Rank | Language | Score | Key Strength | Key Weakness |
|------|----------|-------|--------------|--------------|
| 1 | **C# .NET 10 + Rider** | **4.60** | Best Store + tray integration | Binary size if self-contained |
| 2 | C++ with Qt | 4.05 | Excellent UI, cross-platform | Qt licensing, C++ complexity |
| 3 | C (Pure Win32) | 3.90 | Smallest binary, native API | Painful development |
| 4 | Go | 3.55 | Zero runtime, single binary | Limited UI/dialog options |
| 5 | Rust | 3.50 | Tiny binary, memory safe | Steep learning curve |
| 6 | Electron | 3.20 | Flexible web-based UI | 150-200MB for a tray app |
| 7 | Python 3.11 | 3.15 | Fastest prototyping | Packaging nightmare |
| 8 | Java | 2.55 | Mature ecosystem | JVM overhead, awkward Windows integration |
| 9 | Ruby | 1.75 | — | Wrong tool for Windows desktop |

### 4.2 Evaluation Criteria (Weights)

| Criterion | Weight | What It Measures |
|-----------|--------|------------------|
| Store Compatibility | 20% | MSIX tooling, certification path |
| System Tray Support | 15% | NotifyIcon/tray library quality |
| Win32 API Access | 15% | SetThreadExecutionState ease |
| Binary Size | 10% | Final package size |
| Development Velocity | 15% | Time-to-ship, iteration speed |
| Runtime Dependencies | 10% | What user must have installed |
| UI/Dialog Capability | 10% | About dialog, settings UI polish |
| Long-term Maintenance | 5% | Future-proofing, community |

See `LANGUAGE_RUBRICS.md` for detailed scoring indicators (what defines a 1 vs 3 vs 5 for each criterion).

### 4.3 Final Decision

| Aspect | Decision |
|--------|----------|
| **Language** | C# 14 |
| **Framework** | .NET 10 (LTS, supported until Nov 2028) |
| **IDE** | JetBrains Rider |
| **UI Framework** | WPF (pure WPF, no WinForms mixing) |
| **System Tray** | H.NotifyIcon.Wpf (NuGet package) |
| **Packaging** | MSIX via Rider or `dotnet publish` |

**Rationale:** Best overall fit for Microsoft Store distribution, excellent system tray support, fastest path to a polished product. .NET 10 is the current LTS with longest support window.

See `LANGUAGE_RUBRICS.md` for full evaluation methodology.

---

## 5. Feature Clarifications & Design Decisions

### 5.1 "Inactive for" Feature — CONFIRMED

**Behavior (Option D)**: Pause and Auto-Resume

- Only available when app is currently in **Active** state
- Immediately deactivates (allows PC to sleep)
- Automatically re-activates after the specified duration
- Use case: "I'm in a meeting for 1 hour, let PC sleep, but resume keeping awake after"

**State Machine:**
```
[Active] --"Inactive for 30 min"--> [Paused] --timer expires--> [Active]
```

**Menu behavior:**
- "Inactive for" submenu is **grayed out** when app is in Inactive state
- Selecting a duration immediately pauses and starts the resume countdown

---

### 5.2 Menu Structure (Final Design)

```
┌─────────────────────────────┐
│ ☕ Caffeine                 │
├─────────────────────────────┤
│ ● Keep awake          [ON] │  ← Green indicator when active
│ ○ Keep awake         [OFF] │  ← Red indicator when inactive
├─────────────────────────────┤
│ ⏱️ Active for...          ▶│  ← Submenu (how long to stay awake)
│ ⏸️ Inactive for...        ▶│  ← Submenu (pause duration, grayed if inactive)
├─────────────────────────────┤
│ ⚙️ Settings               ▶│  ← Submenu
│    ├─ 🖥️ Keep display on   │  ← Checkbox (default: ON)
│    ├─ 🔔 Notifications    ▶│  ← Sub-submenu (4 options)
│    ├─ 🚀 Run at startup    │  ← Checkbox toggle
│    └─ 📊 Show remaining    │  ← Checkbox (tooltip display)
├─────────────────────────────┤
│ ℹ️ About                   │
│ 🚪 Exit                    │
└─────────────────────────────┘
```

**"Active for" submenu:**
```
├─ Indefinitely (default)
├─ 15 minutes
├─ 30 minutes
├─ 45 minutes
├─ 1 hour
├─ 2 hours
├─ 4 hours
├─ 6 hours
├─ 8 hours
└─ 24 hours
```

**"Inactive for" submenu:** (same durations, minus "Indefinitely")

**Notifications submenu:**
```
├─ ✓ Notification + Sound (default)
├─ Notification only
├─ Sound only
└─ Silent
```

---

### 5.3 Tooltip Behavior

When hovering over tray icon, show contextual information:

| State | Tooltip Example |
|-------|-----------------|
| Active (indefinitely, display ON) | `Caffeine: Active` |
| Active (indefinitely, display OFF) | `Caffeine: Active (display can sleep)` |
| Active (timed) | `Caffeine: Active (1h 23m remaining)` |
| Paused (inactive for) | `Caffeine: Paused (resumes in 45m)` |
| Inactive | `Caffeine: Inactive` |

---

## 6. Limitations & Constraints

| Constraint | Impact | Mitigation |
|------------|--------|------------|
| Microsoft Store certification | Requires MSIX, app manifest, privacy policy | Use Rider MSIX packaging or `dotnet publish` |
| Windows Defender SmartScreen | New publishers face warnings initially | Get EV code signing certificate (or build reputation) |
| No admin privileges | Cannot override Group Policy power settings | Document limitation in About dialog |
| Battery-powered devices | Keeping awake drains battery faster | Show warning tooltip on laptops |
| Single-thread execution state | Per-thread, must maintain on main thread | Ensure PowerService runs on UI thread |

### 6.1 About Dialog Content

```
┌─────────────────────────────────────────┐
│           ☕ Caffeine v1.0.0            │
│                                         │
│   Keep your PC awake, effortlessly.     │
│                                         │
│   © 2026 Teocci                         │
│                                         │
│   ⚠️ Note: Cannot override Group        │
│   Policy or corporate power settings.   │
│   May increase battery drain on         │
│   laptops.                              │
│                                         │
│   ─────────────────────────────────     │
│                                         │
│   Support Development:                  │
│                                         │
│   [☕ Buy Me a Coffee]  [🎨 Patreon]    │
│                                         │
│   github.com/teocci/csh-caffeine        │
│                                         │
└─────────────────────────────────────────┘
```

---

## 7. Project Structure (C# / .NET 10)

```
csh-caffeine/                            # Solution root
├── .idea/                               # Rider configuration (auto-generated)
├── csh-caffeine.sln                     # Solution file
│
├── caffeine/                            # Main project folder
│   ├── caffeine.csproj                  # Project file
│   ├── App.xaml                         # Application definition
│   ├── App.xaml.cs                      # Application startup logic
│   ├── AssemblyInfo.cs                  # Assembly metadata
│   │
│   ├── Models/
│   │   ├── AppState.cs                  # Active, Inactive, Paused enum
│   │   ├── NotificationMode.cs          # Notification preference enum
│   │   └── UserSettings.cs              # Serializable settings (KeepDisplayOn, etc.)
│   │
│   ├── Services/
│   │   ├── PowerService.cs              # SetThreadExecutionState wrapper
│   │   ├── TrayIconService.cs           # System tray (H.NotifyIcon.Wpf)
│   │   ├── TimerService.cs              # Duration countdown (active/pause)
│   │   ├── NotificationService.cs       # Toast + sound handling
│   │   ├── SettingsService.cs           # JSON persistence
│   │   └── StartupService.cs            # Windows startup registry
│   │
│   ├── ViewModels/
│   │   ├── TrayMenuViewModel.cs         # Context menu state
│   │   └── AboutViewModel.cs
│   │
│   ├── Views/
│   │   └── AboutWindow.xaml             # Minimalist about dialog
│   │
│   ├── Assets/
│   │   ├── Icons/
│   │   │   ├── tray-active.ico          # Steaming cup (multi-size)
│   │   │   ├── tray-inactive.ico        # Empty cup (multi-size)
│   │   │   ├── tray-paused.ico          # Paused state (multi-size)
│   │   │   └── app-logo.ico             # App icon for About/Store
│   │   ├── Sounds/
│   │   │   └── notification.wav         # Subtle timer-end sound
│   │   └── Images/
│   │       ├── buymeacoffee.png         # Support button
│   │       └── patreon.png              # Support button
│   │
│   ├── Resources/
│   │   └── Strings.resx                 # Localization-ready strings
│   │
│   ├── bin/                             # Build output (auto-generated)
│   └── obj/                             # Build intermediates (auto-generated)
│
├── caffeine.Tests/                  # Test project (add later)
│   ├── caffeine.Tests.csproj
│   ├── PowerServiceTests.cs
│   └── TimerServiceTests.cs
│
└── packaging/                           # MSIX packaging (add for Store)
    └── caffeine.Package/
        ├── Package.appxmanifest
        ├── Assets/
        │   ├── StoreLogo.png
        │   ├── Square44x44Logo.png
        │   ├── Square150x150Logo.png
        │   └── Wide310x150Logo.png
        └── caffeine.Package.wapproj
```

### 7.1 NuGet Packages Required

```xml
<!-- Add to caffeine.csproj -->
<ItemGroup>
    <PackageReference Include="H.NotifyIcon.Wpf" Version="2.4.1" />
    <PackageReference Include="Microsoft.WindowsAppSDK" Version="1.6.241114003" />
</ItemGroup>
```

**Install via CLI (from project folder):**
```bash
cd caffeine
dotnet add package H.NotifyIcon.Wpf --version 2.4.1
dotnet add package Microsoft.WindowsAppSDK --version 1.6.241114003
```

### 7.2 Key Implementation Notes

**Settings Persistence:**
- Store in `%APPDATA%\caffeine\settings.json`
- Settings include: notification mode, run at startup, show remaining time, keep display on, last-used duration

**Windows Startup:**
- Use Registry key: `HKCU\Software\Microsoft\Windows\CurrentVersion\Run`
- Or use Windows Task Scheduler for more reliability

**Notification Sound:**
- Use `Microsoft.WindowsAppSDK` for toast notifications with system sounds
- Or `System.Media.SoundPlayer` for custom WAV playback

---

## 8. Icon Design Specifications

### 8.1 Tray Icons (16x16, 24x24, 32x32, 48x48)

| State | Design | Colors |
|-------|--------|--------|
| **Active** | Coffee cup WITH steam wisps | Cup: Brown (#6F4E37), Steam: Green (#4CAF50) |
| **Inactive** | Coffee cup WITHOUT steam (empty look) | Cup: Gray (#9E9E9E) |
| **Paused** | Coffee cup with "pause" indicator or dimmed steam | Cup: Brown, Steam: Orange (#FF9800) |

**Design notes:**
- Steam should be 2-3 curved lines above the cup
- Minimal detail for clarity at small sizes
- High contrast for visibility on both light/dark taskbars

### 8.2 Menu Icons (SVG, 16x16)

| Menu Item | Icon Description | Suggested Icon |
|-----------|------------------|----------------|
| Keep awake ON | Filled circle | `●` Green (#4CAF50) |
| Keep awake OFF | Outlined circle | `○` Red (#F44336) |
| Active for... | Timer/stopwatch | Clock with play indicator |
| Inactive for... | Pause symbol | `⏸` Pause bars |
| Settings | Gear | `⚙` Cog wheel |
| Keep display on | Monitor/screen | `🖥️` Display icon |
| Notifications | Bell | `🔔` Bell icon |
| Run at startup | Rocket | `🚀` Launch icon |
| Show remaining | Chart/timer | `📊` or `⏱` |
| About | Info circle | `ℹ` Information |
| Exit | Door with arrow | `🚪` Exit door |

### 8.3 About Dialog Assets

- App logo: 128x128 coffee cup (high detail version)
- Buy Me a Coffee button: Use official BMC branding (yellow #FFDD00)
- Patreon button: Use official Patreon branding (coral #FF424D)

---

## 9. Open Questions

| # | Question | Status |
|---|----------|--------|
| 1 | "Inactive for" feature behavior | ✅ RESOLVED: Pause and auto-resume |
| 2 | Startup behavior | ✅ RESOLVED: Start in Active state |
| 3 | Windows startup option | ✅ RESOLVED: Include (opt-in setting) |
| 4 | Timer notifications | ✅ RESOLVED: Configurable (4 modes) |
| 5 | Tray icon animation | ✅ RESOLVED: Steaming/non-steaming visual |
| 6 | Implementation language | ✅ RESOLVED: **C# 14 / .NET 10 with JetBrains Rider** |

**All questions resolved. Document ready for implementation.**

---

## 10. Next Steps

### Implementation Checklist

1. ✅ Finalize requirements (this document)
2. ✅ Choose implementation language (C# 14 / .NET 10 + Rider)
3. ⬜ Set up .NET 10 solution in Rider
4. ⬜ Create icon assets (SVG → ICO conversion)
5. ⬜ Implement `PowerService` (SetThreadExecutionState wrapper)
6. ⬜ Implement `TrayIconService` (H.NotifyIcon.Wpf + context menu)
7. ⬜ Implement `TimerService` (active/pause countdowns)
8. ⬜ Implement `NotificationService` (toast + sound)
9. ⬜ Implement `SettingsService` (JSON persistence)
10. ⬜ Implement `StartupService` (Windows startup registry)
11. ⬜ Create About dialog (WPF window)
12. ⬜ Package as MSIX
13. ⬜ Test on Windows 10 & 11
14. ⬜ Submit to Microsoft Store

### Tech Stack Summary

| Component | Technology |
|-----------|------------|
| Language | C# 14 |
| Runtime | .NET 10 (LTS, supported until Nov 2028) |
| IDE | JetBrains Rider |
| UI (Dialogs) | WPF |
| System Tray | H.NotifyIcon.Wpf |
| Win32 Interop | P/Invoke |
| Settings Storage | JSON in %APPDATA% (System.Text.Json) |
| Notifications | Microsoft.WindowsAppSDK |
| Packaging | MSIX |
| Distribution | Microsoft Store |

---

## 11. Claude Code Instructions

This document is designed to be used with Claude Code for implementation. The project has already been created in JetBrains Rider.

### Project Location

```
D:\Teocci\Devs\csharp\csh-caffeine\
├── csh-caffeine.sln
└── caffeine\
    └── caffeine.csproj
```

### First Steps (Package Installation)

```bash
# Navigate to project folder
cd D:\Teocci\Devs\csharp\csh-caffeine\caffeine

# Install required packages
dotnet add package H.NotifyIcon.Wpf --version 2.4.1
dotnet add package Microsoft.WindowsAppSDK --version 1.6.241114003
```

### Key Implementation Priorities

1. **Start with `PowerService`** - This is the core functionality. Get `SetThreadExecutionState` working first.

2. **Then `TrayIconService`** - Get a basic tray icon with a simple menu using H.NotifyIcon.Wpf. Test that it appears in notification area.

3. **Wire them together** - Menu toggle should call PowerService to enable/disable.

4. **Add timers** - Implement countdown logic for "Active for" and "Inactive for" features.

5. **Add polish** - Settings persistence, About dialog, notifications, icons.

### Critical Implementation Notes

- **Single Instance:** Use a named Mutex to prevent multiple instances
- **Thread Safety:** `SetThreadExecutionState` must be called on the thread that will remain alive
- **Icon Sizes:** Include 16x16, 24x24, 32x32, 48x48 in ICO files for proper DPI scaling
- **Settings Path:** Use `Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)`
- **Startup Registry:** `HKCU\Software\Microsoft\Windows\CurrentVersion\Run`
- **No MainWindow:** Set `ShutdownMode="OnExplicitShutdown"` in App.xaml since app runs from tray only

### H.NotifyIcon.Wpf Quick Start

**Note:** H.NotifyIcon.Wpf provides the same API as Hardcodet.NotifyIcon.Wpf with identical `TaskbarIcon` class and properties. The project will create the TaskbarIcon programmatically in `TrayIconService.cs` rather than in App.xaml for better service integration.

```xml
<!-- App.xaml - Add namespace and ShutdownMode -->
<Application x:Class="caffeine.App"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:tb="http://www.hardcodet.net/taskbar"
             ShutdownMode="OnExplicitShutdown">
    <Application.Resources>
        <tb:TaskbarIcon x:Key="TrayIcon"
                        IconSource="/Assets/Icons/tray-active.ico"
                        ToolTipText="Caffeine: Active">
            <tb:TaskbarIcon.ContextMenu>
                <ContextMenu>
                    <MenuItem Header="Keep awake" IsCheckable="True" IsChecked="True"/>
                    <Separator/>
                    <MenuItem Header="About"/>
                    <MenuItem Header="Exit"/>
                </ContextMenu>
            </tb:TaskbarIcon.ContextMenu>
        </tb:TaskbarIcon>
    </Application.Resources>
</Application>
```

### Reference Files

- `CLAUDE.md` - This document (requirements and design)
- `LANGUAGE_RUBRICS.md` - Language evaluation methodology (for reference only)

---

## Appendix A: SetThreadExecutionState Reference

```csharp
[DllImport("kernel32.dll")]
static extern uint SetThreadExecutionState(uint esFlags);

const uint ES_CONTINUOUS = 0x80000000;
const uint ES_SYSTEM_REQUIRED = 0x00000001;
const uint ES_DISPLAY_REQUIRED = 0x00000002;

// Mode 1: Keep PC awake + Display ON (default)
SetThreadExecutionState(ES_CONTINUOUS | ES_SYSTEM_REQUIRED | ES_DISPLAY_REQUIRED);

// Mode 2: Keep PC awake + Display can turn off (for background tasks)
SetThreadExecutionState(ES_CONTINUOUS | ES_SYSTEM_REQUIRED);

// Deactivate (return to normal power behavior)
SetThreadExecutionState(ES_CONTINUOUS);
```

### PowerService Implementation Logic

```csharp
public void SetAwakeState(bool keepAwake, bool keepDisplayOn)
{
    uint flags = ES_CONTINUOUS;
    
    if (keepAwake)
    {
        flags |= ES_SYSTEM_REQUIRED;
        
        if (keepDisplayOn)
        {
            flags |= ES_DISPLAY_REQUIRED;
        }
    }
    
    SetThreadExecutionState(flags);
}
```

---

*Document Version: 1.3 (Final)*
*Last Updated: 2026-01-20*
*Status: Ready for Implementation with Claude Code*

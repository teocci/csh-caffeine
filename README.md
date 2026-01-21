# Caffeine

A lightweight Windows system tray application that prevents your PC from entering sleep mode or turning off the display.

## Features

- **Keep Awake**: Prevents Windows from entering sleep mode
- **Display Control**: Option to keep the display on or allow it to turn off
- **Timed Activation**: Set the app to stay active for a specific duration (15min, 30min, 1h, 2h, 4h, 6h, 8h, 24h)
- **Pause Feature**: Temporarily disable keep-awake with auto-resume
- **System Tray Only**: Runs quietly in your notification area - no intrusive windows
- **Notifications**: Configurable alerts when timers expire
- **Startup Integration**: Option to launch automatically with Windows

## Requirements

- Windows 10 (version 1809+) or Windows 11
- .NET 10 Runtime

## Installation

### Microsoft Store
Coming soon.

### Manual Installation
1. Download the latest release from the [Releases](https://github.com/teocci/csh-caffeine/releases) page
2. Extract and run `caffeine.exe`

## Building from Source

```bash
# Clone the repository
git clone https://github.com/teocci/csh-caffeine.git
cd csh-caffeine

# Build
cd caffeine
dotnet build

# Run
dotnet run

# Build for release
dotnet publish -c Release -r win-x64 --self-contained false
```

## Usage

1. Launch Caffeine - it will appear in your system tray
2. Right-click the tray icon to access the menu:
   - **Keep awake**: Toggle to activate/deactivate
   - **Active for...**: Set a timer for how long to keep the PC awake
   - **Inactive for...**: Pause the app temporarily
   - **Settings**: Configure display behavior, notifications, and startup options
   - **About**: View app information
   - **Exit**: Close the application

### Tray Icon States
- **Steaming cup**: Active - keeping your PC awake
- **Empty cup**: Inactive - normal power behavior
- **Dimmed cup**: Paused - temporarily inactive

## How It Works

Caffeine uses the Windows `SetThreadExecutionState` API to signal to the operating system that the system is in use and should not enter sleep mode. This is the same mechanism used by video players and presentation software.

## Support the Project

If you find Caffeine useful, consider supporting its development:

[![Buy Me A Coffee](https://img.shields.io/badge/Buy%20Me%20A%20Coffee-ffdd00?style=for-the-badge&logo=buy-me-a-coffee&logoColor=black)](https://www.buymeacoffee.com/teocci)

[![Patreon](https://img.shields.io/badge/Patreon-F96854?style=for-the-badge&logo=patreon&logoColor=white)](https://www.patreon.com/teocci)

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Links

- **Repository**: [https://github.com/teocci/csh-caffeine](https://github.com/teocci/csh-caffeine)
- **Issues**: [https://github.com/teocci/csh-caffeine/issues](https://github.com/teocci/csh-caffeine/issues)
- **Buy Me a Coffee**: [https://www.buymeacoffee.com/teocci](https://www.buymeacoffee.com/teocci)
- **Patreon**: [https://www.patreon.com/teocci](https://www.patreon.com/teocci)

# Icon Assets Required

This application requires three ICO files for the system tray icon:

## Required Files

### 1. tray-active.ico
- **State**: Application is actively keeping the PC awake
- **Design**: Brown coffee cup with green steam
- **Sizes**: 16x16, 24x24, 32x32, 48x48 pixels
- **Description**: Indicates the app is preventing sleep and display timeout

### 2. tray-inactive.ico
- **State**: Application is inactive, normal power behavior
- **Design**: Gray coffee cup, no steam
- **Sizes**: 16x16, 24x24, 32x32, 48x48 pixels
- **Description**: Indicates the app is idle and PC can sleep normally

### 3. tray-paused.ico
- **State**: Application is temporarily paused (will auto-resume)
- **Design**: Brown coffee cup with orange/yellow steam
- **Sizes**: 16x16, 24x24, 32x32, 48x48 pixels
- **Description**: Indicates the app is paused for a set duration

## Icon Source Files

The `Sources/` directory contains:
- **SVG source files** - Vector artwork for each icon state
- **Python conversion script** - Generates multi-resolution ICO files from code
- **Verification scripts** - Validate ICO file structure

### SVG Files
- `tray-active.svg` - Brown coffee cup with green steam
- `tray-inactive.svg` - Gray coffee cup without steam
- `tray-paused.svg` - Brown coffee cup with orange steam

## Regenerating Icons

If you need to regenerate the ICO files (e.g., to modify the design):

### Prerequisites
```bash
pip install pillow
```

### Generate ICO Files
```bash
cd Sources
python create_multisize_ico.py
```

This creates proper multi-resolution ICO files with all required sizes (16x16, 24x24, 32x32, 48x48).

### Verify ICO Files
```bash
cd Sources
python verify_ico_structure.py
```

This validates that all ICO files contain the correct multi-resolution structure.

## Icon Design

The current icons feature:
- **Active**: Brown cup (#8B4513) with green steam (#4CAF50)
- **Inactive**: Gray cup (#808080) with no steam
- **Paused**: Brown cup (#8B4513) with orange steam (#FF9800)

Design optimized for clarity at 16x16 pixels (system tray size).

## Alternative Creation Methods

If Python is not available, you can use:
- **ImageMagick** - Convert SVG to ICO: `magick tray-active.svg -define icon:auto-resize=16,24,32,48 tray-active.ico`
- **IcoFX** (Windows) - Import PNG and export as ICO
- **GIMP** (Cross-platform) - Export as ICO format
- **Online converters** - Upload PNG/SVG and download ICO

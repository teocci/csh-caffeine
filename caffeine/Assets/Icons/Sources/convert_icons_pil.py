"""
Icon Conversion Script (PIL-only version)
Creates multi-resolution ICO files for Windows system tray using PIL drawing.
Works on Windows without external dependencies.

Requirements:
    pip install pillow

Usage:
    python convert_icons_pil.py
"""

from PIL import Image, ImageDraw
import os


def draw_coffee_cup(draw, size, cup_color, steam_color=None):
    """
    Draw a coffee cup icon at the specified size.

    Args:
        draw: ImageDraw object
        size: Icon size (width/height)
        cup_color: RGB tuple for cup color
        steam_color: RGB tuple for steam color (None for no steam)
    """
    # Scale factors based on icon size
    scale = size / 48.0

    # Cup dimensions (scaled)
    cup_x = int(16 * scale)
    cup_y = int(22 * scale)
    cup_width = int(16 * scale)
    cup_height = int(18 * scale)
    cup_radius = int(2 * scale)

    # Draw cup body
    draw.rounded_rectangle(
        [cup_x, cup_y, cup_x + cup_width, cup_y + cup_height],
        radius=cup_radius,
        fill=cup_color
    )

    # Draw coffee surface (darker shade)
    surface_color = tuple(int(c * 0.7) for c in cup_color)
    center_x = cup_x + cup_width // 2
    surface_y = cup_y
    surface_rx = int(8 * scale)
    surface_ry = int(2 * scale)

    draw.ellipse(
        [center_x - surface_rx, surface_y - surface_ry,
         center_x + surface_rx, surface_y + surface_ry],
        fill=surface_color
    )

    # Draw cup handle (arc)
    handle_width = int(2.5 * scale)
    if handle_width < 1:
        handle_width = 1

    handle_x = cup_x + cup_width
    handle_y1 = int(28 * scale)
    handle_y2 = int(36 * scale)
    handle_extend = int(4 * scale)

    # Draw handle as a thick arc
    for i in range(handle_width):
        draw.arc(
            [handle_x + i, handle_y1, handle_x + handle_extend, handle_y2],
            start=270, end=90,
            fill=cup_color,
            width=1
        )

    # Draw steam if specified
    if steam_color:
        steam_width = max(1, int(2 * scale))

        # Steam line 1 (left)
        steam1_x = int(20 * scale)
        draw.arc(
            [steam1_x - 1, int(14 * scale), steam1_x + 3, int(18 * scale)],
            start=180, end=0,
            fill=steam_color,
            width=steam_width
        )

        # Steam line 2 (center)
        steam2_x = int(24 * scale)
        draw.arc(
            [steam2_x - 1, int(11 * scale), steam2_x + 3, int(16 * scale)],
            start=180, end=0,
            fill=steam_color,
            width=steam_width
        )

        # Steam line 3 (right)
        steam3_x = int(28 * scale)
        draw.arc(
            [steam3_x - 1, int(14 * scale), steam3_x + 3, int(18 * scale)],
            start=180, end=0,
            fill=steam_color,
            width=steam_width
        )


def create_icon(name, cup_color, steam_color=None, sizes=[16, 24, 32, 48]):
    """
    Create a multi-resolution ICO file.

    Args:
        name: Icon name (e.g., 'tray-active')
        cup_color: RGB tuple for cup color
        steam_color: RGB tuple for steam color (None for no steam)
        sizes: List of icon sizes to include
    """
    print(f"Creating {name}.ico...")

    images = []

    for size in sizes:
        # Create transparent image
        img = Image.new('RGBA', (size, size), (0, 0, 0, 0))
        draw = ImageDraw.Draw(img)

        # Draw coffee cup
        draw_coffee_cup(draw, size, cup_color, steam_color)

        images.append(img)
        print(f"  - Generated {size}x{size}px")

    # Save as multi-resolution ICO
    script_dir = os.path.dirname(os.path.abspath(__file__))
    parent_dir = os.path.dirname(script_dir)
    ico_path = os.path.join(parent_dir, f'{name}.ico')

    # PIL requires all images to be in the append_images list for multi-resolution ICO
    # Don't use the first image separately
    images[0].save(
        ico_path,
        format='ICO',
        append_images=images[1:],
        bitmap_format='bmp'
    )

    print(f"  [OK] Created {ico_path}\n")


def main():
    """Create all three tray icons."""
    print("=" * 50)
    print("Caffeine Icon Creator")
    print("=" * 50)
    print()

    # Color definitions
    BROWN = (139, 69, 19)      # #8B4513
    GRAY = (128, 128, 128)      # #808080
    GREEN = (76, 175, 80)       # #4CAF50
    ORANGE = (255, 152, 0)      # #FF9800

    # Create active icon (brown cup with green steam)
    create_icon('tray-active', BROWN, GREEN)

    # Create inactive icon (gray cup without steam)
    create_icon('tray-inactive', GRAY, None)

    # Create paused icon (brown cup with orange steam)
    create_icon('tray-paused', BROWN, ORANGE)

    print("=" * 50)
    print("Icon creation complete!")
    print("=" * 50)


if __name__ == '__main__':
    main()

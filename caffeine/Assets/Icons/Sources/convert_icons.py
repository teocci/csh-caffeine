"""
Icon Conversion Script
Converts SVG source files to multi-resolution ICO files for Windows system tray.

Requirements:
    pip install cairosvg pillow

Usage:
    python convert_icons.py
"""

from PIL import Image
import io
import os

try:
    from cairosvg import svg2png
except ImportError:
    print("Error: cairosvg is not installed.")
    print("Please install required dependencies:")
    print("  pip install cairosvg pillow")
    exit(1)


def svg_to_ico(svg_path, ico_path, sizes=[16, 24, 32, 48]):
    """
    Convert SVG to multi-resolution ICO file.

    Args:
        svg_path: Path to source SVG file
        ico_path: Path to output ICO file
        sizes: List of icon sizes to include (default: [16, 24, 32, 48])
    """
    print(f"Converting {os.path.basename(svg_path)}...")

    images = []

    for size in sizes:
        # Convert SVG to PNG at specific size
        png_data = svg2png(url=svg_path, output_width=size, output_height=size)
        img = Image.open(io.BytesIO(png_data))
        images.append(img)
        print(f"  - Generated {size}x{size}px")

    # Save as multi-resolution ICO
    images[0].save(
        ico_path,
        format='ICO',
        sizes=[(img.width, img.height) for img in images],
        append_images=images[1:]
    )

    print(f"  ✓ Created {ico_path}\n")


def main():
    """Convert all three tray icons."""
    script_dir = os.path.dirname(os.path.abspath(__file__))
    parent_dir = os.path.dirname(script_dir)

    icons = [
        ('tray-active.svg', 'tray-active.ico'),
        ('tray-inactive.svg', 'tray-inactive.ico'),
        ('tray-paused.svg', 'tray-paused.ico')
    ]

    print("=" * 50)
    print("Caffeine Icon Converter")
    print("=" * 50)
    print()

    for svg_name, ico_name in icons:
        svg_path = os.path.join(script_dir, svg_name)
        ico_path = os.path.join(parent_dir, ico_name)

        if not os.path.exists(svg_path):
            print(f"Warning: {svg_name} not found, skipping...")
            continue

        try:
            svg_to_ico(svg_path, ico_path)
        except Exception as e:
            print(f"Error converting {svg_name}: {e}\n")

    print("=" * 50)
    print("Conversion complete!")
    print("=" * 50)


if __name__ == '__main__':
    main()

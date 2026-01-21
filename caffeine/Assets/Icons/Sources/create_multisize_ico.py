"""
Multi-Resolution ICO Creator
Creates proper multi-resolution ICO files by manually constructing the ICO file format.

Requirements:
    pip install pillow

Usage:
    python create_multisize_ico.py
"""

from PIL import Image, ImageDraw
import struct
import io
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
    cup_radius = max(1, int(2 * scale))

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
    surface_ry = max(1, int(2 * scale))

    draw.ellipse(
        [center_x - surface_rx, surface_y - surface_ry,
         center_x + surface_rx, surface_y + surface_ry],
        fill=surface_color
    )

    # Draw cup handle (arc)
    handle_width = max(1, int(2.5 * scale))
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


def create_ico_file(images, output_path):
    """
    Manually create a multi-resolution ICO file.

    Args:
        images: List of PIL Image objects (must be square)
        output_path: Path to output ICO file
    """
    # ICO file structure:
    # - ICONDIR header (6 bytes)
    # - ICONDIRENTRY for each image (16 bytes each)
    # - PNG or BMP data for each image

    ico_data = io.BytesIO()

    # Write ICONDIR header
    ico_data.write(struct.pack('<HHH', 0, 1, len(images)))  # Reserved, Type (1=ICO), Count

    # Prepare image data
    image_data_list = []
    for img in images:
        # Convert to PNG
        png_buffer = io.BytesIO()
        img.save(png_buffer, format='PNG')
        png_data = png_buffer.getvalue()
        image_data_list.append(png_data)

    # Calculate offsets for image data
    offset = 6 + (16 * len(images))  # Header + all ICONDIRENTRY structures

    # Write ICONDIRENTRY for each image
    for img, png_data in zip(images, image_data_list):
        width = img.width
        height = img.height

        # Width and height (0 means 256)
        w = width if width < 256 else 0
        h = height if height < 256 else 0

        ico_data.write(struct.pack('<BBBBHHII',
            w,                    # Width
            h,                    # Height
            0,                    # Color palette size (0 for PNG)
            0,                    # Reserved
            1,                    # Color planes
            32,                   # Bits per pixel
            len(png_data),        # Size of image data
            offset                # Offset to image data
        ))

        offset += len(png_data)

    # Write all image data
    for png_data in image_data_list:
        ico_data.write(png_data)

    # Write to file
    with open(output_path, 'wb') as f:
        f.write(ico_data.getvalue())


def create_icon(name, cup_color, steam_color=None, sizes=[16, 24, 32, 48]):
    """
    Create a multi-resolution ICO file with coffee cup icon.

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

    create_ico_file(images, ico_path)

    print(f"  [OK] Created {ico_path}\n")


def main():
    """Create all three tray icons."""
    print("=" * 50)
    print("Caffeine Multi-Resolution Icon Creator")
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

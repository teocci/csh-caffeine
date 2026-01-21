#!/usr/bin/env python3
"""Create multi-resolution ICO files for tray icons and app icon using Pillow."""

import os
import struct
from io import BytesIO
from PIL import Image, ImageDraw

# Icon sizes for Windows ICO (standard Windows icon sizes)
SIZES = [16, 24, 32, 48, 256]

# Source and output directories
SCRIPT_DIR = os.path.dirname(os.path.abspath(__file__))
OUTPUT_DIR = os.path.dirname(SCRIPT_DIR)

# Colors
BACKGROUND_COLOR = "#ffa01a"  # Orange background
CUP_COLOR = "#000000"  # Black cup
STATUS_COLORS = {
    "active": "#4CAF50",   # Green
    "inactive": "#9E9E9E", # Grey
    "paused": "#FFC107"    # Amber
}


def draw_app_icon(size: int) -> Image.Image:
    """Draw the main app icon at the specified size (no status indicator)."""
    # Create image with transparency
    img = Image.new('RGBA', (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)

    # Scale factor from 48x48 base
    scale = size / 48

    # Draw orange background circle
    bg_center = 24 * scale
    bg_radius = 18.24 * scale
    bg_color = hex_to_rgb(BACKGROUND_COLOR)
    draw.ellipse(
        [bg_center - bg_radius, bg_center - bg_radius,
         bg_center + bg_radius, bg_center + bg_radius],
        fill=bg_color + (255,)
    )

    # Draw cup
    offset_x = 7.2 * scale
    offset_y = 7.2 * scale
    cup_scale = 0.7 * scale

    # Cup lid
    lid_left = offset_x + 12 * cup_scale
    lid_top = offset_y + 9 * cup_scale
    lid_right = offset_x + 36 * cup_scale
    lid_bottom = offset_y + 14 * cup_scale
    cup_color = hex_to_rgb(CUP_COLOR)
    draw.rectangle([lid_left, lid_top, lid_right, lid_bottom], fill=cup_color + (255,))

    # Cup body
    body_points = [
        (offset_x + 14.1 * cup_scale, offset_y + 16 * cup_scale),
        (offset_x + 33.9 * cup_scale, offset_y + 16 * cup_scale),
        (offset_x + 31.1 * cup_scale, offset_y + 42 * cup_scale),
        (offset_x + 16.9 * cup_scale, offset_y + 42 * cup_scale),
    ]
    draw.polygon(body_points, fill=cup_color + (255,))

    # Inner cutout (hollow cup)
    inner_margin = max(1, 2 * scale)
    inner_points = [
        (body_points[0][0] + inner_margin, body_points[0][1] + inner_margin),
        (body_points[1][0] - inner_margin, body_points[1][1] + inner_margin),
        (body_points[2][0] - inner_margin, body_points[2][1] - inner_margin),
        (body_points[3][0] + inner_margin, body_points[3][1] - inner_margin),
    ]
    draw.polygon(inner_points, fill=bg_color + (255,))

    # Lid line detail
    line_y = offset_y + 11 * cup_scale
    line_height = max(1, 1 * scale)
    draw.rectangle(
        [lid_left + inner_margin, line_y,
         lid_right - inner_margin, line_y + line_height],
        fill=bg_color + (255,)
    )

    return img


def hex_to_rgb(hex_color: str) -> tuple:
    """Convert hex color to RGB tuple."""
    hex_color = hex_color.lstrip('#')
    return tuple(int(hex_color[i:i+2], 16) for i in (0, 2, 4))


def draw_icon(size: int, status: str) -> Image.Image:
    """Draw a tray icon at the specified size with the given status color."""
    # Create image with transparency
    img = Image.new('RGBA', (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)

    # Scale factor from 48x48 base
    scale = size / 48

    # Draw orange background circle
    bg_center = 24 * scale
    bg_radius = 18.24 * scale
    bg_color = hex_to_rgb(BACKGROUND_COLOR)
    draw.ellipse(
        [bg_center - bg_radius, bg_center - bg_radius,
         bg_center + bg_radius, bg_center + bg_radius],
        fill=bg_color + (255,)
    )

    # Draw cup
    offset_x = 7.2 * scale
    offset_y = 7.2 * scale
    cup_scale = 0.7 * scale

    # Cup lid
    lid_left = offset_x + 12 * cup_scale
    lid_top = offset_y + 9 * cup_scale
    lid_right = offset_x + 36 * cup_scale
    lid_bottom = offset_y + 14 * cup_scale
    cup_color = hex_to_rgb(CUP_COLOR)
    draw.rectangle([lid_left, lid_top, lid_right, lid_bottom], fill=cup_color + (255,))

    # Cup body
    body_points = [
        (offset_x + 14.1 * cup_scale, offset_y + 16 * cup_scale),
        (offset_x + 33.9 * cup_scale, offset_y + 16 * cup_scale),
        (offset_x + 31.1 * cup_scale, offset_y + 42 * cup_scale),
        (offset_x + 16.9 * cup_scale, offset_y + 42 * cup_scale),
    ]
    draw.polygon(body_points, fill=cup_color + (255,))

    # Inner cutout
    inner_margin = max(1, 2 * scale)
    inner_points = [
        (body_points[0][0] + inner_margin, body_points[0][1] + inner_margin),
        (body_points[1][0] - inner_margin, body_points[1][1] + inner_margin),
        (body_points[2][0] - inner_margin, body_points[2][1] - inner_margin),
        (body_points[3][0] + inner_margin, body_points[3][1] - inner_margin),
    ]
    draw.polygon(inner_points, fill=bg_color + (255,))

    # Lid line
    line_y = offset_y + 11 * cup_scale
    line_height = max(1, 1 * scale)
    draw.rectangle(
        [lid_left + inner_margin, line_y,
         lid_right - inner_margin, line_y + line_height],
        fill=bg_color + (255,)
    )

    # Status indicator
    status_cx = offset_x + 24 * cup_scale
    status_cy = offset_y + 30 * cup_scale
    status_r = max(2, 6 * cup_scale)
    status_color = hex_to_rgb(STATUS_COLORS[status])
    draw.ellipse(
        [status_cx - status_r, status_cy - status_r,
         status_cx + status_r, status_cy + status_r],
        fill=status_color + (255,)
    )

    return img


def create_ico_manually(images: list, ico_path: str) -> None:
    """Create an ICO file manually with multiple sizes."""
    # ICO file structure:
    # Header: 6 bytes
    # Directory entries: 16 bytes each
    # Image data: PNG or BMP format

    num_images = len(images)

    # Prepare PNG data for each image
    png_data_list = []
    for img in images:
        buf = BytesIO()
        img.save(buf, format='PNG')
        png_data_list.append(buf.getvalue())

    # Calculate offsets
    header_size = 6
    dir_entry_size = 16
    data_offset = header_size + (dir_entry_size * num_images)

    with open(ico_path, 'wb') as f:
        # Write ICO header
        f.write(struct.pack('<HHH', 0, 1, num_images))  # Reserved, Type (1=ICO), Count

        # Write directory entries
        current_offset = data_offset
        for i, img in enumerate(images):
            width = img.width if img.width < 256 else 0
            height = img.height if img.height < 256 else 0
            png_size = len(png_data_list[i])

            # Directory entry: width, height, colors, reserved, planes, bpp, size, offset
            f.write(struct.pack('<BBBBHHII',
                width,           # Width (0 means 256)
                height,          # Height (0 means 256)
                0,               # Color count (0 for >256 colors)
                0,               # Reserved
                1,               # Color planes
                32,              # Bits per pixel
                png_size,        # Size of image data
                current_offset   # Offset to image data
            ))
            current_offset += png_size

        # Write image data
        for png_data in png_data_list:
            f.write(png_data)


def create_ico(status: str) -> None:
    """Create an ICO file for the given status."""
    images = []
    for size in SIZES:
        img = draw_icon(size, status)
        images.append(img)

    ico_path = os.path.join(OUTPUT_DIR, f"tray-{status}.ico")
    create_ico_manually(images, ico_path)
    print(f"Created: {ico_path} ({os.path.getsize(ico_path)} bytes)")


def create_app_ico() -> None:
    """Create the main application icon (caffeine.ico)."""
    images = []
    for size in SIZES:
        img = draw_app_icon(size)
        images.append(img)

    ico_path = os.path.join(OUTPUT_DIR, "caffeine.ico")
    create_ico_manually(images, ico_path)
    print(f"Created: {ico_path} ({os.path.getsize(ico_path)} bytes)")


def main():
    # Create main app icon
    print("Creating app icon...")
    create_app_ico()

    # Create tray icons for each status
    print("\nCreating tray icons...")
    for status in STATUS_COLORS.keys():
        create_ico(status)

    print("\nAll ICO files created successfully!")

    # Also create PNG previews for verification
    preview_dir = os.path.join(OUTPUT_DIR, "previews")
    os.makedirs(preview_dir, exist_ok=True)

    # App icon preview
    for size in [48, 256]:
        img = draw_app_icon(size)
        png_path = os.path.join(preview_dir, f"caffeine-{size}.png")
        img.save(png_path)
        print(f"Created preview: {png_path}")

    # Tray icon previews
    for status in STATUS_COLORS.keys():
        for size in [48, 256]:
            img = draw_icon(size, status)
            png_path = os.path.join(preview_dir, f"tray-{status}-{size}.png")
            img.save(png_path)
            print(f"Created preview: {png_path}")


if __name__ == '__main__':
    main()

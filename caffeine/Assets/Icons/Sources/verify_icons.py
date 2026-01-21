"""
Verify ICO files have correct multi-resolution structure.
"""

from PIL import Image
import os


def verify_ico(ico_path):
    """Verify ICO file structure and print details."""
    print(f"\nVerifying: {os.path.basename(ico_path)}")
    print("-" * 50)

    try:
        # Open ICO file
        img = Image.open(ico_path)

        # Get all sizes in the ICO
        print(f"Format: {img.format}")
        print(f"Mode: {img.mode}")
        print(f"Size: {img.size}")

        # Try to get all frames
        sizes = []
        try:
            for i in range(100):  # arbitrary large number
                img.seek(i)
                sizes.append(img.size)
        except EOFError:
            pass

        print(f"Number of sizes: {len(sizes)}")
        print(f"Sizes included: {sizes}")

        # Check file size
        file_size = os.path.getsize(ico_path)
        print(f"File size: {file_size} bytes")

        if len(sizes) >= 4:
            print("Status: [OK] Multi-resolution ICO")
        else:
            print("Status: [WARNING] Expected 4 sizes, found", len(sizes))

        return len(sizes)

    except Exception as e:
        print(f"Error: {e}")
        return 0


def main():
    """Verify all three tray icons."""
    script_dir = os.path.dirname(os.path.abspath(__file__))
    parent_dir = os.path.dirname(script_dir)

    print("=" * 50)
    print("ICO File Verification")
    print("=" * 50)

    icons = [
        'tray-active.ico',
        'tray-inactive.ico',
        'tray-paused.ico'
    ]

    total_sizes = 0
    for icon in icons:
        ico_path = os.path.join(parent_dir, icon)
        if os.path.exists(ico_path):
            total_sizes += verify_ico(ico_path)
        else:
            print(f"\n{icon}: [NOT FOUND]")

    print("\n" + "=" * 50)
    if total_sizes >= 12:  # 3 icons × 4 sizes each
        print("Overall Status: [OK]")
    else:
        print(f"Overall Status: [WARNING] Expected 12 total sizes, found {total_sizes}")
    print("=" * 50)


if __name__ == '__main__':
    main()

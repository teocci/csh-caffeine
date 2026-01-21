"""
ICO File Structure Verifier
Reads ICO file headers directly to verify multi-resolution structure.
"""

import struct
import os


def verify_ico_structure(ico_path):
    """
    Verify ICO file by reading its header structure directly.

    ICO file format:
    - ICONDIR header (6 bytes):
        - Reserved (2 bytes)
        - Type (2 bytes) - 1 for .ICO
        - Count (2 bytes) - number of images
    - ICONDIRENTRY (16 bytes each):
        - Width, Height, Colors, Reserved
        - Planes, BitCount
        - SizeInBytes, FileOffset
    """
    print(f"\nAnalyzing: {os.path.basename(ico_path)}")
    print("-" * 50)

    try:
        with open(ico_path, 'rb') as f:
            # Read ICONDIR header
            reserved, type_field, count = struct.unpack('<HHH', f.read(6))

            print(f"ICO Header:")
            print(f"  Reserved: {reserved} (should be 0)")
            print(f"  Type: {type_field} (1 = ICO, 2 = CUR)")
            print(f"  Image Count: {count}")
            print()

            if type_field != 1:
                print("  [ERROR] Not a valid ICO file (type should be 1)")
                return False

            # Read each ICONDIRENTRY
            print(f"Image Entries:")
            sizes = []
            for i in range(count):
                entry_data = f.read(16)
                if len(entry_data) < 16:
                    print(f"  [ERROR] Incomplete ICONDIRENTRY #{i+1}")
                    break

                width, height, colors, reserved, planes, bit_count, size_bytes, offset = \
                    struct.unpack('<BBBBHHII', entry_data)

                # Width/Height of 0 means 256
                actual_width = width if width != 0 else 256
                actual_height = height if height != 0 else 256

                sizes.append((actual_width, actual_height))

                print(f"  Image #{i+1}:")
                print(f"    Size: {actual_width}x{actual_height}")
                print(f"    Bit Depth: {bit_count} bits/pixel")
                print(f"    Data Size: {size_bytes} bytes")
                print(f"    Offset: {offset}")

            print()
            print(f"Total sizes found: {len(sizes)}")
            print(f"Sizes: {sorted(set(sizes))}")

            # Check file size
            file_size = os.path.getsize(ico_path)
            print(f"File size: {file_size} bytes")
            print()

            # Verify we have the expected sizes
            expected_sizes = [(16, 16), (24, 24), (32, 32), (48, 48)]
            if sorted(sizes) == expected_sizes:
                print("Status: [OK] All expected sizes present")
                return True
            else:
                print(f"Status: [WARNING] Expected {expected_sizes}, got {sorted(sizes)}")
                return len(sizes) >= 2

    except Exception as e:
        print(f"Error reading ICO file: {e}")
        return False


def main():
    """Verify all three tray icons."""
    script_dir = os.path.dirname(os.path.abspath(__file__))
    parent_dir = os.path.dirname(script_dir)

    print("=" * 50)
    print("ICO File Structure Verification")
    print("=" * 50)

    icons = [
        'tray-active.ico',
        'tray-inactive.ico',
        'tray-paused.ico'
    ]

    all_valid = True
    for icon in icons:
        ico_path = os.path.join(parent_dir, icon)
        if os.path.exists(ico_path):
            valid = verify_ico_structure(ico_path)
            all_valid = all_valid and valid
        else:
            print(f"\n{icon}: [NOT FOUND]")
            all_valid = False

    print("\n" + "=" * 50)
    if all_valid:
        print("Overall Status: [OK] All icons valid")
    else:
        print("Overall Status: [FAILED] Some icons have issues")
    print("=" * 50)

    return all_valid


if __name__ == '__main__':
    import sys
    sys.exit(0 if main() else 1)

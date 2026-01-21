using System.Diagnostics;
using System.Runtime.InteropServices;

namespace caffeine.Services;

/// <summary>
/// Service responsible for managing system power state to prevent sleep and display timeout.
/// Uses Windows SetThreadExecutionState API to keep the PC awake.
/// </summary>
public class PowerService
{
    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern uint SetThreadExecutionState(uint esFlags);

    private const uint ES_CONTINUOUS = 0x80000000;
    private const uint ES_SYSTEM_REQUIRED = 0x00000001;
    private const uint ES_DISPLAY_REQUIRED = 0x00000002;

    private bool _isAwake;
    private bool _displayOn;

    /// <summary>
    /// Gets whether the system is currently being kept awake.
    /// </summary>
    public bool IsAwake => _isAwake;

    /// <summary>
    /// Gets whether the display is being kept on.
    /// </summary>
    public bool IsDisplayOn => _displayOn;

    /// <summary>
    /// Sets the system awake state, optionally keeping the display on.
    /// </summary>
    /// <param name="keepAwake">True to prevent sleep, false to allow normal power behavior.</param>
    /// <param name="keepDisplayOn">True to keep display on (Mode 1), false to allow display timeout (Mode 2).</param>
    public void SetAwakeState(bool keepAwake, bool keepDisplayOn)
    {
        _isAwake = keepAwake;
        _displayOn = keepDisplayOn;

        uint flags = ES_CONTINUOUS;

        if (keepAwake)
        {
            flags |= ES_SYSTEM_REQUIRED;

            if (keepDisplayOn)
            {
                flags |= ES_DISPLAY_REQUIRED;
            }
        }

        uint result = SetThreadExecutionState(flags);

        if (result == 0)
        {
            // P/Invoke failed - log but don't throw
            Debug.WriteLine("[PowerService] SetThreadExecutionState failed. System may not stay awake.");
        }
        else
        {
            string mode = keepAwake ? (keepDisplayOn ? "Active (display on)" : "Active (display can sleep)") : "Inactive";
            Debug.WriteLine($"[PowerService] Power state set to: {mode}");
        }
    }

    /// <summary>
    /// Resets the power state to allow normal sleep behavior.
    /// Call this on application exit to ensure the system can sleep normally.
    /// </summary>
    public void Reset()
    {
        SetAwakeState(false, false);
    }
}

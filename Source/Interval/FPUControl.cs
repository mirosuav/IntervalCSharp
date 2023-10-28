using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace Interval;

/// <summary>
/// Uses '_controlfp_s' function in C++ lib <see href="https://learn.microsoft.com/en-us/cpp/c-runtime-library/reference/controlfp-s?view=msvc-170"/>
/// </summary>
/// 
public static class FPUControl
{
    const uint RoundingMask = 0x00000300; //_MCW_RC 
    public enum RoundingMode : uint
    {
        Nearest = 0x00000000, //_RC_NEAR 
        Down = 0x00000100, //_RC_DOWN
        Up = 0x00000200, //_RC_UP
        Truncate = 0x00000300 //_RC_CHOP
    }

    public static readonly RoundingMode InitialRoundingMode;
    static FPUControl()
    {
        InitialRoundingMode = GetRoundingMode();
    }

    public static void SetRoundingNEAR() => SetRoundingMode(RoundingMode.Nearest);
    public static void SetRoundingUP() => SetRoundingMode(RoundingMode.Up);
    public static void SetRoundingDOWN() => SetRoundingMode(RoundingMode.Down);
    public static void SetRoundingTRUNC() => SetRoundingMode(RoundingMode.Truncate);
    public static void RevertRoundingMode() => SetRoundingMode(InitialRoundingMode);

    public static RoundingMode GetRoundingMode()
    {
        uint _currentControl = 0;
        uint err = _controlfp_s(ref _currentControl, 0, 0);
        if (err != 0)
            throw new ApplicationException($"Error while getting FPU rounding mode, [code:{err}].");

        return (RoundingMode)(_currentControl & RoundingMask);
    }

    public static void SetRoundingMode(RoundingMode mode)
    {
        uint _currentControl = 0;
        uint err = _controlfp_s(ref _currentControl, (uint)mode, RoundingMask);
        if (err != 0)
            throw new ApplicationException($"Error while setting FPU rounding mode {mode}, [code:{err}].");
    }

    [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    static extern uint _controlfp_s(ref uint currentControl, uint newControl, uint mask);
}

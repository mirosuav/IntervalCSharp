using System.Runtime.InteropServices;
using System.Security;

namespace IntervalCSharp.Helpers;

/// <summary>
/// Uses '_controlfp_s' function in C++ lib <see href="https://learn.microsoft.com/en-us/cpp/c-runtime-library/reference/controlfp-s?view=msvc-170"/>
/// </summary>
public static class FPURounding
{
    /// <summary>
    /// Rounding mask to the FPU control word
    /// </summary>
    const uint RoundingMask = 0x00000300; //_MCW_RC 

    /// <summary>
    /// Rounding modes control for _controlfp_s function
    /// </summary>
    public enum RoundingMode : uint
    {
        Nearest = 0x00000000, //_RC_NEAR 
        Down = 0x00000100, //_RC_DOWN
        Up = 0x00000200, //_RC_UP
        Truncate = 0x00000300 //_RC_CHOP
    }

    public static readonly RoundingMode InitialRoundingMode;
    static FPURounding()
    {
        //Remember initial rounding mode at the start of program
        InitialRoundingMode = Get();
    }

    public static void Near() => Set(RoundingMode.Nearest);
    public static void Up() => Set(RoundingMode.Up);
    public static void Down() => Set(RoundingMode.Down);
    public static void Trunc() => Set(RoundingMode.Truncate);
    public static void Reset() => Set(InitialRoundingMode);

    public static RoundingMode Get()
    {
        uint _currentControl = 0;
        uint err = _controlfp_s(ref _currentControl, 0, 0);
        if (err != 0)
            throw new ApplicationException($"Error while getting FPU rounding mode, [code:{err}].");

        return (RoundingMode)(_currentControl & RoundingMask);
    }

    public static void Set(RoundingMode mode)
    {
        uint _currentControl = 0;
        uint err = _controlfp_s(ref _currentControl, (uint)mode, RoundingMask);
        if (err != 0)
            throw new ApplicationException($"Error while setting FPU rounding mode {mode}, [code:{err}].");
    }

    [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
    static extern uint _controlfp_s(ref uint currentControl, uint newControl, uint mask);
}

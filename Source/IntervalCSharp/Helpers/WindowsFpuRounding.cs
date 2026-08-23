using System.Runtime.InteropServices;

namespace IntervalCSharp.Helpers;

internal sealed class WindowsFpuRounding(int[] nativeModes)
    : PlatformFpuRounding(nativeModes)
{
    /// <summary>_MCW_RC — the rounding-control field of the Windows FPU control word.</summary>
    private const uint _windowsRoundingMask = 0x00000300;

    public override RoundingMode Get()
    {
        uint control = 0;
        ThrowOnError(Windows._controlfp_s(ref control, 0, 0), "reading");
        var native = (int)(control & _windowsRoundingMask);

        var mode = Array.IndexOf(NativeModes, native);
        if (mode < 0)
            throw new InvalidOperationException(
                $"Unrecognized FPU rounding mode 0x{native:X} reported by the platform.");

        return (RoundingMode)mode;
    }

    public override void Set(RoundingMode mode)
    {
        var native = NativeModes[(int)mode];
        uint control = 0;
        ThrowOnError(Windows._controlfp_s(ref control, (uint)native, _windowsRoundingMask),
            $"setting {mode} on");
    }

    // The P/Invokes are nested so that a target is only ever resolved on the platform that uses it.
    // SuppressGCTransition is safe here: each call is a handful of instructions, never blocks and
    // never calls back into managed code.

    private static class Windows
    {
        [DllImport("ucrtbase.dll", CallingConvention = CallingConvention.Cdecl), SuppressGCTransition]
        internal static extern uint _controlfp_s(ref uint currentControl, uint newControl, uint mask);
    }
}
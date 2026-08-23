using System.Runtime.InteropServices;

namespace IntervalCSharp.Helpers;

internal sealed class LinuxFpuRounding(int[] nativeModes)
    : PlatformFpuRounding(nativeModes)
{
    public override RoundingMode Get()
    {
        var native = Linux.fegetround();
        var mode = Array.IndexOf(NativeModes, native);
        if (mode < 0)
            throw new InvalidOperationException(
                $"Unrecognized FPU rounding mode 0x{native:X} reported by the platform.");

        return (RoundingMode)mode;
    }

    public override void Set(RoundingMode mode)
    {
        var native = NativeModes[(int)mode];
        ThrowOnError(Linux.fesetround(native), $"setting {mode} on");
    }

    private static class Linux
    {
        [DllImport("libc.so.6"), SuppressGCTransition]
        internal static extern int fegetround();

        [DllImport("libc.so.6"), SuppressGCTransition]
        internal static extern int fesetround(int mode);
    }
}
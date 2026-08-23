using System.Runtime.InteropServices;

namespace IntervalCSharp.Helpers;

internal sealed class MacOsFpuRounding(int[] nativeModes)
    : PlatformFpuRounding(nativeModes)
{
    public override RoundingMode Get()
    {
        var native = MacOS.fegetround();

        int mode = Array.IndexOf(NativeModes, native);
        if (mode < 0)
            throw new InvalidOperationException(
                $"Unrecognized FPU rounding mode 0x{native:X} reported by the platform.");

        return (RoundingMode)mode;
    }

    public override void Set(RoundingMode mode)
    {
        int native = NativeModes[(int)mode];
        ThrowOnError(MacOS.fesetround(native), $"setting {mode} on");
    }

    private static class MacOS
    {
        [DllImport("libSystem.dylib"), SuppressGCTransition]
        internal static extern int fegetround();

        [DllImport("libSystem.dylib"), SuppressGCTransition]
        internal static extern int fesetround(int mode);
    }
}
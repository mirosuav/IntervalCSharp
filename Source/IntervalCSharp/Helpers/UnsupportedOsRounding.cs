using System.Runtime.InteropServices;

namespace IntervalCSharp.Helpers;

internal sealed class UnsupportedOsRounding() : PlatformFpuRounding([])
{
    public override void InitializeRounding()
    {
        InitialRoundingMode = RoundingMode.Nearest;
    }

    public override RoundingMode Get() => throw Unsupported();

    public override void Set(RoundingMode mode) => throw Unsupported();

    private static PlatformNotSupportedException Unsupported()
        => new($"Directed FPU rounding is not available on {RuntimeInformation.OSDescription} " +
               $"/ {RuntimeInformation.ProcessArchitecture}. Windows, Linux and macOS on x86/x64/ARM are supported; " +
               "WebAssembly exposes no rounding-mode control at all.");
}
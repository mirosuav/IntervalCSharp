namespace IntervalCSharp.Helpers;

/// <summary>
/// Controls the hardware floating-point rounding mode (MXCSR on x86/x64, FPCR on ARM) so that
/// interval bounds can be computed with true directed rounding.
/// </summary>
/// <remarks>
/// <para>
/// The rounding mode is part of every <b>OS thread's</b> saved CPU context, so concurrent threads
/// never stomp on each other's mode and the <c>try/finally</c> reset pattern used by the operators
/// is sufficient. The one hazard: a rounding-mode scope must <b>never contain an <c>await</c></b> —
/// a continuation may resume on a different thread pool thread where the mode does not apply.
/// </para>
/// <para>
/// Supported on Windows (<c>ucrtbase.dll</c>, <c>_controlfp_s</c>), Linux (<c>libc.so.6</c>) and
/// macOS (<c>libSystem.dylib</c>, both via <c>fenv.h</c>), on x86/x64 and ARM/ARM64. WebAssembly
/// exposes no rounding-mode control and is explicitly unsupported.
/// </para>
/// </remarks>
public static class FpuRounding
{
    private static readonly PlatformFpuRounding _rounding;
    static FpuRounding()
    {
        _rounding = PlatformFpuRounding.Detect();
        _rounding.InitializeRounding();
    }
    
    public static RoundingMode InitialRoundingMode => _rounding.InitialRoundingMode;
    public static void Near() => _rounding.Set(RoundingMode.Nearest);
    public static void Up() => _rounding.Set(RoundingMode.Up);
    public static void Down() => _rounding.Set(RoundingMode.Down);
    public static void Trunc() => _rounding.Set(RoundingMode.Truncate);
    public static void Reset() => _rounding.Set(_rounding.InitialRoundingMode);
    public static RoundingMode Get() => _rounding.Get();
    public static void Set(RoundingMode mode) => _rounding.Set(mode);
}
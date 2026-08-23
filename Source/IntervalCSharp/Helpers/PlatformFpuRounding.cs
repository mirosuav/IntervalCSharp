using System.Runtime.InteropServices;

namespace IntervalCSharp.Helpers;

public abstract class PlatformFpuRounding(int[] nativeModes)
{
    /// <summary>Native mode codes for the running platform, indexed by <see cref="RoundingMode"/>.</summary>
    protected readonly int[] NativeModes = nativeModes;

    /// <summary>Rounding mode observed when this type was first used; <see cref="Reset"/> restores it.</summary>
    public RoundingMode InitialRoundingMode { get; protected set; }

    public virtual void InitializeRounding()
    {
        InitialRoundingMode = Get();
    }


    public abstract RoundingMode Get();

    public abstract void Set(RoundingMode mode);

    protected static void ThrowOnError(uint error, string action)
    {
        if (error != 0)
            throw new InvalidOperationException($"Error while {action} the FPU rounding mode, [code:{error}].");
    }

    protected static void ThrowOnError(int error, string action) => ThrowOnError((uint)error, action);


    /// <summary>
    /// Picks the native API and the native mode codes for the running OS and architecture.
    /// The <c>fenv.h</c> codes are architecture-specific — note that Up and Down swap places
    /// between the x86 control-word layout and the ARM FPCR RMode field.
    /// </summary>
    public static PlatformFpuRounding Detect()
    {
        // _RC_NEAR / _RC_DOWN / _RC_UP / _RC_CHOP - the same on every Windows architecture.
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return new WindowsFpuRounding([0x000, 0x100, 0x200, 0x300]);

        Platform platform =
            RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? Platform.Linux :
            RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? Platform.MacOS :
            Platform.Unsupported;

        return RuntimeInformation.ProcessArchitecture switch
        {
            // FE_TONEAREST / FE_DOWNWARD / FE_UPWARD / FE_TOWARDZERO, x87 control-word layout.
            Architecture.X86 or Architecture.X64 => platform switch
            {
                Platform.Linux => new LinuxFpuRounding([0x0, 0x400, 0x800, 0xC00]),
                Platform.MacOS => new MacOsFpuRounding([0x0, 0x400, 0x800, 0xC00]),
                _ => new UnsupportedOsRounding()
            },
            // Same names, FPCR RMode field (bits 22-23) - Up and Down are the other way round.
            Architecture.Arm or Architecture.Arm64 => platform switch
            {
                Platform.Linux => new LinuxFpuRounding([0x0, 0x800000, 0x400000, 0xC00000]),
                Platform.MacOS => new MacOsFpuRounding([0x0, 0x800000, 0x400000, 0xC00000]),
                _ => new UnsupportedOsRounding()
            },
            _ => new UnsupportedOsRounding()
        };
    }
}
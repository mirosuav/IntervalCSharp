namespace IntervalCSharp.Helpers;

/// <summary>
/// Platform-neutral rounding modes. The values are ordinals — the native codes live in
/// <see cref="_nativeModes"/>, which is indexed by them.
/// </summary>
public enum RoundingMode
{
    Nearest = 0,
    Down = 1,
    Up = 2,
    Truncate = 3
}
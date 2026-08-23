using System.Numerics;

namespace IntervalCSharp.Helpers;

/// <summary>
/// Fixed-arity min/max over the four candidate products (or quotients) of an interval
/// multiplication or division. Fixed arity - not <c>params T[]</c> - because these sit on the
/// hot path of every such operator and must not allocate.
/// </summary>
public static class MathHelper
{
    public static T Min<T>(T a, T b, T c, T d) where T : INumber<T>
        => T.Min(T.Min(a, b), T.Min(c, d));

    public static T Max<T>(T a, T b, T c, T d) where T : INumber<T>
        => T.Max(T.Max(a, b), T.Max(c, d));
}

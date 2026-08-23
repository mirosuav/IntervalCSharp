namespace IntervalCSharp;

internal static class IntervalExceptions
{
    internal static readonly IntervalFormatExceptions IntervalStringParsingError
        = new IntervalFormatExceptions("Could not parse string as Interval.");

    internal static readonly DivideByZeroException DividingByIntervalContainingZero
        = new DivideByZeroException("Division by interval containing Zero.");

    internal static readonly ArgumentOutOfRangeException SquareRootOfNegativeInterval
        = new ArgumentOutOfRangeException("value", "Square root of an interval reaching below Zero.");

    internal static readonly ArgumentOutOfRangeException LogarithmOfNegativeInterval
        = new ArgumentOutOfRangeException("value", "Logarithm of an interval reaching below Zero.");

    internal static readonly ArgumentOutOfRangeException TangentOfIntervalContainingPole
        = new ArgumentOutOfRangeException("value", "Tangent of an interval containing a pole at (k + 1/2)*PI.");
}

public class IntervalFormatExceptions : FormatException
{
    public IntervalFormatExceptions(string? message) : base(message)
    {
    }
    public IntervalFormatExceptions(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}

namespace IntervalCSharp;

internal static class IntervalExceptions
{
    internal static readonly IntervalFormatExceptions IntervalStringParsingError
        = new IntervalFormatExceptions("Could not parse string as Interval.");

    internal static readonly DivideByZeroException DividingByIntervalContainingZero
        = new DivideByZeroException("Division by interval containing Zero.");
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

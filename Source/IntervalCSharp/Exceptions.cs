using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace IntervalCSharp;

public class IntervalFormatExceptions : FormatException
{
    public static readonly IntervalFormatExceptions CannotParseAsInterval 
        = new IntervalFormatExceptions("Couldn't parse string as Interval.");

    public IntervalFormatExceptions(string? message) : base(message)
    {
    }
    public IntervalFormatExceptions(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}

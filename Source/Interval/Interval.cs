using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;
using System.Runtime.Serialization;

namespace Interval;

public readonly record struct Interval(decimal Min, decimal Max) 
{

}

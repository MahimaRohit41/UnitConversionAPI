namespace UnitConversionApi.Exceptions;

/// <summary>
/// Thrown when a conversion cannot be performed due to an unknown unit or
/// an attempt to convert across incompatible categories.
/// </summary>
public sealed class ConversionException : Exception
{
    public ConversionException(string message) : base(message) { }
}

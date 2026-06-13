namespace UnitConversionApi.Models;

/// <summary>Response payload returned by the convert endpoint.</summary>
public sealed class ConversionResponse
{
    public double InputValue { get; init; }
    public required string InputUnit { get; init; }
    public required string InputUnitSymbol { get; init; }

    public double OutputValue { get; init; }
    public required string OutputUnit { get; init; }
    public required string OutputUnitSymbol { get; init; }

    public required string Category { get; init; }
}

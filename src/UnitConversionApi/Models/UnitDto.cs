namespace UnitConversionApi.Models;

/// <summary>Represents a single supported unit of measurement.</summary>
public sealed class UnitDto
{
    public required string Key { get; init; }
    public required string DisplayName { get; init; }
    public required string Symbol { get; init; }
    public required string Category { get; init; }
}

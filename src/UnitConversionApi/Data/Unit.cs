namespace UnitConversionApi.Data;

/// <summary>
/// Represents a unit of measurement with its conversion logic relative to a base unit.
/// </summary>
public sealed class Unit
{
    /// <summary>Unique identifier used in API requests (e.g., "meters", "celsius").</summary>
    public required string Key { get; init; }

    /// <summary>Human-readable name (e.g., "Meters", "Celsius").</summary>
    public required string DisplayName { get; init; }

    /// <summary>Abbreviated symbol (e.g., "m", "°C").</summary>
    public required string Symbol { get; init; }

    /// <summary>Category this unit belongs to (e.g., "length", "temperature").</summary>
    public required string Category { get; init; }

    /// <summary>Converts a value in this unit to the category's base unit.</summary>
    public required Func<double, double> ToBase { get; init; }

    /// <summary>Converts a value from the category's base unit to this unit.</summary>
    public required Func<double, double> FromBase { get; init; }
}

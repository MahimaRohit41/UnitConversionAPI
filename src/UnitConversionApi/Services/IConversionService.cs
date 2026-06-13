using UnitConversionApi.Models;

namespace UnitConversionApi.Services;

public interface IConversionService
{
    /// <summary>
    /// Converts <paramref name="value"/> from <paramref name="fromUnit"/> to
    /// <paramref name="toUnit"/>.
    /// </summary>
    /// <exception cref="Exceptions.ConversionException">
    /// Thrown when either unit key is unknown, or the units belong to
    /// different categories.
    /// </exception>
    ConversionResponse Convert(double value, string fromUnit, string toUnit);

    /// <summary>Returns all supported units, optionally filtered by category.</summary>
    IReadOnlyCollection<UnitDto> GetUnits(string? category = null);

    /// <summary>Returns the distinct set of supported category names.</summary>
    IReadOnlyCollection<string> GetCategories();
}

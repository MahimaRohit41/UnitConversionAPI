using UnitConversionApi.Data;
using UnitConversionApi.Exceptions;
using UnitConversionApi.Models;

namespace UnitConversionApi.Services;

/// <inheritdoc />
public sealed class ConversionService : IConversionService
{
    private readonly UnitRegistry _registry;

    public ConversionService(UnitRegistry registry) => _registry = registry;

    /// <inheritdoc />
    public ConversionResponse Convert(double value, string fromUnit, string toUnit)
    {
        if (!_registry.TryGetUnit(fromUnit, out var from) || from is null)
            throw new ConversionException($"Unknown unit: '{fromUnit}'.");

        if (!_registry.TryGetUnit(toUnit, out var to) || to is null)
            throw new ConversionException($"Unknown unit: '{toUnit}'.");

        if (!from.Category.Equals(to.Category, StringComparison.OrdinalIgnoreCase))
            throw new ConversionException(
                $"Cannot convert between '{from.Category}' and '{to.Category}'. " +
                "Both units must belong to the same category.");

        var baseValue   = from.ToBase(value);
        var outputValue = to.FromBase(baseValue);

        return new ConversionResponse
        {
            InputValue       = value,
            InputUnit        = from.DisplayName,
            InputUnitSymbol  = from.Symbol,
            OutputValue      = outputValue,
            OutputUnit       = to.DisplayName,
            OutputUnitSymbol = to.Symbol,
            Category         = from.Category
        };
    }

    /// <inheritdoc />
    public IReadOnlyCollection<UnitDto> GetUnits(string? category = null)
    {
        var source = string.IsNullOrWhiteSpace(category)
            ? _registry.GetAll()
            : _registry.GetByCategory(category);

        return source
            .OrderBy(u => u.Category)
            .ThenBy(u => u.DisplayName)
            .Select(u => new UnitDto
            {
                Key         = u.Key,
                DisplayName = u.DisplayName,
                Symbol      = u.Symbol,
                Category    = u.Category
            })
            .ToList();
    }

    /// <inheritdoc />
    public IReadOnlyCollection<string> GetCategories() =>
        _registry.GetCategories();
}

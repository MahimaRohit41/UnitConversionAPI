using FluentAssertions;
using UnitConversionApi.Data;
using UnitConversionApi.Exceptions;
using UnitConversionApi.Services;

namespace UnitConversionApi.Tests.Services;

public sealed class ConversionServiceTests
{
    private readonly ConversionService _sut = new(new UnitRegistry());

    // -------------------------------------------------------------------------
    // Length
    // -------------------------------------------------------------------------
    [Theory]
    [InlineData(1.0,     "meters",      "feet",         3.28084)]
    [InlineData(1.0,     "kilometers",  "miles",        0.621371)]
    [InlineData(1.0,     "inches",      "centimeters",  2.54)]
    [InlineData(1.0,     "yards",       "meters",       0.9144)]
    [InlineData(1_609.344, "meters",    "miles",        1.0)]
    public void Convert_Length_ReturnsCorrectValue(
        double value, string from, string to, double expected)
    {
        var result = _sut.Convert(value, from, to);
        result.OutputValue.Should().BeApproximately(expected, precision: 0.001);
    }

    // -------------------------------------------------------------------------
    // Temperature
    // -------------------------------------------------------------------------
    [Theory]
    [InlineData(0.0,   "celsius",    "fahrenheit", 32.0)]
    [InlineData(100.0, "celsius",    "fahrenheit", 212.0)]
    [InlineData(0.0,   "celsius",    "kelvin",     273.15)]
    [InlineData(32.0,  "fahrenheit", "celsius",    0.0)]
    [InlineData(212.0, "fahrenheit", "celsius",    100.0)]
    [InlineData(273.15,"kelvin",     "celsius",    0.0)]
    public void Convert_Temperature_ReturnsCorrectValue(
        double value, string from, string to, double expected)
    {
        var result = _sut.Convert(value, from, to);
        result.OutputValue.Should().BeApproximately(expected, precision: 0.01);
    }

    // -------------------------------------------------------------------------
    // Weight
    // -------------------------------------------------------------------------
    [Theory]
    [InlineData(1.0,    "kilograms",  "pounds",     2.20462)]
    [InlineData(1.0,    "pounds",     "kilograms",  0.453592)]
    [InlineData(1000.0, "grams",      "kilograms",  1.0)]
    [InlineData(1.0,    "metric-tons","kilograms",  1000.0)]
    public void Convert_Weight_ReturnsCorrectValue(
        double value, string from, string to, double expected)
    {
        var result = _sut.Convert(value, from, to);
        result.OutputValue.Should().BeApproximately(expected, precision: 0.001);
    }

    // -------------------------------------------------------------------------
    // Edge cases
    // -------------------------------------------------------------------------
    [Fact]
    public void Convert_SameUnit_ReturnsSameValue()
    {
        var result = _sut.Convert(42.5, "meters", "meters");
        result.OutputValue.Should().Be(42.5);
    }

    [Fact]
    public void Convert_ZeroValue_ReturnsZeroForLinearUnits()
    {
        var result = _sut.Convert(0.0, "meters", "feet");
        result.OutputValue.Should().Be(0.0);
    }

    [Fact]
    public void Convert_NegativeValue_IsHandledCorrectly()
    {
        // -40 °C == -40 °F
        var result = _sut.Convert(-40.0, "celsius", "fahrenheit");
        result.OutputValue.Should().BeApproximately(-40.0, precision: 0.001);
    }

    [Fact]
    public void Convert_UnknownFromUnit_ThrowsConversionException()
    {
        var act = () => _sut.Convert(1.0, "parsecs", "meters");
        act.Should().Throw<ConversionException>().WithMessage("*parsecs*");
    }

    [Fact]
    public void Convert_UnknownToUnit_ThrowsConversionException()
    {
        var act = () => _sut.Convert(1.0, "meters", "parsecs");
        act.Should().Throw<ConversionException>().WithMessage("*parsecs*");
    }

    [Fact]
    public void Convert_CrossCategoryConversion_ThrowsConversionException()
    {
        var act = () => _sut.Convert(1.0, "meters", "kilograms");
        act.Should().Throw<ConversionException>()
           .WithMessage("*length*weight*");
    }

    [Fact]
    public void Convert_UnitKeysAreCaseInsensitive()
    {
        var result = _sut.Convert(100.0, "Celsius", "FAHRENHEIT");
        result.OutputValue.Should().BeApproximately(212.0, precision: 0.01);
    }

    // -------------------------------------------------------------------------
    // Registry / metadata
    // -------------------------------------------------------------------------
    [Fact]
    public void GetCategories_ContainsAllRequiredCategories()
    {
        var categories = _sut.GetCategories();
        categories.Should().Contain(new[]
        {
            "length", "temperature", "weight",
            "volume", "digital-storage"
        });
    }

    [Fact]
    public void GetUnits_WithCategory_ReturnsOnlyThatCategory()
    {
        var units = _sut.GetUnits("length");
        units.Should().NotBeEmpty();
        units.Should().AllSatisfy(u => u.Category.Should().Be("length"));
    }

    [Fact]
    public void GetUnits_WithoutCategory_ReturnsAllUnits()
    {
        var all    = _sut.GetUnits();
        var length = _sut.GetUnits("length");
        all.Count.Should().BeGreaterThan(length.Count);
    }
}

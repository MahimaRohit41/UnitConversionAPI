namespace UnitConversionApi.Data;

/// <summary>
/// In-memory registry of all supported units, keyed by their identifier.
/// The registry is designed to be registered as a singleton; all lookups are O(1).
///
/// Conversion strategy:
///   Linear units  → base = value × factor  (e.g., length base = metres)
///   Temperature   → custom affine functions (non-linear offset + scale)
///
/// To add a new unit: yield a new entry inside BuildUnits().
/// To add a new category: just start yielding units with a new category string.
/// </summary>
public sealed class UnitRegistry
{
    private readonly IReadOnlyDictionary<string, Unit> _units;

    public UnitRegistry()
    {
        _units = BuildUnits()
            .ToDictionary(u => u.Key, StringComparer.OrdinalIgnoreCase);
    }

    public bool TryGetUnit(string key, out Unit? unit) =>
        _units.TryGetValue(key, out unit);

    public IReadOnlyCollection<Unit> GetAll() =>
        (IReadOnlyCollection<Unit>)_units.Values;

    public IReadOnlyCollection<Unit> GetByCategory(string category) =>
        _units.Values
              .Where(u => u.Category.Equals(category, StringComparison.OrdinalIgnoreCase))
              .ToList();

    public IReadOnlyCollection<string> GetCategories() =>
        _units.Values
              .Select(u => u.Category)
              .Distinct(StringComparer.OrdinalIgnoreCase)
              .OrderBy(c => c)
              .ToList();

    // -------------------------------------------------------------------------
    // Unit definitions
    // -------------------------------------------------------------------------
    private static IEnumerable<Unit> BuildUnits()
    {
        // --- Length (base: metres) ---
        yield return Linear("meters",        "Meters",         "m",     "length", 1.0);
        yield return Linear("kilometers",    "Kilometers",     "km",    "length", 1_000.0);
        yield return Linear("centimeters",   "Centimeters",    "cm",    "length", 0.01);
        yield return Linear("millimeters",   "Millimeters",    "mm",    "length", 0.001);
        yield return Linear("feet",          "Feet",           "ft",    "length", 0.3048);
        yield return Linear("inches",        "Inches",         "in",    "length", 0.0254);
        yield return Linear("yards",         "Yards",          "yd",    "length", 0.9144);
        yield return Linear("miles",         "Miles",          "mi",    "length", 1_609.344);
        yield return Linear("nautical-miles","Nautical Miles",  "nmi",  "length", 1_852.0);

        // --- Weight / Mass (base: kilograms) ---
        yield return Linear("kilograms",     "Kilograms",      "kg",    "weight", 1.0);
        yield return Linear("grams",         "Grams",          "g",     "weight", 0.001);
        yield return Linear("milligrams",    "Milligrams",     "mg",    "weight", 0.000_001);
        yield return Linear("metric-tons",   "Metric Tons",    "t",     "weight", 1_000.0);
        yield return Linear("pounds",        "Pounds",         "lb",    "weight", 0.453_592_37);
        yield return Linear("ounces",        "Ounces",         "oz",    "weight", 0.028_349_523_125);
        yield return Linear("stones",        "Stones",         "st",    "weight", 6.350_293_18);
        yield return Linear("us-tons",       "US Short Tons",  "tn",    "weight", 907.184_74);

        // --- Temperature (base: Celsius) — affine conversions ---
        yield return new Unit
        {
            Key = "celsius",      DisplayName = "Celsius",     Symbol = "°C",
            Category = "temperature",
            ToBase   = v => v,
            FromBase = v => v
        };
        yield return new Unit
        {
            Key = "fahrenheit",   DisplayName = "Fahrenheit",  Symbol = "°F",
            Category = "temperature",
            ToBase   = v => (v - 32.0) * 5.0 / 9.0,
            FromBase = v => v * 9.0 / 5.0 + 32.0
        };
        yield return new Unit
        {
            Key = "kelvin",       DisplayName = "Kelvin",      Symbol = "K",
            Category = "temperature",
            ToBase   = v => v - 273.15,
            FromBase = v => v + 273.15
        };
        yield return new Unit
        {
            Key = "rankine",      DisplayName = "Rankine",     Symbol = "°R",
            Category = "temperature",
            ToBase   = v => (v - 491.67) * 5.0 / 9.0,
            FromBase = v => (v + 273.15) * 9.0 / 5.0
        };

        // --- Volume (base: litres) ---
        yield return Linear("liters",           "Liters",             "L",    "volume", 1.0);
        yield return Linear("milliliters",      "Milliliters",        "mL",   "volume", 0.001);
        yield return Linear("cubic-meters",     "Cubic Meters",       "m³",   "volume", 1_000.0);
        yield return Linear("cubic-centimeters","Cubic Centimeters",  "cm³",  "volume", 0.001);
        yield return Linear("gallons-us",       "Gallons (US)",       "gal",  "volume", 3.785_411_784);
        yield return Linear("quarts-us",        "Quarts (US)",        "qt",   "volume", 0.946_352_946);
        yield return Linear("pints-us",         "Pints (US)",         "pt",   "volume", 0.473_176_473);
        yield return Linear("fluid-ounces-us",  "Fluid Ounces (US)",  "fl oz","volume", 0.029_573_529_562_5);
        yield return Linear("cups-us",          "Cups (US)",          "cup",  "volume", 0.236_588_236_5);

        // --- Digital Storage (base: bytes) ---
        yield return Linear("bytes",     "Bytes",     "B",  "digital-storage", 1.0);
        yield return Linear("kilobytes", "Kilobytes", "KB", "digital-storage", 1_024.0);
        yield return Linear("megabytes", "Megabytes", "MB", "digital-storage", 1_048_576.0);
        yield return Linear("gigabytes", "Gigabytes", "GB", "digital-storage", 1_073_741_824.0);
        yield return Linear("terabytes", "Terabytes", "TB", "digital-storage", 1_099_511_627_776.0);
        yield return Linear("bits",      "Bits",      "b",  "digital-storage", 0.125);
    }

    private static Unit Linear(
        string key, string displayName, string symbol, string category, double factor) =>
        new()
        {
            Key         = key,
            DisplayName = displayName,
            Symbol      = symbol,
            Category    = category,
            ToBase      = v => v * factor,
            FromBase    = v => v / factor
        };
}

# Unit Conversion API

A RESTful ASP.NET Core Web API that converts numerical values between different units of measurement.

## Supported Categories

| Category | Units |
|---|---|
| Length | meters, kilometers, centimeters, millimeters, feet, inches, yards, miles, nautical miles |
| Temperature | Celsius, Fahrenheit, Kelvin, Rankine |
| Weight / Mass | kilograms, grams, milligrams, metric tons, pounds, ounces, stones, US short tons |
| Volume | liters, milliliters, cubic meters, cubic centimeters, gallons (US), quarts (US), pints (US), fluid ounces (US), cups (US) |
| Digital Storage | bytes, KB, MB, GB, TB, bits |

---

## Prerequisites

| Tool | Minimum version |
|---|---|
| [.NET SDK](https://dotnet.microsoft.com/download) | 10.0 |

Verify your installation:

```bash
dotnet --version
```

---

## Running Locally

```bash
# 1. Clone the repository
git clone <repository-url>
cd unit-conversion-api

# 2. Run the API
dotnet run --project src/UnitConversionApi

# The API starts on http://localhost:5250.
# Interactive API documentation (Scalar UI) is available at:
#   http://localhost:5250/scalar/v1
```

> The launch URL and port can be customised in `src/UnitConversionApi/Properties/launchSettings.json`.

---

## Running Tests

```bash
dotnet test
```

---

## API Reference

All endpoints are under `/api/conversions`.

### `GET /api/conversions/convert`

Converts a value from one unit to another.

| Query parameter | Required | Description |
|---|---|---|
| `value` | Yes | Numerical value to convert |
| `from` | Yes | Source unit key (see `/api/conversions/units`) |
| `to` | Yes | Target unit key |

**Example request**

```
GET /api/conversions/convert?value=100&from=celsius&to=fahrenheit
```

**Example response `200 OK`**

```json
{
  "inputValue": 100,
  "inputUnit": "Celsius",
  "inputUnitSymbol": "°C",
  "outputValue": 212,
  "outputUnit": "Fahrenheit",
  "outputUnitSymbol": "°F",
  "category": "temperature"
}
```

---

### `GET /api/conversions/units`

Returns all supported units, optionally filtered by category.

| Query parameter | Required | Description |
|---|---|---|
| `category` | No | Filter by category (e.g. `length`) |

---

### `GET /api/conversions/categories`

Returns all supported category names.

---

## Project Structure

```
.
├── docs/
│   └── architecture.html       # Architecture overview, request flow, design decisions
├── src/
│   └── UnitConversionApi/
│       ├── Controllers/        # HTTP layer — thin, delegates to services
│       ├── Data/               # UnitRegistry + Unit model
│       ├── Exceptions/         # Domain exceptions
│       ├── Models/             # DTOs (response shapes)
│       ├── Services/           # IConversionService + implementation
│       └── Program.cs          # Composition root
├── tests/
│   └── UnitConversionApi.Tests/
│       ├── Controllers/        # Integration tests (WebApplicationFactory)
│       └── Services/           # Unit tests for ConversionService
└── UnitConversion.slnx
```

---

## Architecture Documentation

A detailed architecture document is available at [`docs/architecture.html`](docs/architecture.html) — open it directly in any browser (no server required).

It covers:

- Layered architecture overview (HTTP → Service → Data)
- Full request flow with a step-by-step walkthrough
- Why the base-unit conversion strategy was chosen (with naive vs base-unit comparison)
- Dependency injection wiring and lifetime choices
- Error handling strategy
- Testing approach

---

## Design Decisions

### Base-unit conversion strategy
Each unit stores two functions — `ToBase` and `FromBase` — that convert to/from a canonical base unit for its category (e.g. metres for length, kilograms for weight, Celsius for temperature).

A conversion from unit A to unit B is always:

```
result = B.FromBase( A.ToBase(value) )
```

This requires only **N** conversion functions for N units (rather than N²) and makes adding a new unit a one-liner in `UnitRegistry.BuildUnits()`.

### Temperature handling
Temperature uses affine (non-linear) conversions, so it cannot use the same `factor` shorthand as linear units. Each temperature unit defines its own `ToBase` / `FromBase` lambdas explicitly, keeping the same interface.

### Scalability
`UnitRegistry` is registered as a **singleton** and backed by a plain dictionary built once at startup. Extending the system to load units from a database or configuration file in the future only requires changing `UnitRegistry`, with no other code changes.

### No authentication / rate limiting
This version is intentionally minimal. A production deployment would add API-key authentication and rate limiting via ASP.NET Core middleware.

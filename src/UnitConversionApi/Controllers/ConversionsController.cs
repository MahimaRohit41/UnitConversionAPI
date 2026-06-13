using Microsoft.AspNetCore.Mvc;
using UnitConversionApi.Exceptions;
using UnitConversionApi.Models;
using UnitConversionApi.Services;

namespace UnitConversionApi.Controllers;

/// <summary>
/// Endpoints for converting values between units and discovering supported units.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public sealed class ConversionsController : ControllerBase
{
    private readonly IConversionService _conversionService;

    public ConversionsController(IConversionService conversionService) =>
        _conversionService = conversionService;

    /// <summary>
    /// Converts a numerical value from one unit to another.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <param name="from">Source unit key (e.g. <c>celsius</c>, <c>meters</c>).</param>
    /// <param name="to">Target unit key (e.g. <c>fahrenheit</c>, <c>feet</c>).</param>
    /// <returns>The converted value together with unit metadata.</returns>
    /// <response code="200">Conversion was successful.</response>
    /// <response code="400">Unknown unit or cross-category conversion attempted.</response>
    [HttpGet("convert")]
    [ProducesResponseType(typeof(ConversionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails),     StatusCodes.Status400BadRequest)]
    public IActionResult Convert(
        [FromQuery] double value,
        [FromQuery] string from,
        [FromQuery] string to)
    {
        if (string.IsNullOrWhiteSpace(from) || string.IsNullOrWhiteSpace(to))
            return BadRequest(new ProblemDetails
            {
                Title  = "Bad Request",
                Detail = "Both 'from' and 'to' query parameters are required."
            });

        try
        {
            return Ok(_conversionService.Convert(value, from, to));
        }
        catch (ConversionException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Title  = "Conversion Error",
                Detail = ex.Message
            });
        }
    }

    /// <summary>
    /// Returns all supported conversion categories (e.g. length, temperature, weight).
    /// </summary>
    [HttpGet("categories")]
    [ProducesResponseType(typeof(IReadOnlyCollection<string>), StatusCodes.Status200OK)]
    public IActionResult GetCategories() =>
        Ok(_conversionService.GetCategories());

    /// <summary>
    /// Returns all supported units, optionally filtered to a single category.
    /// </summary>
    /// <param name="category">Optional category filter (e.g. <c>length</c>).</param>
    [HttpGet("units")]
    [ProducesResponseType(typeof(IReadOnlyCollection<UnitDto>), StatusCodes.Status200OK)]
    public IActionResult GetUnits([FromQuery] string? category = null) =>
        Ok(_conversionService.GetUnits(category));
}

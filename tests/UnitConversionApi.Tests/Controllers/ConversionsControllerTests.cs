using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using UnitConversionApi.Models;

namespace UnitConversionApi.Tests.Controllers;

public sealed class ConversionsControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ConversionsControllerTests(WebApplicationFactory<Program> factory) =>
        _client = factory.CreateClient();

    // -------------------------------------------------------------------------
    // /api/conversions/convert
    // -------------------------------------------------------------------------
    [Fact]
    public async Task Convert_ValidRequest_Returns200WithCorrectResult()
    {
        var response = await _client.GetAsync(
            "/api/conversions/convert?value=100&from=celsius&to=fahrenheit");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ConversionResponse>();
        result.Should().NotBeNull();
        result!.OutputValue.Should().BeApproximately(212.0, precision: 0.01);
        result.Category.Should().Be("temperature");
    }

    [Fact]
    public async Task Convert_UnknownUnit_Returns400()
    {
        var response = await _client.GetAsync(
            "/api/conversions/convert?value=1&from=unknown-unit&to=meters");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Convert_CrossCategoryConversion_Returns400()
    {
        var response = await _client.GetAsync(
            "/api/conversions/convert?value=1&from=meters&to=kilograms");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Convert_MissingFromParameter_Returns400()
    {
        var response = await _client.GetAsync(
            "/api/conversions/convert?value=1&to=meters");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Convert_MissingToParameter_Returns400()
    {
        var response = await _client.GetAsync(
            "/api/conversions/convert?value=1&from=meters");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // -------------------------------------------------------------------------
    // /api/conversions/categories
    // -------------------------------------------------------------------------
    [Fact]
    public async Task GetCategories_Returns200WithExpectedCategories()
    {
        var response = await _client.GetAsync("/api/conversions/categories");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var categories = await response.Content.ReadFromJsonAsync<string[]>();
        categories.Should().NotBeNullOrEmpty();
        categories.Should().Contain(new[] { "length", "temperature", "weight" });
    }

    // -------------------------------------------------------------------------
    // /api/conversions/units
    // -------------------------------------------------------------------------
    [Fact]
    public async Task GetUnits_NoFilter_ReturnsAllUnits()
    {
        var response = await _client.GetAsync("/api/conversions/units");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var units = await response.Content.ReadFromJsonAsync<UnitDto[]>();
        units.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetUnits_WithCategoryFilter_ReturnsOnlyThatCategory()
    {
        var response = await _client.GetAsync(
            "/api/conversions/units?category=temperature");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var units = await response.Content.ReadFromJsonAsync<UnitDto[]>();
        units.Should().NotBeNullOrEmpty();
        units.Should().AllSatisfy(u => u.Category.Should().Be("temperature"));
    }

    [Fact]
    public async Task GetUnits_WithUnknownCategory_ReturnsEmptyList()
    {
        var response = await _client.GetAsync(
            "/api/conversions/units?category=does-not-exist");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var units = await response.Content.ReadFromJsonAsync<UnitDto[]>();
        units.Should().BeEmpty();
    }
}

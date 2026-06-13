using Microsoft.OpenApi;
using Scalar.AspNetCore;
using System.Text.Json.Nodes;
using UnitConversionApi.Data;
using UnitConversionApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((doc, _, _) =>
    {
        doc.Info.Title       = "Unit Conversion API";
        doc.Info.Version     = "v1";
        doc.Info.Description = "A RESTful API for converting numerical values between different units of measurement.";
        return Task.CompletedTask;
    });

    // Fix the 'value' query parameter on /convert so Scalar shows it as a plain number,
    // not number | string (which is an artefact of how .NET binds query strings).
    options.AddOperationTransformer((operation, context, _) =>
    {
        if (context.Description.ActionDescriptor.RouteValues.TryGetValue("action", out var action)
            && action == "Convert")
        {
            var valueParam = operation.Parameters?.FirstOrDefault(p => p.Name == "value");
            if (valueParam is OpenApiParameter concreteParam)
            {
                concreteParam.Schema = new OpenApiSchema
                {
                    Type    = JsonSchemaType.Number,
                    Format  = "double",
                    Example = JsonValue.Create(100.0)
                };
            }
        }
        return Task.CompletedTask;
    });
});

// Domain services — UnitRegistry is singleton because unit definitions are immutable.
builder.Services.AddSingleton<UnitRegistry>();
builder.Services.AddScoped<IConversionService, ConversionService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();                               // serves /openapi/v1.json
    app.MapScalarApiReference(opts =>
    {
        opts.Title = "Unit Conversion API";
        opts.WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
    });                                             // serves /scalar/v1
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();

// Required for WebApplicationFactory in integration tests.
public partial class Program { }

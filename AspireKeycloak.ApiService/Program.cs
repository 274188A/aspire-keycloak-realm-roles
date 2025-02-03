using AspireKeycloak.ApiService;
using AspireKeycloak.ServiceDefaults;
using Microsoft.AspNetCore.Authentication;

var builder = WebApplication.CreateBuilder(args);

const string ForcasterPolicy = "Forcaster";
const string ForcasterRole = "forcaster";

// Add service defaults & Aspire components.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();

builder.Services.AddAuthentication()
                .AddKeycloakJwtBearer("keycloak", realm: "WeatherShop", options =>
                {
                    // Require HTTPS metadata only in production
                    options.RequireHttpsMetadata = builder.Environment.IsProduction();
                    options.Audience = "weather.api";
                });

builder.Services.AddTransient<IClaimsTransformation, RoleClaimsTransformation>();

// Add authorization builder and define a role-based policy
builder.Services.AddAuthorizationBuilder()
    .AddPolicy(ForcasterPolicy, policy => policy.RequireRole(ForcasterRole));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error");
}

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.RequireAuthorization(ForcasterPolicy);

app.MapDefaultEndpoints();

app.Run();

internal sealed record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AspireKeycloak.ServiceDefaults;
using AspireKeycloak.Web;
using AspireKeycloak.Web.Components;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire components.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddOutputCache();

builder.Services.AddHttpContextAccessor()
                .AddTransient<AuthorizationHandler>();

builder.Services.AddHttpClient<WeatherApiClient>(client =>
    {
        // This URL uses "https+http://" to indicate HTTPS is preferred over HTTP.
        // Learn more about service discovery scheme resolution at https://aka.ms/dotnet/sdschemes.
        client.BaseAddress = new("https+http://apiservice");
    })
    .AddHttpMessageHandler<AuthorizationHandler>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme) // Register the Cookie authentication handler
.AddKeycloakOpenIdConnect("keycloak", realm: "WeatherShop", OpenIdConnectDefaults.AuthenticationScheme, options =>
{
    options.ClientId = "WeatherWeb";
    options.ResponseType = OpenIdConnectResponseType.Code;
    options.Scope.Add("weather:all");
    options.RequireHttpsMetadata = builder.Environment.IsProduction();
    options.TokenValidationParameters.NameClaimType = JwtRegisteredClaimNames.Name;
    options.SaveTokens = true;
    options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme; // Ensure cookies are used for sign-in

    options.Events.OnTokenValidated = context =>
    {
        var identity = context.Principal?.Identity as ClaimsIdentity;
        if (identity == null)
        {
            return Task.CompletedTask;
        }

        if (context.SecurityToken is JwtSecurityToken token)
        {
            var realmAccessClaim = token.Claims.FirstOrDefault(c => c.Type == "realm_access");
            identity.AddRealmRoles(realmAccessClaim?.Value);
        }

        return Task.CompletedTask;
    };

});

builder.Services.AddCascadingAuthenticationState();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.UseOutputCache();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapDefaultEndpoints();
app.MapLoginAndLogout();

app.Run();

using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using RagPlatform.Application;
using RagPlatform.Infrastructure;
using RagPlatform.Infrastructure.Security;
using RagPlatform.Web.Mvc.Authentication;

var builder = WebApplication.CreateBuilder(args);

// --- Authentication ---
// "Authentication:UseLocalDevAuth": true (see appsettings.json) auto-signs every request in
// as a fake user, so the app runs without a real Entra ID tenant. Set it to false and fill
// in real AzureAd:* values (Instance/TenantId/ClientId) to switch to real Entra ID sign-in.
var useLocalDevAuth = builder.Configuration.GetValue<bool>("Authentication:UseLocalDevAuth");

if (useLocalDevAuth)
{
    builder.Services.AddAuthentication("DevAuth")
        .AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, DevAuthenticationHandler>("DevAuth", _ => { });
}
else
{
    builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
        .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"));
}

builder.Services.AddAuthorization(options => options.AddRagPlatformPolicies());

var mvcBuilder = builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new Microsoft.AspNetCore.Mvc.Authorization.AuthorizeFilter());
});

if (!useLocalDevAuth)
{
    // Adds the Microsoft-hosted sign-in/out Razor Pages; only meaningful with real Entra ID.
    mvcBuilder.AddMicrosoftIdentityUI();
}

// Razor Pages engine registration - required whenever MapRazorPages() is called below,
// regardless of which auth mode is active ("Unable to find the required services...
// AddRazorPages()" otherwise).
builder.Services.AddRazorPages();

builder.Services.AddApplicationInsightsTelemetry();

// --- Clean Architecture layers ---
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Documents}/{action=Index}/{id?}");
app.MapRazorPages(); // required by AddMicrosoftIdentityUI for the sign-in/out UI (real Entra ID mode)

app.Run();

